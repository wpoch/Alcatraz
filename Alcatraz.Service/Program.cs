using System;
using System.IO;
using log4net.Config;

namespace Alcatraz.Service
{
    class Program
    {
        static void Main(string[] args)
        {
            XmlConfigurator.ConfigureAndWatch(new FileInfo("log4net.config"));

            new LogServer().Run();

            Console.WriteLine("Presione una tecla para salir.");
            Console.ReadLine();
        }
    }
}
