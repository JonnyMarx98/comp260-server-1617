using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

#if TARGET_LINUX
using Mono.Data.Sqlite;
using sqliteConnection 	=Mono.Data.Sqlite.SqliteConnection;
using sqliteCommand 	=Mono.Data.Sqlite.SqliteCommand;
using sqliteDataReader	=Mono.Data.Sqlite.SqliteDataReader;
#endif

#if TARGET_WINDOWS
using System.Data.SQLite;
using sqliteConnection = System.Data.SQLite.SQLiteConnection;
using sqliteCommand = System.Data.SQLite.SQLiteCommand;
using sqliteDataReader = System.Data.SQLite.SQLiteDataReader;
#endif

namespace Server
{
    public class Dungeon
    {
        //public Dictionary<String, Room> roomMap;

        sqliteConnection conn = null;
        string databaseName = "data.database";

        string currentRoom;

        public void Init()
        {
            var roomMap = new Dictionary<string, Room>();
            {
                var room = new Room("Room 0", "You are standing in the entrance hall\nAll adventures start here");
                room.north = "Room 1";
                roomMap.Add(room.name, room);
            }

            {
                var room = new Room("Room 1", "You are in room 1");
                room.south = "Room 0";
                room.east = "Room 2";
                roomMap.Add(room.name, room);
            }

            {
                var room = new Room("Room 2", "You are in room 2");
                room.north = "Room 3";
                roomMap.Add(room.name, room);
            }

            {
                var room = new Room("Room 3", "You are in room 3");
                room.west = "Room 4";
                roomMap.Add(room.name, room);
            }

            {
                var room = new Room("Room 4", "You are in room 4");
                room.east = "Room 3";
                room.west = "Room 5";
                roomMap.Add(room.name, room);
            }

            {
                var room = new Room("Room 5", "You are in room 5");
                room.north = "Room 8";
                room.east = "Room 4";
                room.south = "Room 6";
                room.west = "Room 7";
                roomMap.Add(room.name, room);
            }

            {
                var room = new Room("Room 6", "You are in room 6");
                room.north = "Room 5";
                room.south = "Room 11";
                roomMap.Add(room.name, room);
            }

            {
                var room = new Room("Room 7", "You are in room 7");
                room.east = "Room 5";
                room.west = "Room 12";
                roomMap.Add(room.name, room);
            }

            {
                var room = new Room("Room 8", "You are in room 8");
                room.east = "Room 10";
                room.south = "Room 5";
                room.west = "Room 9";
                roomMap.Add(room.name, room);
            }

            {
                var room = new Room("Room 9", "You are in room 9, lol you are trapped");
                roomMap.Add(room.name, room);
            }

            {
                var room = new Room("Room 10", "You are in room 10");
                room.north = "Room 15";
                room.east = "Room 16";
                room.south = "Room 4";
                room.west = "Room 8";
                roomMap.Add(room.name, room);
            }

            {
                var room = new Room("Room 11", "You are in room 11");
                room.north = "Room 6";
                room.east = "Room 0";
                roomMap.Add(room.name, room);
            }

            {
                var room = new Room("Room 12", "You are in room 12");
                room.east = "Room 7";
                room.west = "Room 13";
                roomMap.Add(room.name, room);
            }

            {
                var room = new Room("Room 13", "You are in room 13");
                room.east = "Room 12";
                room.west = "Room 14";
                roomMap.Add(room.name, room);
            }

            {
                var room = new Room("Room 14", "You are in room 14");
                room.east = "Room 13";
                roomMap.Add(room.name, room);
            }

            {
                var room = new Room("Room 15", "You are in room 15");
                room.east = "Room 21";
                room.south = "Room 10";
                roomMap.Add(room.name, room);
            }

            {
                var room = new Room("Room 16", "You are in room 16");
                room.east = "Room 17";
                room.west = "Room 10";
                roomMap.Add(room.name, room);
            }

            {
                var room = new Room("Room 17", "You are in room 17");
                room.north = "Room 18";
                room.west = "Room 16";
                roomMap.Add(room.name, room);
            }

            {
                var room = new Room("Room 18", "You are in room 18");
                room.north = "Room 19";
                room.south = "Room 17";
                roomMap.Add(room.name, room);
            }

            {
                var room = new Room("Room 19", "You are in room 19");
                room.west = "Room 20";
                room.south = "Room 18";
                roomMap.Add(room.name, room);
            }

            {
                var room = new Room("Room 20", "You are in room 20");
                room.west = "Room 22";
                room.south = "Room 21";
                roomMap.Add(room.name, room);
            }

            {
                var room = new Room("Room 21", "You are in room 21");
                room.west = "Room 15";
                roomMap.Add(room.name, room);
            }

            {
                var room = new Room("Room 22", "You teleported to the Secret Room");
                room.north = "Room 0";
                roomMap.Add(room.name, room);
            }

            //currentRoom = roomMap["Room 0"];
            try
            {
                sqliteConnection.CreateFile(databaseName);

                conn = new sqliteConnection("Data Source=" + databaseName + ";Version=3;FailIfMissing=True");

                sqliteCommand command;

                conn.Open();

                command = new sqliteCommand("create table table_rooms (name varchar(2000), desc varchar(2000), north varchar(200), south varchar(200), west varchar(200), east varchar(200))", conn);
                command.ExecuteNonQuery();

                foreach (var kvp in roomMap)
                {
                    try
                    {
                        var sql = "insert into " + "table_rooms" + " (name, desc, north, south, west, east) values ";
                        sql += "('" + kvp.Key + "'";
                        sql += ",";
                        sql += "'" + kvp.Value.desc + "'";
                        sql += ",";
                        sql += "'" + kvp.Value.north + "'";
                        sql += ",";
                        sql += "'" + kvp.Value.south + "'";
                        sql += ",";
                        sql += "'" + kvp.Value.west + "'";
                        sql += ",";
                        sql += "'" + kvp.Value.east + "'";
                        sql += ")";

                        command = new sqliteCommand(sql, conn);
                        command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Failed to add room\n" + ex);
                    }
                }

                //command = new SQLiteCommand("drop table table_phonenumbers", conn);
                try
                {
                    Console.WriteLine("");
                    command = new sqliteCommand("select * from " + "table_rooms" + " order by name asc", conn);
                    var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        Console.WriteLine("Name: " + reader["name"] + " " + "Exits: "+ reader["north"] + " " + 
                            reader["south"] + " " + reader["west"] + " " + reader["east"]);
                    }

                    reader.Close();
                    Console.WriteLine("");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to display DB " + ex);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Create DB failed: " + ex);
            }

            currentRoom = "Room 0";

        }

        public void roomUpdate(Player currentPlayer, string previousRoom)
        {
            foreach (Player player in server.PlayerList)
            {
                if (player.currentRoom == previousRoom)
                {
                    server.roomUpdate(player, currentPlayer, false);
                }
            }
        }

        public String DungeonInfo(Player currentPlayer, bool enteredNewRoom)
        {
            String info = "";
            currentRoom = currentPlayer.currentRoom;

            sqliteCommand command;

            command = new sqliteCommand("select * from  table_rooms where name == '" + currentRoom + "'", conn);
            var reader = command.ExecuteReader();
            bool newPlayer = false;

            while (reader.Read())
            {
                String[] temp = { "north", "south", "east", "west" };
               
                if (enteredNewRoom)
                {
                    //currentRoom.PlayerCount += 1;
                    newPlayer = true;
                    info += reader["desc"];
                    info += "\nExits\n";

                    for (var i = 0; i < temp.Length; i++)
                    {
                        string result = reader[temp[i]] as String;
                        if (result != "")
                        {
                            info += (reader[temp[i]] + " " + temp[i] + "\n");
                        }
                    }
                }
            }

            info += ("\n> ");

            int Players = 1;

            foreach (Player player in server.PlayerList)
            {

                if (currentPlayer.currentRoom == player.currentRoom)
                {
                    if (player != currentPlayer)
                    {

                        Players++;
                        if (Players == 2)
                        {
                            info += "\nOther players here ->";
                            info += " [" + player.playerName + "]";
                            if (newPlayer)
                            {
                                server.roomUpdate(player, currentPlayer, true);

                            }
                        }
                        else
                        {
                            info += " [" + player.playerName + "]";
                            if (newPlayer)
                            {
                                server.roomUpdate(player, currentPlayer, true);
                            }
                        }
                    }
                }
            }

            if (Players == 1) { info += "\nYou're alone!\n"; }
            else { info+= "\n"; }

            return info;

        }

        public string Process(string Key, Player player, int PlayerID)
        {
            currentRoom = player.currentRoom; // Sets current room to the players current room
            var command = new sqliteCommand("select * from  table_rooms where name == '" + currentRoom + "'", conn);
            var reader = command.ExecuteReader();
            DungeonInfo(player, false); // Displays the dungeon info to player
            String returnString = ""; 
            var input = Key.Split(' ');

            switch (input[0].ToLower())
            {
                case "help":
                    Console.Clear();
                    returnString += ("\nCommands are ....");
                    returnString += ("\nhelp - for this screen");
                    returnString += ("\nlook - to look around");
                    returnString += ("\ngo [north | south | east | west]  - to travel between locations");
                    returnString += ("\nPress any key to continue");
                    returnString += ("\nname - to set name your name");
                    returnString += ("\nsay - global chat");
                    returnString += ("\nlocal - local chat");
                    // returnString += ("\n" + currentRoom.desc);
                    returnString += ("\nExits");

                    //returnString += DungeonInfo(player);
                    return returnString;

                case "name":
                    String newName = "";
                    for (var i = 1; i < input.Length; i++)
                    {
                        newName += (input[i]);
                    }
                    player.playerName = (newName);
                    returnString += ("\nYou set your name to --> " + newName);
                    return returnString;

                case "look":
                    //Console.Clear();
                    Thread.Sleep(500);
                    returnString = DungeonInfo(player, false);
                    return returnString;

                case "local":
                    returnString += ("[local][" + player.playerName + "]");
                    for (var i = 1; i < input.Length; i++)
                    {
                        returnString += (input[i] + " ");
                    }

                    Thread.Sleep(1000);
                    //returnString += DungeonInfo(player);
                    return returnString;

                case "say":
                    //returnString += ("[Player " + PlayerID + "]");
                    returnString += ("[global][" + player.playerName + "]");
                    for (var i = 1; i < input.Length; i++)
                    {
                        returnString += (input[i] + " ");
                    }

                    Thread.Sleep(1000);
                    //returnString += DungeonInfo(player);
                    return returnString;

                case "go":

                    //var command = new sqliteCommand("select * from  table_rooms where name == '" + currentRoom + "'", conn);
                    //var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        //returnString += ("Name: " + reader["name"] + "\tdesc: " + reader["desc"]); returnString += "\n";
                        //returnString += (reader["desc"]); returnString += "\n";
                        //returnString += ("Exits"); returnString += "\n";

                        //String[] temp = { "north", "south", "east", "west" };

                        //for (var i = 0; i < temp.Length; i++)
                        //{
                        //    if (reader[temp[i]] != null)
                        //    {
                        //        returnString += (reader[temp[i]] + " ");
                        //    }
                        //}

                        

                        if ((input[1].ToLower() == "north") && (reader["north"] != null))
                        {
                            currentRoom = reader["north"].ToString();
                        }
                        else
                        {
                            if ((input[1].ToLower() == "south") && (reader["south"] != null))
                            {
                                currentRoom = reader["south"].ToString();
                            }
                            else
                            {
                                if ((input[1].ToLower() == "east") && (reader["east"] != null))
                                {
                                    currentRoom = reader["east"].ToString();
                                }
                                else
                                {
                                    if ((input[1].ToLower() == "west") && (reader["west"] != null))
                                    {
                                        currentRoom = reader["west"].ToString();
                                    }
                                    else
                                    {
                                        //handle error
                                        Console.WriteLine("\nERROR");
                                        Console.WriteLine("\nCan not go " + input[1] + " from here");
                                        Console.WriteLine("\nPress any key to continue");
                                        Console.ReadKey(true);
                                    }
                                }
                            }
                        }
                        player.currentRoom = currentRoom;
                        bool error = false;
                        if (!error)
                        {
                            roomUpdate(player, currentRoom);
                            //DungeonInfo(player, false);
                        }

                    }

                    //bool error = false;
                    //if (!error)
                    //{
                    //    roomUpdate(player, currentRoom);
                    //}
                    returnString += DungeonInfo(player, true);
                    return returnString;

                default:
                    returnString += ("\nERROR");
                    returnString += ("\nCan not " + Key);
                    returnString += ("\nPress any key to continue");
                    
                    return returnString;
            }

            return returnString;

        }
    }
}
