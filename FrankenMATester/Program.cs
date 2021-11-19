using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FrankenMAGateway;

namespace FrankenMATester
{
    class Program
    {
        static void Main(string[] args)
        {
            var gateway = new Gateway();
            gateway.Start();
            Console.ReadKey();
        }
    }
}
