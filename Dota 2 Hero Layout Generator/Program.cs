using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Net;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Dota_2_Hero_Layout_Generator
{
    public class Hero
    {
        public string name;
        public float pickrate { get; set; }
        public float winrate { get; set; }
    }

    public class Collection
    {
        public string name;
        public float threshold;
        public string url;
    }

    class Program
    {
        static string[] heroesListedByID = {
                "Anti-Mage",
                "Axe",
                "Bane",
                "Bloodseeker",
                "Crystal Maiden",
                "Drow Ranger",
                "Earthshaker",
                "Juggernaut",
                "Mirana",
                "Morphling",
                "Shadow Fiend",
                "Phantom Lancer",
                "Puck",
                "Pudge",
                "Razor",
                "Sand King",
                "Storm Spirit",
                "Sven",
                "Tiny",
                "Vengeful Spirit",
                "Windranger",
                "Zeus",
                "Kunkka",
                "-",
                "Lina",
                "Lion",
                "Shadow Shaman",
                "Slardar",
                "Tidehunter",
                "Witch Doctor",
                "Lich",
                "Riki",
                "Enigma",
                "Tinker",
                "Sniper",
                "Necrophos",
                "Warlock",
                "Beastmaster",
                "Queen of Pain",
                "Venomancer",
                "Faceless Void",
                "Wraith King",
                "Death Prophet",
                "Phantom Assassin",
                "Pugna",
                "Templar Assassin",
                "Viper",
                "Luna",
                "Dragon Knight",
                "Dazzle",
                "Clockwerk",
                "Leshrac",
                "Nature&#39;s Prophet",
                "Lifestealer",
                "Dark Seer",
                "Clinkz",
                "Omniknight",
                "Enchantress",
                "Huskar",
                "Night Stalker",
                "Broodmother",
                "Bounty Hunter",
                "Weaver",
                "Jakiro",
                "Batrider",
                "Chen",
                "Spectre",
                "Ancient Apparition",
                "Doom",
                "Ursa",
                "Spirit Breaker",
                "Gyrocopter",
                "Alchemist",
                "Invoker",
                "Silencer",
                "Outworld Devourer",
                "Lycan",
                "Brewmaster",
                "Shadow Demon",
                "Lone Druid",
                "Chaos Knight",
                "Meepo",
                "Treant Protector",
                "Ogre Magi",
                "Undying",
                "Rubick",
                "Disruptor",
                "Nyx Assassin",
                "Naga Siren",
                "Keeper of the Light",
                "Io",
                "Visage",
                "Slark",
                "Medusa",
                "Troll Warlord",
                "Centaur Warrunner",
                "Magnus",
                "Timbersaw",
                "Bristleback",
                "Tusk",
                "Skywrath Mage",
                "Abaddon",
                "Elder Titan",
                "Legion Commander",
                "Techies",
                "Ember Spirit",
                "Earth Spirit",
                "Underlord",
                "Terrorblade",
                "Phoenix",
                "Oracle",
                "Winter Wyvern",
                "Arc Warden",
                "Monkey King",
                "-",
                "-",
                "-",
                "-",
                "Dark Willow",
                "Pangolier",
                "Grimstroke",
                "-",
                "-",
                "-",
                "-",
                "Void Spirit",
                "-",
                "Snapfire",
                "Mars"
            };


        static string getDotabuffTable(string url)
        {
            WebClient client = new WebClient();

            client.Headers.Add("User-Agent: BrowseAndDownload");
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            Thread.Sleep(TimeSpan.FromSeconds(5));
            Console.WriteLine("Requesting: " + url);
            string downloadString = client.DownloadString(new Uri(url));

            int start = downloadString.IndexOf("<tbody>");
            int end = downloadString.IndexOf("</tbody>");

            downloadString = downloadString.Substring(start + 7, end - start - 7);

            return downloadString;
        }

        static List<Hero> getHeroList(string table)
        {
            Regex herolines = new Regex("<a class=\"link-type-hero\" href=\"/heroes/.+?(?=\")\">.+?(?=<)</a>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            MatchCollection heroes = herolines.Matches(table);
            Regex percentlines = new Regex("<td data-value=\".+?(?=\")\">.+?(?=%)%", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            MatchCollection percenteges = percentlines.Matches(table);

            List<Hero> heroesList = new List<Hero>();

            foreach (Match hero in heroes)
            {
                string element = hero.Value;
                element = element.Replace("</a>", "");
                element = Regex.Replace(element, "<a class=\"link-type-hero\" href=\"/heroes/.+?(?=\")\">", "");

                Hero tmp = new Hero();
                tmp.name = element;
                heroesList.Add(tmp);
            }

            int counter = 0;
            int index = 0;
            foreach (Match percent in percenteges)
            {

                string element = percent.Value;
                element = element.Replace("%", "");
                element = Regex.Replace(element, "<td data-value=\".+?(?=\")\">", "");

                if (counter % 2 == 0)
                {
                    float pick = float.Parse(element);
                    heroesList[index].pickrate = pick;
                }
                else
                {
                    float win = float.Parse(element);
                    heroesList[index].winrate = win;
                    index++;
                }

                counter++;
            }

            return heroesList;
        }

        public static bool createLayout(List<Collection> items, string[] datas)
        {
            Console.WriteLine("");
            DateTime thisDay = DateTime.Now;
            int size = items.Count;

            List<string> layout = new List<string>();
            layout.Add("{");
            layout.Add("\t\"version\": 3,");
            layout.Add("\t\"configs\":");
            layout.Add("\t[");
            layout.Add("\t\t{");
            layout.Add("\t\t\t\"config_name\": \"Dotabuff Lanes (" + thisDay.ToString() + ")\",");
            layout.Add("\t\t\t\"categories\":");
            layout.Add("\t\t\t[");

            float padding = 20f;
            float width = 1180f;
            float height = 80;
            float x = 0f;
            float y = 0f;

            for(int i = 0; i < size; i++)
            {
                int count = 0;
                Console.WriteLine("Exporting data from " + items[i].name);
                List<Hero> laneHeroesData = getHeroList(datas[i]);
                //Console.WriteLine(lanes[i]);
                for (int d = 0; d < laneHeroesData.Count; d++)
                {
                    if (laneHeroesData[d].pickrate >= items[i].threshold) count++;
                }
                Console.WriteLine(count + " heroes are meeting the threshold for " + items[i].name);

                Console.WriteLine("Generating layout for " + items[i].name);
                layout.Add("\t\t\t\t{");
                layout.Add("\t\t\t\t\t\"category_name\": \"" + items[i].name + "\",");
                layout.Add("\t\t\t\t\t\"x_position\": " + x +",");
                layout.Add("\t\t\t\t\t\"y_position\": " + y + ",");
                layout.Add("\t\t\t\t\t\"width\": " + width + ",");
                layout.Add("\t\t\t\t\t\"height\": " + height * (count / 24 + 1) + ",");
                y += height * (count / 24 + 1) + padding;
                layout.Add("\t\t\t\t\t\"hero_ids\":");
                layout.Add("\t\t\t\t\t[");

                

                for (int d = 0; d < count; d++)
                {
                    if(laneHeroesData[d].pickrate >= items[i].threshold)
                    {
                        int id = Array.IndexOf(heroesListedByID, laneHeroesData[d].name) + 1;
                        if(id < 1)
                        {
                            Console.WriteLine(laneHeroesData[d].name + "\t" + laneHeroesData[d].pickrate + "\twasn't found in heroes list :(");
                        } else
                        {
                            if (d == count - 1)
                            {
                                layout.Add("\t\t\t\t\t\t" + id);
                            }
                            else
                            {
                                layout.Add("\t\t\t\t\t\t" + id + ",");
                            }
                        }
                    }
                }

                layout.Add("\t\t\t\t\t]");
                if (i == size - 1) {
                    layout.Add("\t\t\t\t}");
                } else
                {
                    layout.Add("\t\t\t\t},");
                }

                Console.WriteLine("");
            }


            layout.Add("\t\t\t]");
            layout.Add("\t\t}");
            layout.Add("\t]");
            layout.Add("}");

            TextWriter tw = new StreamWriter("hero_grid_config.json");

            foreach (String s in layout)
                tw.WriteLine(s);

            tw.Close();



            return true;
        }


        static void Main(string[] args)
        {

            StreamReader r = new StreamReader("config.json");
            string json = r.ReadToEnd();
            List<Collection> items = JsonConvert.DeserializeObject<List<Collection>>(json);

            /*string[] laneNames = { "Mid Lane", "Off Lane", "Safe Lane", "Jungle", "Roaming" };
            float[] thresholds = { 30f, 30f, 30f, 11f, 0.0f };
            string[] urls = {
                "https://www.dotabuff.com/heroes/lanes?lane=mid",
                "https://www.dotabuff.com/heroes/lanes?lane=off",
                "https://www.dotabuff.com/heroes/lanes?lane=safe",
                "https://www.dotabuff.com/heroes/lanes?lane=jungle",
                "https://www.dotabuff.com/heroes/lanes?lane=roaming"
            };*/
            string[] data = new string[items.Count];

            for(int i = 0; i < 20+20+60+4; i++)
            {
                Console.Write("-");
            }
            Console.WriteLine();
            Console.WriteLine(String.Format("|{0,-20}|{1,-20}|{2,-60}|", "Lane", "Threshold", "Url"));
            for (int i = 0; i < 20 + 20 + 60 + 4; i++)
            {
                Console.Write("-");
            }
            Console.WriteLine();
            for (int i = 0; i < 5; i++)
            {
                Console.WriteLine(String.Format("|{0,-20}|{1,-20}|{2,-60}|", items[i].name, ">=" + items[i].threshold + "%", items[i].url));
            }
            for(int i = 0; i < 20+20+60+4; i++)
            {
                Console.Write("-");
            }
            Console.WriteLine();
            Console.WriteLine();
            for (int i = 0; i < items.Count; i++)
            {
                data[i] = getDotabuffTable(items[i].url);
            }

            if(createLayout(items, data))
            {
                Console.WriteLine("Hero Grid Layout has been created!");
            } else
            {
                Console.WriteLine("Couldn't create layout");
            }

            Console.WriteLine("\n\nPress any key to exit");
            Console.ReadKey();
        }
    }
}
