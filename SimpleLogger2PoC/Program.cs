using SimpleLogger2;
using System;

namespace SimpleLogger2PoC
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("--------------- START ---------------");

            for (int i = 0; i < 10000; i++)
            {
                Logger.Debug("Degug Test");
                Logger.Debug("Degug Test{0}", i); // by default, automatically add the source file and the line
            }

            Logger.Info("Info Test");
            Logger.Info("Info Test{0}", 2); // no caller info on INFO

            Logger.Error("Error Test");
            Logger.Error("Error Test{0}", 3); // by default, automatically add the source file and the line

            Logger.CallerInfo = CallerInfo.ClassMethod;

            Logger.Debug("When caller info is class and method"); // automatically add the class and the method (though lower speed)

            Logger.CallerInfo = CallerInfo.NoCallerInfo; // does not add caller information

            Logger.Debug("When there is no caller info");

            Console.WriteLine("--------------- E N D ---------------");
            Console.ReadLine();
        }
    }
}