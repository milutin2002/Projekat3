using System;
using System.Threading.Tasks;

namespace Projekat3
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            HttpServer server = new HttpServer();
            server.start();
            Console.ReadKey();
            Console.WriteLine("Ugasen je server");
        }
    }
}