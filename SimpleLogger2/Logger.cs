using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace SimpleLogger2
{
    public class Logger
    {
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

        public static event LoggedEventHandler InfoLogged;

        public static event LoggedEventHandler DebugLogged;

        public static event LoggedEventHandler ErrorLogged;

        public static bool InfoOnConsole { get { return _logger.InfoOnConsole; } set { _logger.InfoOnConsole = value; } }
        public static bool DebugOnConsole { get { return _logger.DebugOnConsole; } set { _logger.DebugOnConsole = value; } }
        public static bool ErrorOnConsole { get { return _logger.ErrorOnConsole; } set { _logger.ErrorOnConsole = value; } }

        public static CallerInfo CallerInfo { get; set; } // 기본값 CallerInfo.SourceLine;

        public static bool Async { get { return _logger.Async; } set { _logger.Async = value; } }
        //public static bool AutoBufferResize { get { return _logger.AutoBufferResize; } set { _logger.AutoBufferResize = value; } }

        /// <summary>
        /// Write un-flushed logs after AutoFlushWait in async mode. Milliseconds.
        /// </summary>
        public static double AutoFlushWait { get { return _logger.AutoFlushWait; } set { _logger.AutoFlushWait = value; } }

        public static string LogFolderPath { get { return _logger.LogFolderPath; } set { _logger.LogFolderPath = value; } }

        public static void Info(string format, object o1 = null, object o2 = null, object o3 = null, object o4 = null, object o5 = null, object o6 = null, object o7 = null, object o8 = null, object o9 = null, object o10 = null)
        //[CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            if (string.IsNullOrEmpty(format))
                return;

            // no caller info in INFO
            string text = "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " INFO] " +
                    string.Format(format, o1, o2, o3, o4, o5, o6, o7, o8, o9, o10);
            _logger.Info(text);

            /*
            if (CallerInfo == CallerInfo.ClassMethod) // 퍼포먼스 떨어짐
            {
                StackFrame frame = new StackFrame(1, false);
                string text = "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " INFO " + frame.GetMethod().DeclaringType.Name + "." + frame.GetMethod().Name + "] " +
                    string.Format(format, o1, o2, o3, o4, o5, o6, o7, o8, o9, o10);
                _logger.Info(text);
            }
            else if (CallerInfo == CallerInfo.SourceLine) // 기본값
            {
                string text = "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " INFO " + sourceFilePath.Substring(sourceFilePath.LastIndexOf('\\') + 1) + " " + sourceLineNumber + "] " +
                    string.Format(format, o1, o2, o3, o4, o5, o6, o7, o8, o9, o10);
                _logger.Info(text);
            }
            else // CallerInfo.NoCallerInfo
            {
                string text = "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " INFO] " +
                    string.Format(format, o1, o2, o3, o4, o5, o6, o7, o8, o9, o10);
                _logger.Info(text);
            }
            */
        }

        public static void Debug(string format, object o1 = null, object o2 = null, object o3 = null, object o4 = null, object o5 = null, object o6 = null, object o7 = null, object o8 = null, object o9 = null, object o10 = null,
            [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            if (string.IsNullOrEmpty(format))
                return;

            if (CallerInfo == CallerInfo.ClassMethod) // 퍼포먼스 떨어짐
            {
                StackFrame frame = new StackFrame(1, false);
                string text = "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " DEBUG " + frame.GetMethod().DeclaringType.Name + "." + frame.GetMethod().Name + "] " +
                    string.Format(format, o1, o2, o3, o4, o5, o6, o7, o8, o9, o10);
                _logger.Debug(text);
            }
            else if (CallerInfo == CallerInfo.SourceLine) // 기본값
            {
                string text = "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " DEBUG " + sourceFilePath.Substring(sourceFilePath.LastIndexOf('\\') + 1) + " " + sourceLineNumber + "] " +
                    string.Format(format, o1, o2, o3, o4, o5, o6, o7, o8, o9, o10);
                _logger.Debug(text);
            }
            else // CallerInfo.NoCallerInfo
            {
                string text = "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " DEBUG] " +
                    string.Format(format, o1, o2, o3, o4, o5, o6, o7, o8, o9, o10);
                _logger.Debug(text);
            }
        }

        public static void Error(string format, object o1 = null, object o2 = null, object o3 = null, object o4 = null, object o5 = null, object o6 = null, object o7 = null, object o8 = null, object o9 = null, object o10 = null,
            [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            if (string.IsNullOrEmpty(format))
                return;

            if (CallerInfo == CallerInfo.ClassMethod) // 퍼포먼스 떨어짐
            {
                StackFrame frame = new StackFrame(1, false);
                string text = "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ERROR " + frame.GetMethod().DeclaringType.Name + "." + frame.GetMethod().Name + "] " +
                    string.Format(format, o1, o2, o3, o4, o5, o6, o7, o8, o9, o10);
                _logger.Error(text);
            }
            else if (CallerInfo == CallerInfo.SourceLine) // 기본값
            {
                string text = "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ERROR " + sourceFilePath.Substring(sourceFilePath.LastIndexOf('\\') + 1) + " " + sourceLineNumber + "] " +
                    string.Format(format, o1, o2, o3, o4, o5, o6, o7, o8, o9, o10);
                _logger.Error(text);
            }
            else // CallerInfo.NoCallerInfo
            {
                string text = "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ERROR] " +
                    string.Format(format, o1, o2, o3, o4, o5, o6, o7, o8, o9, o10);
                _logger.Error(text);
            }
        }

        public static void Flush()
        {
            _logger.Flush();
        }
    }
}