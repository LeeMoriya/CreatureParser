using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace CreatureParser
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Rain World - Creature Parser";
            Console.WriteLine("Drag and drop a World_XX.txt file into this window and hit enter:");
            string file = Console.ReadLine();
            if(file != "")
            {
                if (file.StartsWith("\"") && file.EndsWith("\""))
                {
                    string trim = Regex.Split(file ,"\"")[1];
                    if (File.Exists(trim))
                    {
                        ParseWorldFile(trim);
                    }
                    else
                    {
                        Console.WriteLine("World file could not be found!");
                        Console.ReadLine();
                    }
                    Console.WriteLine("Not a valid file path!");
                    Console.ReadLine();
                }
            }
        }

        static void ParseWorldFile(string world)
        {
            //This hurts to look at
            Dictionary<string, Dictionary<string, int>> spawns = new Dictionary<string, Dictionary<string, int>>();
            string[] lines = File.ReadAllLines(world);
            bool creatures = false;
            for (int i = 0; i < lines.Length; i++)
            {
                if(lines[i].StartsWith("END CREATURES"))
                {
                    creatures = false;
                }
                if (creatures)
                {
                    if (!lines[i].StartsWith("//") && lines[i].Length >= 1)
                    {
                        //Character specific
                        if (lines[i].StartsWith("("))
                        {

                        }
                        //Applies to all characters
                        else
                        {
                            //Create new entry for All
                            if (!spawns.ContainsKey("All"))
                            {
                                spawns.Add("All", new Dictionary<string, int>());
                            }
                            //Discard room name and get creature list
                            if(lines[i].StartsWith("LINEAGE"))
                            string[] crits = Regex.Split(lines[i], ":");
                        }
                        Console.WriteLine(lines[i]);
                    }
                }
                if (lines[i].StartsWith("CREATURES"))
                {
                    creatures = true;
                }
            }
            Console.ReadLine();
        }
    }
}
