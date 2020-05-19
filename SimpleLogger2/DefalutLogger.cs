using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Timers;

namespace SimpleLogger2
{
    internal class DefalutLogger
    {
        public DefalutLogger() : this(true, true, true, true, 3000, Path.Combine(System.Environment.CurrentDirectory, "log"))
        {
        }

        public DefalutLogger(bool infoOnConsole, bool debugOnConsole, bool async, bool autoBufferResize, double maxFlushWait, string logFolderPath)
        {
            InfoOnConsole = infoOnConsole;
            DebugOnConsole = debugOnConsole;
            Async = async;
            AutoBufferResize = autoBufferResize;
            MaxFlushWait = maxFlushWait;
            LogFolderPath = logFolderPath;

            _timer.AutoReset = true;
            _timer.Elapsed += _timer_Elapsed;
            _timer.Start();
        }

        public bool InfoOnConsole { get; set; }
        public bool DebugOnConsole { get; set; }
        public bool Async { get; set; }
        public bool AutoBufferResize { get; set; }

        public double MaxFlushWait
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

        public string LogFolderPath
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

        private string _fileName;
        private string _logFolderPath;

        private TextWriter _writer;
        private object _writerCreateLock = new object();

        private int _bufferSize;
        private int _bufferCount;
        private Timer _timer = new Timer();

        public event LoggedEventHandler InfoLogged;

        public event LoggedEventHandler DebugLogged;

        public void Debug(string text)
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

                    string assemblyName = System.Reflection.Assembly.GetEntryAssembly().GetName().Name;
                    _fileName = DateTime.Now.ToString("yyyyMMdd") + "." + assemblyName + ".txt";

                    _writer = TextWriter.Synchronized(new StreamWriter(Path.Combine(_logFolderPath, _fileName), true)); // thread-safe
                }
            }

            // 파일에 적는다
            if (Async)
                _writer.WriteLineAsync(text);
            else
            {
                _writer.WriteLine(text);
                _writer.Flush();
            }

            // AutoBufferResize 로직
            if (_bufferCount > 0) _bufferSize = Math.Min(_bufferSize + 1, int.MaxValue);
            else _bufferSize = Math.Max(_bufferSize - 1, 0);

            if ((AutoBufferResize && _bufferCount >= _bufferSize) || _bufferCount == 1) // 지금 쓰는 하나 빼고는 _bufferCount 가 없을 때
                Flush();

            if (DebugOnConsole)
                Console.WriteLine(text);

            // 이벤트 발생
            DebugLogged?.Invoke(this, new LoggedEventArgs(text));
        }

        public void Info(string text)
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

                    string assemblyName = System.Reflection.Assembly.GetEntryAssembly().GetName().Name;
                    _fileName = DateTime.Now.ToString("yyyyMMdd") + "." + assemblyName + ".txt";

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

            // AutoBufferResize 로직
            if (_bufferCount > 0) _bufferSize = Math.Min(_bufferSize + 1, int.MaxValue);
            else _bufferSize = Math.Max(_bufferSize - 1, 0);

            if ((AutoBufferResize && _bufferCount >= _bufferSize) || _bufferCount == 1) // 지금 쓰는 하나 빼고는 _bufferCount 가 없을 때
                Flush();

            if (InfoOnConsole)
                Console.WriteLine(text);

            // 이벤트 발생
            InfoLogged?.Invoke(this, new LoggedEventArgs(text));
        }

        public void Flush()
        {
            if (_writer != null)
                _writer.Flush();

            _bufferCount = 0;
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Flush();
        }
    }
}