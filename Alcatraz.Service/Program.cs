using System;
using System.IO;
using Alcatraz.Core.Server;
using log4net.Config;

namespace Alcatraz.Service
{
    class Program
    {
        static void Main(string[] args)
        {
            XmlConfigurator.ConfigureAndWatch(new FileInfo("log4net.config"));

            using(var server = new LogServer())
            {
                server.Run();
                
                Console.WriteLine("Presione una tecla para salir.");
                Console.ReadLine();
            };
        }
    }
}
