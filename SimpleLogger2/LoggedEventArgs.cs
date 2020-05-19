using System;

namespace SimpleLogger2
{
    public class LoggedEventArgs : EventArgs
    {
        public string Text { get; }

        public LoggedEventArgs(string text)
        {
            this.Text = text;
        }
    }
}