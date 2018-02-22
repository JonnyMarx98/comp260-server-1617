using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server
{
    class Player
    {
        public Dungeon dungeonRef;
        public Room currentRoom;

        public void Init()
        {
            currentRoom = dungeonRef.roomMap["Room 0"];
            Console.WriteLine("here");
        }
    }
}
