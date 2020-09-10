using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

using System.Configuration;
using System.IO;
using System.Timers;

namespace SimpleLogger2
{
    public class Logger
    {
        static Logger()
        {
            string infoOnConsoleStr = ConfigurationManager.AppSettings["InfoOnConsole"]; // 만일 해당하는 값이 App.config에 없으면 null
            if (!string.IsNullOrEmpty(infoOnConsoleStr)) InfoOnConsole = bool.Parse(infoOnConsoleStr.Trim());
            else InfoOnConsole = true; // 기본값

            string debugOnConsoleStr = ConfigurationManager.AppSettings["DebugOnConsole"]; // 만일 해당하는 값이 App.config에 없으면 null
            if (!string.IsNullOrEmpty(debugOnConsoleStr)) DebugOnConsole = bool.Parse(debugOnConsoleStr.Trim());
            else DebugOnConsole = true;

            string errorOnColsoleStr = ConfigurationManager.AppSettings["ErrorOnConsole"]; // 만일 해당하는 값이 App.config에 없으면 null
            if (!string.IsNullOrEmpty(errorOnColsoleStr)) ErrorOnConsole = bool.Parse(errorOnColsoleStr.Trim());
            else ErrorOnConsole = true;

            string asyncStr = ConfigurationManager.AppSettings["Async"]; // 만일 해당하는 값이 App.config에 없으면 null
            if (!string.IsNullOrEmpty(asyncStr)) Async = bool.Parse(asyncStr.Trim());
            else Async = true;

            string autoFlushWaitStr = ConfigurationManager.AppSettings["AutoFlushWait"]; // 만일 해당하는 값이 App.config에 없으면 null
            if (!string.IsNullOrEmpty(autoFlushWaitStr)) AutoFlushWait = double.Parse(autoFlushWaitStr.Trim());
            else AutoFlushWait = 3000;

            string logFolderPathStr = ConfigurationManager.AppSettings["LogFolderPath"];
            if (!string.IsNullOrEmpty(autoFlushWaitStr))
            {
                if (Path.IsPathRooted(logFolderPathStr)) LogFolderPath = logFolderPathStr;
                else LogFolderPath = Path.Combine(System.Environment.CurrentDirectory, logFolderPathStr);
            }
            else LogFolderPath = Path.Combine(System.Environment.CurrentDirectory, "log");

            _timer.AutoReset = true;
            _timer.Elapsed += _timer_Elapsed;
            _timer.Start();
        }

        private static void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Flush();
        }

        private static string _fileName;
        private static string _logFolderPath;

        private static TextWriter _writer;
        private static object _writerCreateLock = new object();

        private static int _bufferSize;
        private static int _bufferCount;
        private static Timer _timer = new Timer();

        /*
        static Logger()
        {
            _logger.DebugLogged += _logger_DebugLogged;
            _logger.InfoLogged += _logger_InfoLogged;
            _logger.ErrorLogged += _logger_ErrorLogged;
        }

        private static void _logger_ErrorLogged(object sender, LoggedEventArgs e)
        {
            ErrorLogged?.Invoke(sender, e);
        }

        private static void _logger_InfoLogged(object sender, LoggedEventArgs e)
        {
            InfoLogged?.Invoke(sender, e);
        }

        private static void _logger_DebugLogged(object sender, LoggedEventArgs e)
        {
            DebugLogged?.Invoke(sender, e);
        }

        private static readonly DefalutLogger _logger = new DefalutLogger();
        */

        public static event LoggedEventHandler InfoLogged;

        public static event LoggedEventHandler DebugLogged;

        public static event LoggedEventHandler ErrorLogged;

        public static bool InfoOnConsole { get; set; }
        public static bool DebugOnConsole { get; set; }
        public static bool ErrorOnConsole { get; set; }

        public static CallerInfo CallerInfo { get; set; } // 기본값 CallerInfo.SourceLine;

        public static bool Async { get; set; }

        /// <summary>
        /// Write un-flushed logs after AutoFlushWait in async mode. Milliseconds.
        /// </summary>
        public static double AutoFlushWait
        {
            get
            {
                return _timer.Interval;
            }
            set
            {
                _timer.Interval = value;
            }
        }

        public static string LogFolderPath
        {
            get
            {
                return _logFolderPath;
            }
            set
            {
                if (!Directory.Exists(value))
                    Directory.CreateDirectory(value);

                _logFolderPath = value;
            }
        }

        public static void Info(string format, object o1 = null, object o2 = null, object o3 = null, object o4 = null, object o5 = null, object o6 = null, object o7 = null, object o8 = null, object o9 = null, object o10 = null)        
        {
            if (string.IsNullOrEmpty(format))
                return;

            // no caller info in INFO
            string text = "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " INFO] " +
                    string.Format(format, o1, o2, o3, o4, o5, o6, o7, o8, o9, o10);

            // thread-safe 인 _writer 를 점유하고 있는 놈이 완전히 처리하기 전에 불리는 count 를 센다.
            _bufferCount++;

            // 새로 생기거나 날짜가 바뀌면 새로운 _streamWriter 를 할당한다.
            if (_fileName == null || !_fileName.StartsWith(DateTime.Now.ToString("yyyyMMdd")))
            {
                lock (_writerCreateLock)
                {
                    if (_writer != null)
                        _writer.Close();

                    System.Reflection.Assembly assembly = System.Reflection.Assembly.GetEntryAssembly();
                    if (assembly == null) // unmanaged code 등에서 불리면 null 이 된다.
                        assembly = System.Reflection.Assembly.GetCallingAssembly();

                    _fileName = DateTime.Now.ToString("yyyyMMdd") + "." + assembly.GetName().Name + ".txt";

                    _writer = TextWriter.Synchronized(new StreamWriter(Path.Combine(_logFolderPath, _fileName), true)); // thread-safe
                }
            }

            // 쓴다.
            if (Async)
                _writer.WriteLineAsync(text);
            else
            {
                _writer.WriteLine(text);
                _writer.Flush();
            }

            // 버퍼에 쌓여있는 양을 처리하고 나면 동일한 양의 로그가 또 쌓여있는 상황을 이상적인 _bufferSize 로 본다.
            if (_bufferCount > _bufferSize) _bufferSize = Math.Min(_bufferSize + 1, int.MaxValue);
            else _bufferSize = Math.Max(_bufferSize - 1, 0);

            if (_bufferCount >= _bufferSize)
                Flush();

            if (InfoOnConsole)
                Console.WriteLine(text);

            InfoLogged?.Invoke(null, new LoggedEventArgs(text));
        }

        public static void Debug(string format, object o1 = null, object o2 = null, object o3 = null, object o4 = null, object o5 = null, object o6 = null, object o7 = null, object o8 = null, object o9 = null, object o10 = null,
            [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            if (string.IsNullOrEmpty(format))
                return;

            string text = null;
            if (CallerInfo == CallerInfo.ClassMethod) // 퍼포먼스 떨어짐
            {
                StackFrame frame = new StackFrame(1, false);
                text = "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " DEBUG " + frame.GetMethod().DeclaringType.Name + "." + frame.GetMethod().Name + "] " +
                    string.Format(format, o1, o2, o3, o4, o5, o6, o7, o8, o9, o10);
            }
            else if (CallerInfo == CallerInfo.SourceLine) // 기본값
            {
                text = "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " DEBUG " + sourceFilePath.Substring(sourceFilePath.LastIndexOf('\\') + 1) + " " + sourceLineNumber + "] " +
                    string.Format(format, o1, o2, o3, o4, o5, o6, o7, o8, o9, o10);
            }
            else // CallerInfo.NoCallerInfo
            {
                text = "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " DEBUG] " +
                    string.Format(format, o1, o2, o3, o4, o5, o6, o7, o8, o9, o10);
            }

            // thread-safe 인 _writer 를 점유하고 있는 놈이 완전히 처리하기 전에 불리는 count 를 센다.
            _bufferCount++;

            // 새로 생기거나 날짜가 바뀌면 새로운 _streamWriter 를 할당한다.
            if (_fileName == null || !_fileName.StartsWith(DateTime.Now.ToString("yyyyMMdd")))
            {
                lock (_writerCreateLock)
                {
                    if (_writer != null)
                        _writer.Close();

                    System.Reflection.Assembly assembly = System.Reflection.Assembly.GetEntryAssembly();
                    if (assembly == null) // unmanaged code 등에서 불리면 null 이 된다.
                        assembly = System.Reflection.Assembly.GetCallingAssembly();

                    _fileName = DateTime.Now.ToString("yyyyMMdd") + "." + assembly.GetName().Name + ".txt";

                    _writer = TextWriter.Synchronized(new StreamWriter(Path.Combine(_logFolderPath, _fileName), true)); // thread-safe
                }
            }

            // 쓴다.
            if (Async)
                _writer.WriteLineAsync(text);
            else
            {
                _writer.WriteLine(text);
                _writer.Flush();
            }

            // 버퍼에 쌓여있는 양을 처리하고 나면 동일한 양의 로그가 또 쌓여있는 상황을 이상적인 _bufferSize 로 본다.
            if (_bufferCount > _bufferSize) _bufferSize = Math.Min(_bufferSize + 1, int.MaxValue);
            else _bufferSize = Math.Max(_bufferSize - 1, 0);

            if (_bufferCount >= _bufferSize)
                Flush();

            if (DebugOnConsole)
                Console.WriteLine(text);

            DebugLogged?.Invoke(null, new LoggedEventArgs(text));
        }

        public static void Error(string format, object o1 = null, object o2 = null, object o3 = null, object o4 = null, object o5 = null, object o6 = null, object o7 = null, object o8 = null, object o9 = null, object o10 = null,
            [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            if (string.IsNullOrEmpty(format))
                return;

            string text = null;
            if (CallerInfo == CallerInfo.ClassMethod) // 퍼포먼스 떨어짐
            {
                StackFrame frame = new StackFrame(1, false);
                text = "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ERROR " + frame.GetMethod().DeclaringType.Name + "." + frame.GetMethod().Name + "] " +
                    string.Format(format, o1, o2, o3, o4, o5, o6, o7, o8, o9, o10);
            }
            else if (CallerInfo == CallerInfo.SourceLine) // 기본값
            {
                text = "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ERROR " + sourceFilePath.Substring(sourceFilePath.LastIndexOf('\\') + 1) + " " + sourceLineNumber + "] " +
                    string.Format(format, o1, o2, o3, o4, o5, o6, o7, o8, o9, o10);
            }
            else // CallerInfo.NoCallerInfo
            {
                text = "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ERROR] " +
                    string.Format(format, o1, o2, o3, o4, o5, o6, o7, o8, o9, o10);                
            }

            // thread-safe 인 _writer 를 점유하고 있는 놈이 완전히 처리하기 전에 불리는 count 를 센다.
            _bufferCount++;

            // 새로 생기거나 날짜가 바뀌면 새로운 _streamWriter 를 할당한다.
            if (_fileName == null || !_fileName.StartsWith(DateTime.Now.ToString("yyyyMMdd")))
            {
                lock (_writerCreateLock)
                {
                    if (_writer != null)
                        _writer.Close();

                    System.Reflection.Assembly assembly = System.Reflection.Assembly.GetEntryAssembly();
                    if (assembly == null) // unmanaged code 등에서 불리면 null 이 된다.
                        assembly = System.Reflection.Assembly.GetCallingAssembly();

                    _fileName = DateTime.Now.ToString("yyyyMMdd") + "." + assembly.GetName().Name + ".txt";

                    _writer = TextWriter.Synchronized(new StreamWriter(Path.Combine(_logFolderPath, _fileName), true)); // thread-safe
                }
            }

            // 쓴다.
            if (Async)
                _writer.WriteLineAsync(text);
            else
            {
                _writer.WriteLine(text);
                _writer.Flush();
            }

            // 버퍼에 쌓여있는 양을 처리하고 나면 동일한 양의 로그가 또 쌓여있는 상황을 이상적인 _bufferSize 로 본다.
            if (_bufferCount > _bufferSize) _bufferSize = Math.Min(_bufferSize + 1, int.MaxValue);
            else _bufferSize = Math.Max(_bufferSize - 1, 0);

            if (_bufferCount >= _bufferSize)
                Flush();

            if (ErrorOnConsole)
                Console.WriteLine(text);

            ErrorLogged?.Invoke(null, new LoggedEventArgs(text));
        }

        public static void Error(Exception ex, [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            // caller info 를 제대로 찍기 위해서는
            // Error(string format, object o1 = null, object o2...) 를 부르지 말고 여기서 직접 처리해야 한다.

            //if (string.IsNullOrEmpty(format))
            //    return;

            string exceptionStr = ex.ToString();

            string text = null;
            if (CallerInfo == CallerInfo.ClassMethod) // 퍼포먼스 떨어짐
            {
                StackFrame frame = new StackFrame(1, false);
                text = "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ERROR " + frame.GetMethod().DeclaringType.Name + "." + frame.GetMethod().Name + "] " +
                    exceptionStr;
            }
            else if (CallerInfo == CallerInfo.SourceLine) // 기본값
            {
                text = "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ERROR " + sourceFilePath.Substring(sourceFilePath.LastIndexOf('\\') + 1) + " " + sourceLineNumber + "] " +
                    exceptionStr;
            }
            else // CallerInfo.NoCallerInfo
            {
                text = "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ERROR] " +
                    exceptionStr;
            }

            // thread-safe 인 _writer 를 점유하고 있는 놈이 완전히 처리하기 전에 불리는 count 를 센다.
            _bufferCount++;

            // 새로 생기거나 날짜가 바뀌면 새로운 _streamWriter 를 할당한다.
            if (_fileName == null || !_fileName.StartsWith(DateTime.Now.ToString("yyyyMMdd")))
            {
                lock (_writerCreateLock)
                {
                    if (_writer != null)
                        _writer.Close();

                    System.Reflection.Assembly assembly = System.Reflection.Assembly.GetEntryAssembly();
                    if (assembly == null) // unmanaged code 등에서 불리면 null 이 된다.
                        assembly = System.Reflection.Assembly.GetCallingAssembly();

                    _fileName = DateTime.Now.ToString("yyyyMMdd") + "." + assembly.GetName().Name + ".txt";

                    _writer = TextWriter.Synchronized(new StreamWriter(Path.Combine(_logFolderPath, _fileName), true)); // thread-safe
                }
            }

            // 쓴다.
            if (Async)
                _writer.WriteLineAsync(text);
            else
            {
                _writer.WriteLine(text);
                _writer.Flush();
            }

            // 버퍼에 쌓여있는 양을 처리하고 나면 동일한 양의 로그가 또 쌓여있는 상황을 이상적인 _bufferSize 로 본다.
            if (_bufferCount > _bufferSize) _bufferSize = Math.Min(_bufferSize + 1, int.MaxValue);
            else _bufferSize = Math.Max(_bufferSize - 1, 0);

            if (_bufferCount >= _bufferSize)
                Flush();

            if (ErrorOnConsole)
                Console.WriteLine(text);

            ErrorLogged?.Invoke(null, new LoggedEventArgs(text));
        }

        public static void Flush()
        {
            if (_writer != null)
                _writer.Flush();

            _bufferCount = 0;
        }

        /*
        private static void Write(string text)
        {
            // thread-safe 인 _writer 를 점유하고 있는 놈이 완전히 처리하기 전에 불리는 count 를 센다.
            _bufferCount++;

            // 새로 생기거나 날짜가 바뀌면 새로운 _streamWriter 를 할당한다.
            if (_fileName == null || !_fileName.StartsWith(DateTime.Now.ToString("yyyyMMdd")))
            {
                lock (_writerCreateLock)
                {
                    if (_writer != null)
                        _writer.Close();

                    System.Reflection.Assembly assembly = System.Reflection.Assembly.GetEntryAssembly();
                    if (assembly == null) // unmanaged code 등에서 불리면 null 이 된다.
                        assembly = System.Reflection.Assembly.GetCallingAssembly();

                    _fileName = DateTime.Now.ToString("yyyyMMdd") + "." + assembly.GetName().Name + ".txt";

                    _writer = TextWriter.Synchronized(new StreamWriter(Path.Combine(_logFolderPath, _fileName), true)); // thread-safe
                }
            }

            // 쓴다.
            if (Async)
                _writer.WriteLineAsync(text);
            else
            {
                _writer.WriteLine(text);
                _writer.Flush();
            }

            // 버퍼에 쌓여있는 양을 처리하고 나면 동일한 양의 로그가 또 쌓여있는 상황을 이상적인 _bufferSize 로 본다.
            if (_bufferCount > _bufferSize) _bufferSize = Math.Min(_bufferSize + 1, int.MaxValue);
            else _bufferSize = Math.Max(_bufferSize - 1, 0);

            if (_bufferCount >= _bufferSize)
                Flush();
        }
        */
    }
}