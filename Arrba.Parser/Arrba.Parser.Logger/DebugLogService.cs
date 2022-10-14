using System;

namespace Arrba.Parser.Logger
{
    public class DebugLogService : ILogService
    {
        public void Debug(string msg)
        {
            System.Diagnostics.Debug.WriteLine($"Debug: {msg}");
        }

        public void Debug(string msg, Exception ex)
        {
            do
            {
                System.Diagnostics.Debug.WriteLine($"Debug: {msg}, stackTrace: {ex.StackTrace}");
                ex = ex.InnerException;
            }
            while (ex != null);
        }

        public void Error(string msg)
        {
            System.Diagnostics.Debug.WriteLine($"Error: {msg}");
        }

        public void Error(string msg, Exception ex)
        {
            do
            {
                System.Diagnostics.Debug.WriteLine($"Error: {msg}, stackTrace:  {ex.StackTrace}");

                ex = ex.InnerException;
            }
            while (ex != null);
        }

        public void Info(string msg)
        {
            System.Diagnostics.Debug.WriteLine($"Info: {msg}");
        }

        public void Info(string msg, Exception ex)
        {
            do
            {
                System.Diagnostics.Debug.WriteLine($"Info: {msg}, stackTrace:  {ex.StackTrace}");
                ex = ex.InnerException;
            }
            while (ex != null);
        }

        public void Warn(string msg)
        {
            System.Diagnostics.Debug.WriteLine($"Warn: {msg}");
        }

        public void Warn(string msg, Exception ex)
        {
            do
            {
                System.Diagnostics.Debug.WriteLine($"Warn: {msg}, stackTrace:  {ex.StackTrace}");
                ex = ex.InnerException;
            }
            while (ex != null);
        }
    }
}
