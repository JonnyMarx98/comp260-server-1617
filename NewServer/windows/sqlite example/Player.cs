using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server
{
    public class Player
    {
        public Dungeon dungeonRef;
        public String currentRoom;
        public String playerName;
        public String clientID;

        public void Init()
        {
            currentRoom = "Room 0";
        }
    }
}
