using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SUD
{
    public class Dungeon
    {        
        Dictionary<String, Room> roomMap;

        Room currentRoom;

        public void Init()
        {
            roomMap = new Dictionary<string, Room>();
            {
                var room = new Room("Room 0", "You are standing in the entrance hall\nAll adventures start here");
                room.north = "Room 1";
                roomMap.Add(room.name, room);
            }

            {
                var room = new Room("Room 1", "You are in room 1");
                room.south = "Room 0";
                room.west = "Room 3";
                room.east = "Room 2";
                roomMap.Add(room.name, room);
            }

            {
                var room = new Room("Room 2", "You are in room 2");
                room.north = "Room 4";
                roomMap.Add(room.name, room);
            }

            {
                var room = new Room("Room 3", "You are in room 3");
                room.east = "Room 1";
                roomMap.Add(room.name, room);
            }

            {
                var room = new Room("Room 4", "You are in room 4");
                room.south = "Room 2";
                room.west = "Room 5";
                roomMap.Add(room.name, room);
            }

            {
                var room = new Room("Room 5", "You are in room 5");
                room.south = "Room 1";
                room.east = "Room 4";
                roomMap.Add(room.name, room);
            }

            currentRoom = roomMap["Room 0"];
        }

        public void Process()
        {
            Console.Clear();

            Console.WriteLine(currentRoom.desc);
            Console.WriteLine("Exits");
            for (var i = 0; i < currentRoom.exits.Length; i++)
            {
                if (currentRoom.exits[i] != null)
                {
                    Console.Write(Room.exitNames[i] + " ");
                }
            }

            Console.Write("\n> ");

            var key = Console.ReadLine();

            var input = key.Split(' ');

            switch (input[0].ToLower())
            {
                case "help":
                    Console.Clear();
                    Console.WriteLine("\nCommands are ....");
                    Console.WriteLine("help - for this screen");
                    Console.WriteLine("look - to look around");
                    Console.WriteLine("go [north | south | east | west]  - to travel between locations");
                    Console.WriteLine("\nPress any key to continue");
                    Console.ReadKey(true);
                    break;

                case "look":
                    //loop straight back
                    Console.Clear();
                    Thread.Sleep(1000);
                    break;

                case "say":
                    Console.Write("You say ");
                    for (var i = 1; i < input.Length; i++)
                    {
                        Console.Write(input[i] + " ");
                    }

                    Thread.Sleep(1000);
                    Console.Clear();
                    break;

                case "go":
                    // is arg[1] sensible?
                    if ((input[1].ToLower() == "north") && (currentRoom.north != null))
                    {
                        currentRoom = roomMap[currentRoom.north];
                    }
                    else
                    {
                        if ((input[1].ToLower() == "south") && (currentRoom.south != null))
                        {
                            currentRoom = roomMap[currentRoom.south];
                        }
                        else
                        {
                            if ((input[1].ToLower() == "east") && (currentRoom.east != null))
                            {
                                currentRoom = roomMap[currentRoom.east];
                            }
                            else
                            {
                                if ((input[1].ToLower() == "west") && (currentRoom.west != null))
                                {
                                    currentRoom = roomMap[currentRoom.west];
                                }
                                else
                                {
                                    //handle error
                                    Console.WriteLine("\nERROR");
                                    Console.WriteLine("\nCan not go "+ input[1]+ " from here");
                                    Console.WriteLine("\nPress any key to continue");
                                    Console.ReadKey(true);
                                }
                            }
                        }
                    }
                    break;

                default:
                    //handle error
                    Console.WriteLine("\nERROR");
                    Console.WriteLine("\nCan not " + key);
                    Console.WriteLine("\nPress any key to continue");
                    Console.ReadKey(true);
                    break;
            }

        }
    }
}
