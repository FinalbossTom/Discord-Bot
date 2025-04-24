using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace Discord_Bot.Bot.Special
{


    public class HandleEasterEvent
    {

        public static String ResourcePath = "Data";
        public readonly List<Player> Players = new();

        private static FFLocation[] Locations = {
            // Easy - Green Locations
            new("Chocobo Square",[6.1,5.1],0.7,4),
            new("Mor Dhona",[31.0,5.2],1.0,4),
            new("Western Thanalan",[10.7,14.9],1.2,4),          // 16 points
            new("Limsa Lominsa Upper Decks",[7.5,14.7],0.6,4),  // 11.76%

            // Medium - Blue Locations
            new("The Sea of Clouds",[16.4,18.1],0.8,8),
            new("Eulmore",[10.2,12.4],8),
            new("The Ruby Sea",[32.1,5.1],0.6,8),               // 32 points
            new("Lakeland",[5.3,26.7],0.7,8),                   // 23.52%
                                                                // 35% up to this point
            // Hard - Red Locations
            new("The Azim Steppe",[8.9,35.2],2.0,12),
            new("Outer La Noscea",[15.4,10.0],1.0,12),
            new("Kugane",[12.0,5.9],12),                        // 48 points
            new("Fortemps Manor",[6.1,6.1],1.0,12),             // 35.29%
                                                                // up to 96 points
            // Master - Gold Locations
            new("Dusk Vigil",[10.9,5.1],0.5,20),                // 40 points
            new("Specific House",[1.0,1.0],5.0,20),             // 29.41%
        };
        private static readonly int TotalPoints = 136;
        // ranks
        // 0 - 34 - 68 - 102 - 136 - 137
        public bool firstPlaceTaken = false;


        public HandleEasterEvent()
        {
            String[] files = Directory.GetFiles(ResourcePath);
            foreach (string file in files)
            {
                Player target = new Player(file);
                if(target.Points == 137)firstPlaceTaken = true;
                Players.Add(target);
                
            }
        }

        public String CheckLocation(SocketMessage message)
        {
            Player? currentPlayer = null;
            foreach (Player Player in Players)
            {
                if(Player.ID == message.Author.Id) currentPlayer = Player;
            }
            if(currentPlayer == null)
            {
                currentPlayer = new Player(message.Author.Id, message.Author.GlobalName);
                Players.Add(currentPlayer);
                Console.WriteLine("\nPlayer did not exist, created: " + currentPlayer.ID);
                currentPlayer.Save();
            }

            FFLocation? guess = FFLocation.Create(message.Content);
            if(guess == null)
            {
                return "Seems like you did the Format wrong..." +
                "\nMake sure you used the right Format like this: ``Lower La Noscea ( 2.6 , 4.9 )`` or just ``Lavender Beds 22.3 14.2``" +
                "\nYou can make it easy for yourself by creating a Macro with the line ``/echo <pos>`` and just clicking it whenever you are at a Location.";
            }

            if (guess.Compare(new FFLocation("Private House - Shirogane", [6.0,6.0],1.0,0)))
            {
                return "**Hmm...**" +
                    "\nIf you are looking for the Eggcellent egg, you are almost right, but you need something more specific." +
                    "\nLook at what brought you here.";
            }

            currentPlayer.Attempts += 1;

            int i = 0;
            foreach (FFLocation location in Locations)
            {
                if (location.Compare(guess))
                {
                    if (currentPlayer.FoundLocations[i])
                    {
                        Console.WriteLine("Blocking double dip attempt from " + currentPlayer.Name);
                        return "Hey, no double dipping - You already found this Location!";
                    }



                    currentPlayer.Points += location.Points;
                    currentPlayer.NextRank -= location.Points;
                    currentPlayer.FoundLocations[i] = true;
                    

                    int percent = (int)((double)currentPlayer.Points / TotalPoints * 100);

                    Console.WriteLine(currentPlayer.Name + " has found " + location + " and is now at " + currentPlayer.Points + "/" + TotalPoints + "(" + percent + "%) Points.");

                    String output = $"## Thats Correct!" +
                        $"\nYou have received **{location.Points} Points** for this Location." +
                        $"\nYou now have a total of {currentPlayer.Points}/{TotalPoints} ({percent}%) Points.";


                    if(currentPlayer.NextRank <= 0)// 0 - 34 - 68 - 102 - 136 - 137
                    {
                        String newRank = "";
                        bool found = false;

                        if (currentPlayer.Points >= 137 && !found) { newRank = "Golden Egg"; found = true; currentPlayer.NextRank += 999; }
                        if (currentPlayer.Points >= 136 && !found) { newRank = "Colorful Egg"; found = true; currentPlayer.NextRank += 1; }
                        if (currentPlayer.Points >= 102 && !found) { newRank = "Red Egg"; found=true; currentPlayer.NextRank += 34; }
                        if (currentPlayer.Points >= 68 && !found)  { newRank = "Blue Egg"; found=true; currentPlayer.NextRank += 34; }
                        if (currentPlayer.Points >= 34 && !found)  { newRank = "Green Egg"; found =true; currentPlayer.NextRank += 34; }
                        output += $"You have reached the **{newRank}** Rank!";
                    }

                    
                    /*if (!firstPlaceTaken && currentPlayer.Points == TotalPoints)
                    {
                        firstPlaceTaken = true;
                        currentPlayer.Points += 1;
                        output = $"## Good job!" +
                                 $"\nYou are the first to find every single Egg and have reached the **Golden Egg** Rank!" +
                                 $"\nWe hope you enjoyed the Event, and will get back to you for your Reward.";
                    }*/

                    if (/*firstPlaceTaken &&*/ currentPlayer.Points == TotalPoints)
                    {
                        output = $"## Good job!" +
                                 $"\nYou have found every single Egg in this Hunt and have reached the **Colorful Egg** Rank!" +
                                 $"\nWe hope you enjoyed the Event, and will get back to you for your Reward.";
                    }

                    currentPlayer.Save();
                    return output;
                }
                i++;
            }
            currentPlayer.Save();
            Console.WriteLine($"Player {currentPlayer.Name} failed with input: {message.Content}");
            return "Seems like that Location is wrong... " +
                   "\nIf you are certain you got it right, make sure you have the correct format like this: ``Lower La Noscea ( 2.6 , 4.9 )``" +
                   "\nAs well as making sure to use ``/echo <pos>`` or ``/echo <flag`` since some locations are written differently!";
        }

    }

    class FFLocation
    {
        String MapName { get; set; }
        double[] Coordinates { get; set; }
        double leniency { get; set; } = 0.4;
        public int Points { get; set; }

        public FFLocation(String MapName, double[] Coordinates) { this.MapName = MapName; this.Coordinates = Coordinates; }
        public FFLocation(String MapName, double[] Coordinates, int Points) { this.MapName = MapName; this.Coordinates = Coordinates; this.Points = Points; }
        public FFLocation(String MapName, double[] Coordinates, double leniency, int Points) { this.MapName = MapName; this.Coordinates = Coordinates; this.leniency = leniency; this.Points = Points; }

        public bool Compare(FFLocation other)
        {

            Console.WriteLine("\n" + this.ToString() + " against " + other.ToString());

            var commonCharsSource = MapName.ToLower().Intersect(MapName.ToLower());
            var commonCharsOther = MapName.ToLower().Intersect(other.MapName.ToLower());
            double percentage = (double)commonCharsOther.Count() / commonCharsSource.Count() * 100;
            Console.WriteLine($" -> Name is {(int)percentage}/85% correct.");
            if (percentage < 85) return false;

            int x = 0, misses = 0;
            while (x < MapName.Length)
            { // Check if letters in the mapname are wrong - allow 4 mistakes
                if (misses >= 4) { Console.WriteLine(" -> Exceeded Misses, aborting."); return false; }
                if (x > other.MapName.Length) { x++; misses++; continue; }
                if (MapName[x].ToString().ToLower() != other.MapName[x].ToString().ToLower()) { misses++; }
                x++;
            }
            Console.WriteLine($" -> {misses}/4 misses.");

            double differenceX = Coordinates[0] - other.Coordinates[0],
                   differenceY = Coordinates[1] - other.Coordinates[1];
            if (differenceX < 0) differenceX *= -1; // need values in positive area
            if (differenceY < 0) differenceY *= -1;
            Console.WriteLine($" -> Difference: {differenceX} and {differenceY}");
            if (differenceX > leniency || differenceY > leniency) return false;
            return true;
        }

        static public FFLocation? Create(String other)
        {
            try
            {
                Console.WriteLine("Creating new Location from: " + other);
                if (other.Split("]").Length > 1) other = other.Split("]")[1]; // Filter out [12:20] Timestamp
                if (other.Split(":").Length > 1) other = other.Split(":")[1]; // Filter out Player Name:
                if (!Char.IsLetter(other[0])) other = other.Substring(1); //  Filter out red arrow

                string Text = "";
                int i = 0;
                while (!Char.IsNumber(other[i]) && other[i].ToString() != "(")
                {
                    Text += other[i];
                    i++;
                }
                Text = Text.Trim();

                string Numbers = "";
                while(i < other.Length)
                {
                    if (Char.IsNumber(other[i]) || other[i] == '.' || other[i] == ',') Numbers += other[i];
                    i++;
                }
                if (Numbers.Split(',').Length < 2)
                {
                    Console.WriteLine(" -> No comma found - aborting");
                    return null;
                }

                Console.WriteLine($" -> Text: {Text} Numbers: {Numbers}");

                double[] Coordinates = { -1.0, -1.0 };
                Coordinates[0] = Convert.ToDouble(Numbers.Split(',')[0], System.Globalization.CultureInfo.InvariantCulture);
                Coordinates[1] = Convert.ToDouble(Numbers.Split(',')[1], System.Globalization.CultureInfo.InvariantCulture);

                FFLocation output = new FFLocation(Text, Coordinates);
                Console.WriteLine("Creation Successful: " + output.ToString());
                return output;
            }
            catch(Exception ex)
            {
                return null;
            }
        }

        public override string ToString()
        {
            return $"[FFLocation] {MapName} ( {Coordinates[0]} , {Coordinates[1]} )";
        }
    }
    
    public class Player
    {
        public ulong ID { get; set; }
        public string Name { get; set; }
        public int Attempts { get; set; }
        public int Points { get; set; }
        public int NextRank { get; set; }
        public bool[] FoundLocations { get; set; }
        public DateTime LastLocationFound { get; set; }
        public DateTime LastChange { get; set; }
        [JsonIgnore] public int OriginalPoints;

        
        public Player(String path)
        {
            Console.WriteLine("Loading Player from path.");
            if(!Load(path)) throw new Exception("Invalid Path was given [Player]");
            OriginalPoints = Points;
        }

        public Player(ulong ID, String Name)
        {
            Console.WriteLine($"New Player created: Player ID: {ID}");
            this.ID = ID;
            this.Name = Name;
            this.Attempts = 0;
            this.Points = 0;    //  Gr1    Gr2    Gr3    Gr4    Bl1    Bl2    Bl3    Bl4    Re1    Re2     Re3     Re4    Go1    Go2
            this.NextRank = 34; // 0 - 34 - 68 - 102 - 136 - 137
            this.FoundLocations = [false, false, false, false, false, false, false, false, false, false , false , false ,false ,false];
            this.LastLocationFound = DateTime.Now;
            this.LastChange = DateTime.Now;
            OriginalPoints = 0;
            Save();
        }

        [JsonConstructor]
        public Player(ulong ID,String Name,int Attempts, int Points,int NextRank, bool[] FoundLocations,DateTime LastLocationFound, DateTime LastChange)
        {
            this.ID = ID;
            this.Name = Name;
            this.Attempts = Attempts;
            this.Points = Points;
            this.NextRank = NextRank;
            this.FoundLocations = FoundLocations;
            this.LastLocationFound = LastLocationFound;
            this.LastChange = LastChange;
            OriginalPoints = Points;
        }

        public void Save()
        {
            LastChange = DateTime.Now;
            if(OriginalPoints != Points) LastLocationFound = DateTime.Now;
            String jsonString = JsonConvert.SerializeObject(this);
            File.WriteAllText(HandleEasterEvent.ResourcePath + "/" + ID + ".json",jsonString);
        }

        public bool Load() { return Load(HandleEasterEvent.ResourcePath + "/" + ID + ".json"); }
        public bool Load(String Path)
        {
            String jsonString = File.ReadAllText(Path);
            Player? loaded = JsonConvert.DeserializeObject<Player>(jsonString);
            if (loaded == null) return false;
            this.ID = loaded.ID;
            this.Name = loaded.Name;
            this.Attempts = loaded.Attempts;
            this.Points = loaded.Points;
            this.NextRank = loaded.NextRank;
            this.FoundLocations = loaded.FoundLocations;
            this.LastLocationFound= loaded.LastLocationFound;
            this.LastChange = loaded.LastChange;
            Console.WriteLine($"-> {Name}: {Points} points");
            return true;
        }

    }

}
