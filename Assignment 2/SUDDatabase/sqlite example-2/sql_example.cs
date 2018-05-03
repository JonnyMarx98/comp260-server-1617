using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
/*
namespace sqlite_example
{
    class Program
    {
        static void Main(string[] args)
        {
			sqliteConnection conn = null;

            bool quit = false;

            string databaseName = "data.database";

            while (!quit)
            {
                Console.WriteLine("\n");
                Console.WriteLine("1-create new DB");
                Console.WriteLine("2-open existing DB");
                Console.WriteLine("3-add entry");
                Console.WriteLine("4-display entries");
                Console.WriteLine("5-delete an entry");
                Console.WriteLine("X-quit");

                var opt = Console.ReadLine();

                switch (opt[0])
                {
                    case '1':
                        {                            
                            try
                            {
								sqliteConnection.CreateFile(databaseName);

								conn = new sqliteConnection("Data Source=" + databaseName + ";Version=3;FailIfMissing=True");

								sqliteCommand command;

                                conn.Open();

								command = new sqliteCommand("create table table_phonenumbers (name varchar(20), number int )", conn);
                                command.ExecuteNonQuery();

                                //command = new SQLiteCommand("drop table table_phonenumbers", conn);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Create DB failed: " + ex);
                            }
                        }
                        break;

                    case '2':
                        {
							conn = new sqliteConnection("Data Source=" + databaseName + ";Version=3;FailIfMissing=True");

                            try
                            {
                                conn.Open();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Open existing DB failed: " + ex);
                            }
                        }
                        break;

                    case '3':
                        {           
                            try
                            {
                                Console.WriteLine("enter name");
                                var name = Console.ReadLine();

                                Console.WriteLine("enter number:");
                                var phoneNo = Console.ReadLine();

                                
								var command = new sqliteCommand("select * from  table_phonenumbers where name == '" + name + "'", conn);
                                var reader = command.ExecuteReader();

                                if (reader.HasRows == false)
                                {
                                    try
                                    {
                                        var sql = "insert into " + "table_phonenumbers" + " (name, number) values ";
                                        sql += "('" + name + "'";
                                        sql += ",";
                                        sql += "'" + phoneNo + "'";
                                        sql += ")";

										command = new sqliteCommand(sql, conn);
                                        command.ExecuteNonQuery();
                                    }
                                    catch(Exception ex)
                                    {
                                        Console.WriteLine("Failed to add: " + name + " : " + phoneNo + " to DB "+ex);
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("User: " + name + " in DB already");
                                }
                            }
                            catch(Exception ex)
                            {
                                Console.WriteLine("Failed to add to DB " + ex);
                            }
                        }
                        break;

                    case '4':
                        {
                            try
                            {
                                Console.WriteLine("");
								var command = new sqliteCommand("select * from " + "table_phonenumbers" + " order by name asc", conn);
                                var reader = command.ExecuteReader();

                                while (reader.Read())
                                {
                                    Console.WriteLine("Name: " + reader["name"] + "\tnumber: " + reader["number"]);
                                }

                                reader.Close();
                                Console.WriteLine("");
                            }
                            catch(Exception ex)
                            {
                                Console.WriteLine("Failed to display DB");
                            }
                        }
                        break;

                    case '5':
                        {
                            try
                            {
                                Console.WriteLine("enter name");
                                var name = Console.ReadLine();
                                
								var command = new sqliteCommand("delete from table_phonenumbers where name == '" + name + "'", conn);
                                var reader = command.ExecuteReader();

                                if (reader.RecordsAffected > 0)
                                {
                                    Console.WriteLine("Deleted: " + name);
                                }
                                else
                                {
                                    Console.WriteLine("Not in DB: " + name);
                                }
                            }
                            catch(Exception ex)
                            {
                                Console.WriteLine("Failed to delete from DB:"+ex);
                            }
                        }
                        break;

                    case 'X':
                    case 'x':
                        {
                            quit = true;
                        }
                        break;
                }
            }
        }
    }
}
*/