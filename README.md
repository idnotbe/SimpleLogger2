# SimpleLogger
C# intuitive / efficient log library for 90% of projects


unlike the other log libraries such as NLog or log4net, it is not overwhelming.
because it's very simple, you can modify or add the additional features.



here is an usage example.

    private static void Main(string[] args)
    {
        Console.WriteLine("--------------- START ---------------");

        Logger.Debug("Degug Test");
        Logger.Debug("Degug Test{0}", 2); // by default, automatically add the source file and the line

        Logger.Info("Info Test");
        Logger.Info("Info Test{0}", 2); // no caller info on INFO

        Logger.CallerInfo = CallerInfo.ClassMethod;

        Logger.Debug("When caller info is class and method"); // automatically add the class and the method (though lower speed)

        Logger.CallerInfo = CallerInfo.NoCallerInfo; // does not add caller information

        Logger.Debug("When there is no caller info");

        Console.WriteLine("--------------- E N D ---------------");
        Console.ReadLine();
    }


the result in the log file is...

    --------------- START ---------------
    [2020-05-07 01:11:40 DEBUG Program.cs 20] Degug Test
    [2020-05-07 01:11:40 DEBUG Program.cs 21] Degug Test2
    [2020-05-07 01:11:40 INFO] Info Test
    [2020-05-07 01:11:40 INFO] Info Test2
    [2020-05-07 01:11:40 DEBUG Program.Main] When caller info is class and method
    [2020-05-07 01:11:40 DEBUG] When there is no caller info
    --------------- E N D ---------------



basically, you need to know only 2 static methods to write log files.

    Logger.Info(string text, params object[] args);
    Logger.Debug(string text, params object[] args);



and the optional features in Logger class are...

    public static event LoggedEventHandler InfoLogged; // when info logged
    public static event LoggedEventHandler DebugLogged; // when debug logged

    public static bool InfoOnConsole { get; set; } // show the info log on console, while the default is file
    public static bool DebugOnConsole { get; set; } // show the debug log on console, while the default is file

    public static CallerInfo CallerInfo { get; set; } // can automatically add (1) the source file and the line || (2) the class and the method name

    public static bool Async { get; set; } // can log asynchronously which is 5~10 times faster than sync
    public static bool AutoBufferResize { get; set; } // intelligently resize the optimal buffer size in async mode
    public static double MaxFlushWait { get; set; } // write un-flushed logs after the max flush wait in async mode

    public static string LogFolderPath { get; set; } // the default is "log" under the current directory
    public static void Flush(); // in async mode



so, use the static methods in Logger class only - that's all.
