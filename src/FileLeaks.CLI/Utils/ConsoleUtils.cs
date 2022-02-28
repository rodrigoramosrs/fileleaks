using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileLeaks.Utils
{
    public class ConsoleUtils
    {
        private static IAnsiConsole _AnsiConsole;
        public static void SetAnsiConsoleInstance(IAnsiConsole AnsiConsole)
        {
            _AnsiConsole = AnsiConsole;
        }

        public static void MarkupLine(string value)
        {
            try
            {
                _AnsiConsole.MarkupLine(value);
            }
            catch (Exception)
            {

                throw;
            }
            
        }

        public static void WriteLine(string value)
        {
            _AnsiConsole.WriteLine(value);
        }

        public static void Write(string value)
        {
            _AnsiConsole.Write(value);
        }
    }
}
