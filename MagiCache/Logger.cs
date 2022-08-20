using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
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

        public static void Log(APIRequest request, int statusCode)
        {
            var s = $"{request.pathedUrl} - {request.method} - {request.type} - {statusCode} - {DateTime.Now.ToString()}";
            _logs.Enqueue(s);
            Console.WriteLine(s);
        }

        public static void OutputLog()
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
        }

        #endregion Methods
    }
}