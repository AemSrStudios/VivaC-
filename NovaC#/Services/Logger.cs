using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace VivaCSharp.Services
{
    public static class Logger
    {
        private static readonly object Sync = new();

        private static string? _runtimeFolder;
        private static string? _projectFolder;

        public static string RuntimeCrashFolder =>
            _runtimeFolder ??
            Path.Combine(
                AppContext.BaseDirectory,
                "CrashLogs");

        public static string ProjectCrashFolder =>
            _projectFolder ??
            Path.Combine(
                Directory.GetCurrentDirectory(),
                "CrashLogs");

        public static void Initialize()
        {
            lock (Sync)
            {
                try
                {
                    _runtimeFolder = Path.Combine(
                        AppContext.BaseDirectory,
                        "CrashLogs");

                    _projectFolder = FindProjectFolder();

                    CreateFolders(_runtimeFolder);

                    if (!string.IsNullOrWhiteSpace(_projectFolder))
                    {
                        CreateFolders(
                            Path.Combine(
                                _projectFolder,
                                "CrashLogs"));
                    }

                    Log("==================================================");
                    Log("VivaC# LOGGER INITIALIZED");
                    Log("==================================================");
                    Log("Time: " + DateTime.Now);
                    Log("Process: " + Process.GetCurrentProcess().ProcessName);
                    Log("Base Directory: " + AppContext.BaseDirectory);
                    Log("Current Directory: " + Directory.GetCurrentDirectory());
                    Log("OS: " + Environment.OSVersion);
                    Log(".NET: " + Environment.Version);
                    Log("==================================================");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(
                        "Logger initialization failed: " +
                        ex);
                }
            }
        }

        public static void Log(string message)
        {
            lock (Sync)
            {
                try
                {
                    string folder =
                        _runtimeFolder ??
                        Path.Combine(
                            AppContext.BaseDirectory,
                            "CrashLogs");

                    CreateFolders(folder);

                    string logFile = Path.Combine(
                        folder,
                        "Logs",
                        $"VivaCSharp_{DateTime.Now:yyyy-MM-dd}.log");

                    string entry =
                        $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] " +
                        message +
                        Environment.NewLine;

                    File.AppendAllText(
                        logFile,
                        entry,
                        Encoding.UTF8);

                    CopyToProject(
                        "Logs",
                        Path.GetFileName(logFile),
                        entry,
                        append: true);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(
                        "Logger.Log failed: " +
                        ex);
                }
            }
        }

        public static void Error(
            string type,
            Exception exception)
        {
            lock (Sync)
            {
                try
                {
                    string folder =
                        _runtimeFolder ??
                        Path.Combine(
                            AppContext.BaseDirectory,
                            "CrashLogs");

                    CreateFolders(folder);

                    string fileName =
                        $"ERROR_{DateTime.Now:yyyy-MM-dd_HH-mm-ss-fff}.log";

                    string errorFile = Path.Combine(
                        folder,
                        "Errors",
                        fileName);

                    string report =
                        BuildExceptionReport(
                            type,
                            exception);

                    File.WriteAllText(
                        errorFile,
                        report,
                        Encoding.UTF8);

                    CopyToProject(
                        "Errors",
                        fileName,
                        report,
                        append: false);

                    Log(
                        "CRASH REPORT SAVED: " +
                        errorFile);

                    Log(
                        "ERROR TYPE: " +
                        type);

                    Log(
                        "EXCEPTION: " +
                        exception.GetType().FullName);

                    Log(
                        "MESSAGE: " +
                        exception.Message);

                    if (exception.InnerException != null)
                    {
                        Log(
                            "INNER EXCEPTION: " +
                            exception.InnerException.GetType().FullName);

                        Log(
                            "INNER MESSAGE: " +
                            exception.InnerException.Message);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(
                        "Logger.Error failed: " +
                        ex);
                }
            }
        }

        private static string BuildExceptionReport(
            string type,
            Exception exception)
        {
            var builder = new StringBuilder();

            builder.AppendLine(
                "==================================================");

            builder.AppendLine(
                "VivaC# CRASH REPORT");

            builder.AppendLine(
                "==================================================");

            builder.AppendLine();

            builder.AppendLine(
                "TIME: " +
                DateTime.Now.ToString(
                    "yyyy-MM-dd HH:mm:ss.fff"));

            builder.AppendLine();

            builder.AppendLine(
                "ERROR TYPE:");

            builder.AppendLine(type);

            builder.AppendLine();

            builder.AppendLine(
                "BASE DIRECTORY:");

            builder.AppendLine(
                AppContext.BaseDirectory);

            builder.AppendLine();

            builder.AppendLine(
                "CURRENT DIRECTORY:");

            builder.AppendLine(
                Directory.GetCurrentDirectory());

            builder.AppendLine();

            builder.AppendLine(
                "==================================================");

            builder.AppendLine(
                "EXCEPTION CHAIN");

            builder.AppendLine(
                "==================================================");

            Exception? current = exception;

            int level = 0;

            while (current != null)
            {
                builder.AppendLine();

                builder.AppendLine(
                    $"--- Exception Level {level} ---");

                builder.AppendLine();

                builder.AppendLine(
                    "Type:");

                builder.AppendLine(
                    current.GetType().FullName ??
                    "(unknown)");

                builder.AppendLine();

                builder.AppendLine(
                    "Message:");

                builder.AppendLine(
                    current.Message);

                builder.AppendLine();

                builder.AppendLine(
                    "HResult:");

                builder.AppendLine(
                    "0x" +
                    current.HResult.ToString("X8"));

                builder.AppendLine();

                builder.AppendLine(
                    "Stack Trace:");

                builder.AppendLine(
                    current.StackTrace ??
                    "(none)");

                current = current.InnerException;

                level++;
            }

            builder.AppendLine();

            builder.AppendLine(
                "==================================================");

            builder.AppendLine(
                "END CRASH REPORT");

            builder.AppendLine(
                "==================================================");

            return builder.ToString();
        }

        private static void CreateFolders(
            string crashFolder)
        {
            Directory.CreateDirectory(
                crashFolder);

            Directory.CreateDirectory(
                Path.Combine(
                    crashFolder,
                    "Logs"));

            Directory.CreateDirectory(
                Path.Combine(
                    crashFolder,
                    "Errors"));
        }

        private static string? FindProjectFolder()
        {
            try
            {
                DirectoryInfo? directory =
                    new DirectoryInfo(
                        AppContext.BaseDirectory);

                while (directory != null)
                {
                    string project =
                        Path.Combine(
                            directory.FullName,
                            "NovaC#.csproj");

                    if (File.Exists(project))
                    {
                        return directory.FullName;
                    }

                    directory = directory.Parent;
                }
            }
            catch
            {
            }

            return null;
        }

        private static void CopyToProject(
            string folder,
            string fileName,
            string content,
            bool append)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(
                    _projectFolder))
                {
                    return;
                }

                string projectCrash =
                    Path.Combine(
                        _projectFolder,
                        "CrashLogs");

                CreateFolders(projectCrash);

                string destination =
                    Path.Combine(
                        projectCrash,
                        folder,
                        fileName);

                if (append)
                {
                    File.AppendAllText(
                        destination,
                        content,
                        Encoding.UTF8);
                }
                else
                {
                    File.WriteAllText(
                        destination,
                        content,
                        Encoding.UTF8);
                }
            }
            catch
            {
            }
        }
    }
}
