using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace SUD
{    
    class Program
    {    
        static void Main(string[] args)
        {
            Console.SetWindowSize(80, 40);

            var dungeon = new Dungeon();

            dungeon.Init();

            while (true)
            {
                dungeon.Process();                
            }
        }
    }
}
