using MagiCache;
using System;

namespace MyApp // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        #region Methods

        private static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            Inbound.Init();

            while (true)
            {
                System.Threading.Thread.Sleep(10000);
                Logger.OutputLog();
            }

            Console.ReadLine();
        }

        #endregion Methods
    }
}