using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MagiCache
{
    public static class Logger
    {
        #region Fields

        private static ConcurrentQueue<string> _logs = new ConcurrentQueue<string>();
        private static DateTime initDate = DateTime.Now;

        #endregion Fields

        #region Methods

        public static string[] GetLog()
        {
            List<string> logs = new List<string>();

            while (_logs.TryDequeue(out var l))
            {
                logs.Add(l);
            }

            if (Debugger.IsAttached)
            {
                File.AppendAllLines($"{initDate.ToString().Replace("/", "-").Replace(":", "-")}.log", logs);
            }

            return logs.ToArray();
        }

        public static void Log(APIRequest request, int statusCode, string res_body)
        {
            var s = $"{request.pathedUrl} - {request.method} - {request.type} - {statusCode} - {DateTime.Now.ToString()}\r\n{request.data}";
            _logs.Enqueue(s);
            Console.WriteLine(s);
        }

        #endregion Methods
    }
}