using System;
using System.Collections.Generic;
using System.Text;

namespace Arrba.Parser.Logger
{
    public interface ILogService
    {
        void Debug(string msg);
        void Debug(string msg, Exception ex);
        void Error(string msg);
        void Error(string msg, Exception ex);
        void Info(string msg);
        void Info(string msg, Exception ex);
        void Warn(string msg);
        void Warn(string msg, Exception ex);
    }
}
