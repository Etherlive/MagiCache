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
                Console.ReadLine();
            }
        }

        #endregion Methods
    }
}