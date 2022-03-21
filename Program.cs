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
        static Dictionary<string, SortedDictionary<string, int>> spawns = new Dictionary<string, SortedDictionary<string, int>>();
        static Dictionary<string, SortedDictionary<string, int>> lineages = new Dictionary<string, SortedDictionary<string, int>>();

        static void Main(string[] args)
        {
            Console.Title = "Rain World - Creature Parser";
            bool loop = true;
            while (loop)
            {
                Console.WriteLine("Enter or paste the path to your Rain World game directory:");
                string file = Console.ReadLine();
                if(file.ToLower() == "exit" || file.ToLower() == "quit")
                {
                    loop = false;
                }
                ScanWorldFolder(file);
            }
        }

        static void ScanWorldFolder(string path)
        {
            //Clear dictionary
            spawns = new Dictionary<string, SortedDictionary<string, int>>();
            Console.WriteLine();
            if (Directory.Exists(path))
            {
                if (Directory.GetDirectories(path).Contains(path + Path.DirectorySeparatorChar + "World"))
                {
                    Console.WriteLine("World folder detected!");
                    string worldFolder = path + Path.DirectorySeparatorChar + "World" + Path.DirectorySeparatorChar + "Regions";
                    if (Directory.Exists(worldFolder))
                    {
                        Console.WriteLine("Region folder detected!");
                    }
                    else
                    {
                        Console.WriteLine("Region folder could not be located in this directory");
                    }
                    string[] regionFolders = Directory.GetDirectories(worldFolder);
                    for (int i = 0; i < regionFolders.Length; i++)
                    {
                        string regionName = regionFolders[i].Substring(regionFolders[i].Length - 2, 2);
                        string worldFile = (regionFolders[i] + Path.DirectorySeparatorChar + "World_" + regionName + ".txt").ToLower();
                        string[] files = Directory.GetFiles(regionFolders[i]);
                        for (int c = 0; c < files.Length; c++)
                        {
                            files[c] = files[c].ToLower();
                        }
                        if (files.Contains(worldFile))
                        {
                            Console.WriteLine(regionName + " world file detected!");
                            ParseWorldFile(worldFile);
                        }
                    }
                    Console.WriteLine();
                    Console.WriteLine("--------------------------");
                    for (int i = 0; i < spawns.Keys.Count; i++)
                    {
                        Console.WriteLine();
                        Console.WriteLine("[" + IndexToSlugcatName(spawns.ElementAt(i).Key) + "]");
                        //Each creature
                        for (int c = 0; c < spawns.ElementAt(i).Value.Keys.Count; c++)
                        {
                            Console.WriteLine(spawns.ElementAt(i).Value.ElementAt(c).Key + " x" + spawns.ElementAt(i).Value.ElementAt(c).Value);
                        }
                        if (lineages.ContainsKey(spawns.ElementAt(i).Key))
                        {
                            Console.WriteLine();

                                Console.WriteLine("[LINEAGES]");
                                for (int c = 0; c < lineages[spawns.ElementAt(i).Key].Keys.Count; c++)
                                {
                                    Console.WriteLine(lineages[spawns.ElementAt(i).Key].ElementAt(c).Key + " x" + lineages[spawns.ElementAt(i).Key].ElementAt(c).Value);
                                }
                            
                        }
                    }
                }
                else
                {
                    Console.WriteLine("World folder could not be located in this directory");
                }
            }
            else
            {
                Console.WriteLine("Invalid folder path!");
            }
        }

        static void ParseWorldFile(string world)
        {
            //This hurts to look at
            string[] lines = File.ReadAllLines(world);
            string region = world.Substring(world.Length - 6, 2);
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
                            try
                            {
                                //Example line:
                                //(0,8)SB_C01 : 2-TentaclePlant, 7-Black, 9-Black

                                //Slugcats these spawns apply to
                                string[] characters = Regex.Split(Regex.Split(lines[i].Remove(0, 1), "\\)")[0].Trim(), ",");

                                if (!lines[i].Contains("LINEAGE"))
                                {
                                    string critLine = Regex.Split(lines[i], ":")[1];
                                    string[] crits = Regex.Split(critLine, ",");
                                    for (int c = 0; c < crits.Length; c++)
                                    {
                                        string creature = CreatureName(Regex.Split(crits[c], "-")[1]);
                                        int amount = 1;
                                        if (Regex.Split(crits[c], "-").Length >= 3 && Regex.Split(crits[c], "-")[2] != "" && !Regex.Split(crits[c], "-")[2].StartsWith("{") && !Regex.Split(crits[c], "-")[2].Contains("\\.") && !Regex.Split(crits[c], "-")[2].Contains("Winter"))
                                        {
                                            amount = int.Parse(Regex.Split(crits[c], "-")[2]);
                                        }
                                        //Creature not in list
                                        for (int s = 0; s < characters.Length; s++)
                                        {
                                            if (!spawns.ContainsKey(characters[s]))
                                            {
                                                spawns.Add(characters[s], new SortedDictionary<string, int>());
                                            }
                                            if (!spawns[characters[s]].ContainsKey(creature))
                                            {
                                                spawns[characters[s]].Add(creature, amount);
                                            }
                                            else
                                            {
                                                spawns[characters[s]][creature] += amount;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    //Example Lineage
                                    //(0,8)LINEAGE : SU_A20 : 6 : Pink-0.1, NONE-0.5, Blue-0.2, Red-0

                                    string critLine = Regex.Split(lines[i], ":")[3];
                                    string[] crits = Regex.Split(critLine, ",");
                                    for (int c = 0; c < crits.Length; c++)
                                    {
                                        string creature = CreatureName(Regex.Split(crits[c], "-")[0]);
                                        int amount = 1;
                                        //Creature not in list
                                        for (int s = 0; s < characters.Length; s++)
                                        {
                                            if (!lineages.ContainsKey(characters[s]))
                                            {
                                                lineages.Add(characters[s], new SortedDictionary<string, int>());
                                            }
                                            if (!lineages[characters[s]].ContainsKey(creature))
                                            {
                                                lineages[characters[s]].Add(creature, amount);
                                            }
                                            else
                                            {
                                                lineages[characters[s]][creature] += amount;
                                            }
                                        }
                                    }
                                }
                            }
                            catch
                            {
                                Console.WriteLine("ERROR ON LINE: " + (i-1) + " in file: " + world);
                                Console.WriteLine(lines[i]);
                            }
                        }
                        //Applies to all characters
                        else
                        {
                            //Create new entry for All
                            if (!spawns.ContainsKey(region))
                            {
                                spawns.Add(region, new SortedDictionary<string, int>());
                            }
                            try
                            {
                                //Discard room name and get creature list
                                if (!lines[i].Contains("LINEAGE"))
                                {
                                    string critLine = Regex.Split(lines[i], ":")[1];
                                    string[] crits = Regex.Split(critLine, ",");
                                    for (int c = 0; c < crits.Length; c++)
                                    {
                                        string creature = CreatureName(Regex.Split(crits[c], "-")[1]);
                                        int amount = 1;
                                        if (Regex.Split(crits[c], "-").Length >= 3 && Regex.Split(crits[c], "-")[2] != "" && !Regex.Split(crits[c], "-")[2].StartsWith("{") && !Regex.Split(crits[c], "-")[2].Contains("\\.") && !Regex.Split(crits[c], "-")[2].Contains("Winter"))
                                        {
                                            amount = int.Parse(Regex.Split(crits[c], "-")[2]);
                                        }
                                        //Creature not in list
                                        if (!spawns[region].ContainsKey(creature))
                                        {
                                            spawns[region].Add(creature, amount);
                                        }
                                        else
                                        {
                                            spawns[region][creature] += amount;
                                        }
                                    }
                                }
                                else
                                {
                                    //Example Lineage
                                    //(0,8)LINEAGE : SU_A20 : 6 : Pink-0.1, NONE-0.5, Blue-0.2, Red-0

                                    string critLine = Regex.Split(lines[i], ":")[3];
                                    string[] crits = Regex.Split(critLine, ",");
                                    for (int c = 0; c < crits.Length; c++)
                                    {
                                        string creature = CreatureName(Regex.Split(crits[c], "-")[0]);
                                        int amount = 1;
                                        //Creature not in list
                                        if (!lineages.ContainsKey(region))
                                        {
                                            lineages.Add(region, new SortedDictionary<string, int>());
                                        }
                                        if (!lineages[region].ContainsKey(creature))
                                        {
                                            lineages[region].Add(creature, amount);
                                        }
                                        else
                                        {
                                            lineages[region][creature] += amount;
                                        }
                                    }
                                }
                            }
                            catch
                            {
                                Console.WriteLine("ERROR ON LINE: " + (i - 1) + " in file: " + world);
                                Console.WriteLine(lines[i]);
                            }
                        }
                        //Show each World file line
                        //Console.WriteLine(lines[i]);
                    }
                }
                if (lines[i].StartsWith("CREATURES"))
                {
                    creatures = true;
                }
            }
        }

        static string IndexToSlugcatName(string index)
        {
            switch (index)
            {
                case "0":
                    return "Survivor";
                case "1":
                    return "Monk";
                case "2":
                    return "Hunter";
                case "3":
                    return "???";
                case "4":
                    return "Artificer";
                case "5":
                    return "Rivulet";
                case "6":
                    return "Saint";
                case "7":
                    return "Spearmaster";
                case "8":
                    return "Gourmand";
            }
            return index;
        }

        static string CreatureName(string input)
        {
            switch (input)
            {
                case "Eggbug":
                    return "EggBug";
                case "Dropbug":
                case "Dropwig":
                    return "DropBug";
                case "Miros":
                    return "MirosBird";
                case "Mimic":
                    return "PoleMimic";
                case "Tube":
                    return "TubeWorm";
                case "Elite":
                    return "EliteScavenger";
            }
            //Remove all whitespace
            input = Regex.Replace(input, @"\s+", "");
            return input;
        }
    }
}
