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

namespace SUD
{
    public class Dungeon
    {
        sqliteConnection conn = null;
        string databaseName = "data.database";

        
        String currentRoom="";

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

            //currentRoom = roomMap["Room 0"];

            try
            {
                sqliteConnection.CreateFile(databaseName);

                conn = new sqliteConnection("Data Source=" + databaseName + ";Version=3;FailIfMissing=True");

                sqliteCommand command;

                conn.Open();

                command = new sqliteCommand("create table table_rooms (name varchar(20), desc varchar(20), north varchar(20), south varchar(20), west varchar(20), east varchar(20))", conn);
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
                        Console.WriteLine("Failed to add room" + ex);
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
                        Console.WriteLine("Name: " + reader["name"] + "Exits: " + reader["north"] + reader["south"] + reader["west"] + reader["east"]);
                    }

                    reader.Close();
                    Console.WriteLine("");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to display DB");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Create DB failed: " + ex);
            }

            currentRoom = "Room 0";
        }

        public void Process()
        {
            Console.Clear();

            var command = new sqliteCommand("select * from  table_rooms where name == '" + currentRoom + "'", conn);
            var reader = command.ExecuteReader();

            while (reader.Read())
            {                
                Console.WriteLine(reader["desc"]);
                Console.WriteLine("Exits");

                String[] temp = { "north", "south", "east", "west" };

                for (var i = 0; i < temp.Length; i++)
                {
                    string result = reader[temp[i]] as String;


                    if (result != "") 
                    {
                        Console.WriteLine(reader[temp[i]] + " " + temp[i]);
                    }
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
                    command = new sqliteCommand("select * from  table_rooms where name == '" + currentRoom + "'", conn);
                    reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        Console.WriteLine("Name: " + reader["name"] + "\tdesc: " + reader["desc"]);
                        Console.WriteLine(reader["desc"]);
                        Console.WriteLine("Exits");

                        String[] temp = { "north", "south", "east", "west" };

                        for (var i = 0; i < temp.Length; i++)
                        {
                            if (reader[temp[i]] != null)
                            {
                                Console.Write(reader[temp[i]] + " ");
                            }
                        }

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

                    }

                    //if ((input[1].ToLower() == "north") && (reader["north"] != null))
                    //{
                    //    currentRoom = reader["north"].ToString();
                    //}

                    Console.Write("");

#if false

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
#endif

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
