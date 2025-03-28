using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Transactions;
using System.Xml.Linq;

namespace AdventureGame
{
    internal class Program
    {
        /// <summary>
        /// Class-instanzen erstellen (public, NICHT neu initialisieren
        /// </summary>
        public static Inventory inventory = new();
        public static Player player = new();
        public static LevelSelector levelSelector = new();
        public static Enemy enemy = new();
        public static Random random = new Random();

        /// <summary>
        /// global variablen erstellen
        /// </summary>
        /// 
        /*
        public static string[]? enemyNames = { 
            "Firon",
            "Syvor",
            "Glyth",
            "Xylon",
            "Mylar",
            "Kivak",
            "Drayn",
            "Zarok",
            "Wyrik",
            "Hydra"
        };
        */

        public static int position = 0;
        public static int total_enemy_damage = 0;
        public static int highscore = 0;

        public static bool using_damage_booster = false;

        public static bool AppleCooldown = false;
        public static bool HealingPotionCooldown = false;
        public static bool DamageBoosterCooldown = false;
        public static bool SmallBandageCooldown = false;

        public static Thread appleThread = new Thread(AppleCooldownFunction);
        public static Thread healingPotionThread = new Thread(HealingPotionCooldownFunction);

        public static Thread damageBoosterThread = new Thread(DamageBoosterCooldownFunction);
        public static Thread damageBoosterToggleThread = new Thread(DamageBoosterToggleActive);

        public static string savePosition = "position.txt";


        static void Main(string[] args)
        {
            readPositionVar();
            Username(player);
            
        }


        /// <summary>
        /// Inventory class, um z.b. die werte von waffen zu bekommen
        /// </summary>
        public class Inventory
        {
            private static Random random = new Random();
            public int extra_damage = random.Next(3, 5);

            public string WEAPON { get; set; } = "Stick";

            public int WEAPON_DAMAGE { get; set; }

            public int APPLES { get; set; } = 0; // HEALS 20 HP
            public int DAMAGE_BOOSTERS { get; set; } = 1; // INCREASES PLAYER DAMAGE BY 4
            public int HEALING_POTIONS { get; set; } = 2; // HEALS 25 HP
            public int SMALL_BANDAGES { get; set; } = 0; // HEALS 10 HP

            public int WeaponDamage()
            {
                int GetWeaponDamage()
                {  
                    switch (WEAPON)
                    {
                        case "Stick":
                            if (using_damage_booster == true)
                            {
                                return random.Next(3, 4) + extra_damage;
                            }
                            else
                            {
                                return random.Next(3, 4);
                            }
                        case "Iron Sword":
                            if (using_damage_booster == true)
                            {
                                return random.Next(4, 7) + extra_damage;
                            }
                            else
                            {
                                return random.Next(4, 7);
                            }
                        case "Magic Wand":
                            if (using_damage_booster == true)
                            {
                                return random.Next(6, 9) + extra_damage;
                            }
                            else
                            {
                                return random.Next(6, 9);
                            }
                        case "Target Slayer":
                            if (using_damage_booster == true)
                            {
                                return random.Next(8, 12) + extra_damage;
                            }
                            else
                            {
                                return random.Next(8, 12);
                            }
                        case "Aetherclaw":
                            if (using_damage_booster == true)
                            {
                                return random.Next(11, 15) + extra_damage;
                            }
                            else
                            {
                                return random.Next(11, 15);
                            }
                        default:
                            return random.Next(3, 4);
                    }
                }
                return GetWeaponDamage();
            }
        }


        /// <summary>
        /// Player class, also der spieler und seine werte
        /// </summary>
        public class Player
        {

            public string? NAME { get; set; } = "err_NoNameSet";
            public int HP { get; set; } = 100;
            public int DAMAGE()
            {
                return inventory.WeaponDamage();
            }
            public double GOLD { get; set; } = 10.0;
            public double XP { get; set; } = 0.0;
            string WEAPON { get; set; } = inventory.WEAPON;   

        }

        public class LevelSelector
        {
            public Player player1 = new();

            public string Save_1 { get; set; } = "Empty";


        }


        /// <summary>
        /// spielernamen auswählen
        /// </summary>
        /// <param name="player"></param>
        static void Username(Player player)
        {
            Console.Title = "Name Selecting";
            Console.CursorVisible = false;
            Console.Clear();
            Console.SetCursorPosition(45, 14);
            SlowWrite("Enter ", 100);
            Console.ForegroundColor = ConsoleColor.Red;
            SlowWrite("Player Name", 100);
            Console.ForegroundColor = ConsoleColor.White;
            SlowWrite(": ", 100);
            Console.CursorVisible = true;

            string? name = Console.ReadLine();

            if (name != "")
            {
                if (name.Equals("console", StringComparison.OrdinalIgnoreCase) ||
                    name.Equals("cmd", StringComparison.OrdinalIgnoreCase))
                {
                    cmd(name);
                }
                else
                {
                    player.NAME = name;
                    MainMenu(player);
                }
                    
            }

        }

        static void cmd(string name)
        {
            Console.Clear();
            Console.WriteLine("Config Editor, change name before leaving\n");
            bool run = true;

            while (run == true)
            {
                Console.Write(@"CMD-> ");
                string input = Console.ReadLine();

                if (input.StartsWith("gold", StringComparison.OrdinalIgnoreCase))
                {
                    string[] parts = input.Split(' ');
                    if (parts.Length == 2 && double.TryParse(parts[1], out double goldAmount))
                    {
                        player.GOLD = goldAmount;
                        Console.WriteLine($"gold set to: {player.GOLD}");
                    }
                    else
                    {
                        Console.WriteLine("Invalid input. Please enter a valid number after 'gold'.");
                    }
                }
                else if (input.StartsWith("name ", StringComparison.OrdinalIgnoreCase))
                {
                    string newName = input.Substring(5);
                    if (!string.IsNullOrWhiteSpace(newName))
                    {
                        player.NAME = newName;
                        name = null;
                        Console.WriteLine($"Player's name changed to: {player.NAME}");
                    }
                    else
                    {
                        Console.WriteLine("Invalid input. Please provide a non-empty name.");
                    }
                }
                else if (input.StartsWith("hp ", StringComparison.OrdinalIgnoreCase))
                {
                    string newHp = input.Substring(3);
                    if (!string.IsNullOrEmpty(newHp))
                    {
                        int _newHp = int.Parse(newHp);
                        player.HP = _newHp;
                        Console.WriteLine($"Hp set to {_newHp}");
                    }
                }
                else if (input == "help")
                {
                    Console.WriteLine(@"

Commands:

hp <hp>
gold <gold>
name <name>
exit
clear
help

");
                }
                else if (input == "clear" || input == "c")
                {
                    Console.Clear();
                }
                else if (input == "exit")
                {
                    run = false;
                    MainMenu(player);
                }
                else
                {
                    Console.WriteLine("Unrecognized command.");
                }
            }
            
        }


        static void MainMenu(Player player)
        {
            readPositionVar();
            Console.Title = $"{player.NAME} - Main Menu";
            Console.Clear();
            Console.SetCursorPosition(50, 9);
            Console.WriteLine("Main Menu");
            Console.SetCursorPosition(47, 11);
            Console.WriteLine($"HIGHSCORE: {highscore}");
            Console.SetCursorPosition(45, 13);
            Console.WriteLine("1. Start New Run");
            //Console.SetCursorPosition(45, 13);
            //Console.WriteLine("2. Load Game");
            Console.SetCursorPosition(45, 14);
            Console.WriteLine("3. Exit");

            string input = Console.ReadLine();
            switch(input)
            {
                case "1":
                    NewGame(player, levelSelector);
                    break;
                case "2":
                    break;
                case "3":
                    break;
            }
        }

        static void NewGame(Player player, LevelSelector levelSelector)
        {
            Console.Clear();
            Console.SetCursorPosition(50, 12);
            if (levelSelector.Save_1 == "Empty")
            {
                Console.WriteLine($"1. {levelSelector.Save_1}");
            } else if (levelSelector.Save_1 != "Empty")
            {

                Console.WriteLine($"1. {levelSelector.Save_1} - {player.NAME}");
            }

            string input = Console.ReadLine();
            if (input == "1")
            {
                if (levelSelector.Save_1 == "Empty")
                {
                    Console.Clear();
                    Console.Write("Enter save name: ");
                    string save_name = Console.ReadLine();
                    Console.Write("Enter player name: ");
                    string player_name = Console.ReadLine();

                    if (!string.IsNullOrEmpty(save_name))
                    {
                        if (!string.IsNullOrEmpty(player_name))
                        {
                            player.NAME = player_name;
                            levelSelector.Save_1 = save_name;

                            GameLoop(player, inventory);
                        }
                        else
                        {
                            Console.WriteLine("ExeptionCaught: 'player.NAME = null', will be using 'Max' as backup name");
                        }
                    }
                    else
                    {
                        throw new Exception("save_name found: NULL/EMPTY, FIX: Give the save file a name, that isnt empty.");
                    }



                }
                else
                {
                    Console.WriteLine("This save is already being used, and will be overwritten. proced? (y/n): ");
                    string proceed = Console.ReadLine();
                    if (proceed == "y" || proceed == "Y")
                    {
                        // OVERWRITE
                    }
                    else
                    {
                        Console.WriteLine("Ok!");
                        Thread.Sleep(300);
                        NewGame(player, levelSelector);
                    }
                }
            }
        }

        static void GiveNewSaveDetails(Player player, LevelSelector levelSelector)
        {
            Console.Clear();
            Console.SetCursorPosition(50, 12);
            Console.Write("Enter Save Name: ");
            string new_save_name_1 = Console.ReadLine();
            levelSelector.Save_1 = new_save_name_1;

            // ADD FILE SAVING LOGIC HERE
        }

        static void LoadGame(Player player)
        {
            // FILE SAVING LOGIC; RUNS WHEN APP STARTS
        }


        static void SlowWrite(string text, int delay)
        {
            foreach (char c in text)
            {
                Console.Write(c);
                Thread.Sleep(delay);

            }
        }
        

        public class Enemy
        {
            public int HP { get; set; }
            public int CalculateHP(string ename)                                                      // { get; set; } = 100;
            {
                switch (ename)
                {
                    case "Firon":
                        return random.Next(4, 10);
                    case "Syvor":
                        return random.Next(8, 14);
                    case "Glyth":
                        return random.Next(12, 16);
                    case "Xylon":
                        return random.Next(13, 18);
                    case "Mylar":
                        return random.Next(14, 20);
                    case "Kivak":
                        return random.Next(16, 22);
                    case "Drayn":
                        return random.Next(18, 24);
                    case "Zarok":
                        return random.Next(20, 26);
                    case "Wyrik":
                        return random.Next(22, 28);
                    case "Hydra":
                        return random.Next(24, 30);
                }
                return random.Next(4, 10);
            }

           


            public static string[]? enemyNames = {
                "Firon",
                "Syvor",
                "Glyth",
                "Xylon",
                "Mylar",
                "Kivak",
                "Drayn",
                "Zarok",
                "Wyrik",
                "Hydra"
            };

            // static int randomIndex = random.Next(enemyNames.Length);


            public string NAMES = enemyNames.OrderBy(_ => Guid.NewGuid()).First();

            public int DAMAGE()
            {
                switch (NAMES)
                {
                    case "Firon":
                        return random.Next(3, 4);
                    case "Syvor":
                        return random.Next(4, 6);
                    case "Glyth":
                        return random.Next(5, 7);
                    case "Xylon":
                        return random.Next(5, 10);
                    case "Mylar":
                        return random.Next(5, 13);
                    case "Kivak":
                        return random.Next(5, 15);
                    case "Drayn":
                        return random.Next(5, 17);
                    case "Zarok":
                        return random.Next(5, 19);
                    case "Wyrik":
                        return random.Next(5, 21);
                    case "Hydra":
                        return random.Next(5, 23);
                }
                return random.Next(3, 8);
            }

            /*
            public (int, string, int) Create()
            {
                
                string name = NAMES;
                int hp = CalculateHP(name);
                HP = hp;
                int damage = DAMAGE();

                Console.SetCursorPosition(40, 16);
                Console.WriteLine($"{name} is on your path! battle starting..");
                Thread.Sleep(1000);

                return (hp, name, damage);
                
            } */

            public (int, string, int) Create()
            {
                string name = enemyNames.OrderBy(_ => Guid.NewGuid()).First();
                int hp = CalculateHP(name);
                HP = hp;
                int damage = DAMAGE();  // Optionally, create a separate method

                Console.SetCursorPosition(40, 16);
                Console.WriteLine($"{name} is on your path! Battle starting...");
                Thread.Sleep(1000);

                return (hp, name, damage);
            }

        }

        static void GameLoop(Player player, Inventory inventory)
        {
            Console.Clear();
            Console.Title = "Adventure- Moving";
            while (true)
            {
                total_enemy_damage = 0;
                Console.SetCursorPosition(36, 13);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write($"{player.NAME}:   Hp: {player.HP}   Gold: {player.GOLD}");
                if (position <= 100)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write($"   Position: {position}\n");
                }
                else if (position >= 100 && position <= 200)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write($"   Position: {position}\n");
                }
                else if (position >= 200 && position <= 300)
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.Write($"   Position: {position}\n");
                }
                else if (position >= 300 && position <= 400)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write($"   Position: {position}\n");
                }
                else if (position >= 400)
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.Write($"   Position: {position}\n");
                }
                Console.ForegroundColor = ConsoleColor.White;
                Console.SetCursorPosition(50, 15);
                Console.Write("[a/d]: ");

                string input = Console.ReadLine();
                input.ToLower();
                if (input == "d")
                {
                    Console.Clear();
                    int chance = random.Next(0, 140);  //                           MAYBE IMPLEMENT DIFFICULTY SYSTEM
                    if (chance <= 10)
                    {
                        (int e_HP, string e_NAME, int e_DAMAGE) = enemy.Create();
                        Battle(e_NAME, e_HP, e_DAMAGE, player, inventory);
                        position++;
                        Thread.Sleep(50);
                    }
                    else if (chance <= 20 && chance >= 11)
                    {
                        position++;
                        UtilItemStore(player, inventory);
                    }
                    else if (chance >= 135)
                    {
                        position++;
                        GearItemStore(player, inventory);
                    }
                    else
                    {
                        position++;
                        Thread.Sleep(50);
                    }


                }
                else if (input == "a")
                {
                    int a_chance = random.Next(0, 100);
                    if (a_chance <= 7)
                    {

                        (int e_HP, string e_NAME, int e_DAMAGE) = enemy.Create();
                        Battle(e_NAME, e_HP, e_DAMAGE, player, inventory);
                        position++;
                        Thread.Sleep(50);
                    }
                    else
                    {
                        position++;
                        Thread.Sleep(50);
                    }
                }
                else if (input == "save")
                {
                    savePositionVar();
                    Console.Clear();
                    Console.WriteLine("saving..");
                    MainMenu(player);
                }
                else if (input == "inv" || input == "inventory")
                {
                    InventoryIngame(player, inventory);
                        
                }
                else
                {
                    GameLoop(player, inventory);
                }
            }
        }

        static void savePositionVar()
        {
            try
            {
                if (position > highscore)
                {
                    File.WriteAllText(savePosition, position.ToString());
                }
               
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while saving position: {ex.Message}");
            }
        }

        static void readPositionVar()
        {
            try
            {
                // Check if the file exists
                if (File.Exists(savePosition))
                {
                    // Read the content directly using File.ReadAllText
                    string s = File.ReadAllText(savePosition);
                    highscore = int.Parse(s);
                }
                else
                {
                    // If the file doesn't exist, create it and initialize the value
                    File.WriteAllText(savePosition, position.ToString());
                    highscore = position; // Set highscore to the initial position
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while reading position: {ex.Message}");
            }
        }


        static void InventoryIngame(Player player, Inventory inventory)
        {
            while (true)
            {
                DisplayInventory(inventory);
                Console.Write("\nEnter item to consume: ");
                if (inventory.APPLES >= 1 || inventory.HEALING_POTIONS >= 1 || inventory.DAMAGE_BOOSTERS >= 1 || inventory.SMALL_BANDAGES >= 1)
                {
                    string item_to_consume = Console.ReadLine();
                    if (item_to_consume == "apple")
                    {
                        if (inventory.APPLES >= 1)
                        {
                            if (AppleCooldown == false)
                            {
                                inventory.APPLES -= 1;

                                int old_hp = player.HP;
                                player.HP += 20;
                                Task.Run(() => AppleCooldownFunction());
                                Console.WriteLine($"\nOld HP: {old_hp}  New HP: {player.HP}");
                                Thread.Sleep(1000);
                            }
                            else
                            {
                                Console.WriteLine("\nAction is in cooldown! try again in a few seconds.");
                                Thread.Sleep(1000);
                            }
                        }
                        else
                        {
                            Console.WriteLine("Insufficient amount of Apples!");
                            Thread.Sleep(1000);
                        }
                    }

                    if (item_to_consume == "healing potion")
                    {
                        if (inventory.HEALING_POTIONS >= 1)
                        {
                            if (HealingPotionCooldown == false)
                            {
                                inventory.HEALING_POTIONS -= 1;
                                int old_hp = player.HP;
                                player.HP += 30;
                                Task.Run(() => HealingPotionCooldownFunction());
                                Console.WriteLine($"\nOld HP: {old_hp}  New HP: {player.HP}");
                                Thread.Sleep(1000);
                            }
                            else
                            {
                                Console.WriteLine("\nAction is in cooldown! try again in a few seconds.");
                                Thread.Sleep(1000);
                            }
                        }
                        else
                        {
                            Console.WriteLine("Insufficient amount of Healing Potions!");
                            Thread.Sleep(1000);
                        }
                    }

                    if (item_to_consume == "damage booster")
                    {
                        if (inventory.DAMAGE_BOOSTERS >= 1)
                        {
                            if (DamageBoosterCooldown == false)
                            {
                                inventory.DAMAGE_BOOSTERS -= 1;
                                Task.Run(() => DamageBoosterCooldownFunction());
                                Task.Run(() => DamageBoosterToggleActive());
                                Console.WriteLine($"Damage booster Activated for 40 seconds.");
                                Thread.Sleep(1000);
                            }
                            else
                            {
                                Console.WriteLine("\nAction is in cooldown! try again in a bit.");
                                Thread.Sleep(1000);
                            }
                        }
                        else
                        {
                            Console.WriteLine("Insufficient amount of Damage Boosters!");
                            Thread.Sleep(1000);
                        }
                    }

                    if (item_to_consume == "small bandage")
                    {
                        if (inventory.SMALL_BANDAGES >= 1)
                        {
                            if (SmallBandageCooldown == false)
                            {
                                inventory.SMALL_BANDAGES -= 1;

                                int old_hp = player.HP;
                                player.HP += 10;
                                Task.Run(() => SmallBandageCooldownFunction());
                                Console.WriteLine($"\nOld HP: {old_hp}  New HP: {player.HP}");
                                Thread.Sleep(1000);
                            }
                            else
                            {
                                Console.WriteLine("\nAction is in cooldown! try again in a few seconds.");
                                Thread.Sleep(1000);
                            }
                        }
                        else
                        {
                            Console.WriteLine("Insufficient amount of small bandages!");
                            Thread.Sleep(1000);
                        }
                    }

                    if (item_to_consume == "exit")
                    {
                        break;
                    }
                }
                else
                {
                    Console.WriteLine("Your inventory is empty.");
                    Console.ReadKey();
                    break;
                }
            }
        }


        /*
        static void InventoryIngame(Player player, Inventory inventory)
        {
            while (true)
            {
                DisplayInventory(inventory);
                Console.Write("\nEnter item to consume: ");
                if (inventory.APPLES >= 1 || inventory.HEALING_POTIONS >= 1 || inventory.DAMAGE_BOOSTERS >= 1)
                {
                    string item_to_consume = Console.ReadLine();
                    if (item_to_consume == "apple")
                    {
                        if (inventory.APPLES >= 1)
                        {
                            if (AppleCooldown == false)
                            {
                                inventory.APPLES -= 1;

                                int old_hp = player.HP;
                                player.HP += 10;
                                Task.Run(() => AppleCooldownFunction());
                                Console.WriteLine($"\nOld HP: {old_hp}  New HP: {player.HP}");
                                Thread.Sleep(1000);
                            }
                            else
                            {
                                Console.WriteLine("\nAction is in cooldown! try again in a few seconds.");
                                Thread.Sleep(1000);
                            }


                        }
                        else
                        {
                            Console.WriteLine("Insufficient amount of Apples!");
                            Thread.Sleep(1000);
                        }
                    }

                    if (item_to_consume == "healing potion")
                    {
                        if (inventory.HEALING_POTIONS >= 1)
                        {
                            if (HealingPotionCooldown == false)
                            {
                                inventory.HEALING_POTIONS -= 1;
                                int old_hp = player.HP;
                                player.HP += 30;
                                Task.Run(() => HealingPotionCooldownFunction());
                                Console.WriteLine($"\nOld HP: {old_hp}  New HP: {player.HP}");
                                Thread.Sleep(1000);
                            }
                            else
                            {
                                Console.WriteLine("\nAction is in cooldown! try again in a few seconds.");
                                Thread.Sleep(1000);
                            }
                        }
                        else
                        {
                            Console.WriteLine("Insufficient amount of Healing Potions!");
                            Thread.Sleep(1000);
                        }
                    }

                    if (item_to_consume == "damage booster")
                    {
                        if (inventory.DAMAGE_BOOSTERS >= 1)
                        {
                            if (DamageBoosterCooldown == false)
                            {
                                inventory.DAMAGE_BOOSTERS -= 1;
                                Task.Run(() => DamageBoosterCooldownFunction());
                                Task.Run(() => DamageBoosterToggleActive());
                                Console.WriteLine($"Damage booster Activated for 40 seconds.");
                                Thread.Sleep(1000);
                            }
                            else
                            {
                                Console.WriteLine("\nAction is in cooldown! try again in a bit.");
                                Thread.Sleep(1000);
                            }
                        }
                        else
                        {
                            Console.WriteLine("Insufficient amount of Damage Boosters!");
                            Thread.Sleep(1000);
                        }
                    }
                    if (item_to_consume == "exit")
                    {
                        break;  
                    }
                }
                else if (inventory.APPLES <= 0 || inventory.HEALING_POTIONS <= 0 || inventory.DAMAGE_BOOSTERS <= 0)
                {
                    DisplayInventory(inventory);
                }
            }
        }
        */

        //public abstract extern static sealed protected internal override private unsafe virtual async new const ref readonly int MyMethodIsYourMethod();




        static void AppleCooldownFunction()
        {
            AppleCooldown = true;
            Thread.Sleep(10000);
            AppleCooldown = false;
        }

        static void HealingPotionCooldownFunction()
        {
            HealingPotionCooldown = true;
            Thread.Sleep(20000);
            HealingPotionCooldown = false;
        }

        static void SmallBandageCooldownFunction()
        {
            SmallBandageCooldown = true;
            Thread.Sleep(10000);
            HealingPotionCooldown = false;
        }

        static void DamageBoosterCooldownFunction()
        {
            DamageBoosterCooldown = true;
            Thread.Sleep(41000);
            DamageBoosterCooldown = false;
        }

        static void DamageBoosterToggleActive()
        {
            using_damage_booster = true;
            Thread.Sleep(40000);
            using_damage_booster = false;
        }

        static void DisplayInventory(Inventory inventory)
        {
            Console.Clear();
            Console.WriteLine("Inventory:\n");
            Console.WriteLine("- Gear -\n");
            Console.WriteLine($"Weapon: {inventory.WEAPON}");
            Console.WriteLine("\n- Items -\n");
            if (inventory.APPLES >= 1)
            {
                Console.WriteLine($"{inventory.APPLES}x Apple(s)\n");
            }
            if (inventory.HEALING_POTIONS >= 1)
            {
                Console.WriteLine($"{inventory.HEALING_POTIONS}x Healing Potion(s)\n");
            }
            if (inventory.DAMAGE_BOOSTERS >= 1)
            {
                Console.WriteLine($"{inventory.DAMAGE_BOOSTERS}x Damage Booster(s)\n");
            }
            if (inventory.SMALL_BANDAGES >= 1)
            {
                Console.WriteLine($"{inventory.SMALL_BANDAGES}x Small Bandage(s)\n");
            }
            if (inventory.APPLES <= 0 && inventory.HEALING_POTIONS <= 0 && inventory.DAMAGE_BOOSTERS <= 0 && inventory.SMALL_BANDAGES <= 0)
            {
                Console.WriteLine("  Empty\n");
            }
            Console.WriteLine("- Stats -");
            Console.WriteLine($"\nName: {player.NAME}");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Hp:     {player.HP}");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Gold:   {player.GOLD}");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"XP:     {player.XP}\n");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("- End -");

        }



        //////////////////////////////////////////////////////////////////////////////////////////      COMBAT SYSTEM






        static void Battle(string e_NAME, int e_HP, int e_DAMAGE, Player player, Inventory inventory)
        {
            Console.Clear();
            int round = 0;
            Console.Title = $"Fight - Round {round}";

            int fight_starting_hp = player.HP;

            while (player.HP > 0 && enemy.HP > 0)
            {
                round++;
                
                MainBattleScreen(e_NAME, e_HP, e_DAMAGE, player, inventory);

                string input_main = Console.ReadLine();
                if (input_main == "1")
                {
                    AttacksMenu();
                    string input_attacks_menu = Console.ReadLine();

                    if (input_attacks_menu == "1")
                    {
                        Console.SetCursorPosition(47, 18);
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("Going for an attack..");
                        Console.ForegroundColor = ConsoleColor.White;
                        Thread.Sleep(1500);
                        int normal_attack_chance = random.Next(0, 100);
                        if (normal_attack_chance <= 90)
                        {
                            Console.SetCursorPosition(41, 20);
                            PlayerAttack(e_NAME, e_HP, e_DAMAGE, player, inventory, false);
                            Console.WriteLine($"Sucess! dealt {player.DAMAGE()} damage to {e_NAME}.");
                            Console.SetCursorPosition(42, 22);
                            Console.WriteLine($"{player.NAME}'s HP: {player.HP}   {e_NAME}'s HP: {enemy.HP}");
                            if (enemy.HP >= 1)
                            {
                                Thread.Sleep(3000);
                                Console.SetCursorPosition(44, 24);
                                Console.WriteLine($"{e_NAME} now tries to attack!");
                                Thread.Sleep(2000);
                                EnemyAttack(e_NAME, e_HP, e_DAMAGE, player, inventory);
                                Thread.Sleep(2000);
                            }
                        }
                        else
                        {
                            Console.SetCursorPosition(40, 20);
                            Console.WriteLine($"Failed.. {e_NAME} now tries to attack!");
                            Thread.Sleep(2000);
                            Console.SetCursorPosition(35, 22);
                            EnemyAttack(e_NAME, e_HP, e_DAMAGE, player, inventory);
                            Console.SetCursorPosition(41, 24);
                            Console.WriteLine($"{player.NAME}'s HP: {player.HP}   {e_NAME}'s HP: {enemy.HP}");
                            Thread.Sleep(2000);
                        }
                    }
                    else if (input_attacks_menu == "2")
                    {
                        Console.SetCursorPosition(45, 18);
                        Console.ForegroundColor = ConsoleColor.DarkCyan;
                        Console.WriteLine("Going for an super attack..");
                        Console.ForegroundColor = ConsoleColor.White;
                        Thread.Sleep(1500);
                        int super_attack_chance = random.Next(0, 100);
                        if (super_attack_chance <= 30)
                        {
                            Console.SetCursorPosition(41, 20);
                            int e_hp_before = enemy.HP;
                            PlayerAttack(e_NAME, e_HP, e_DAMAGE, player, inventory, true);
                            int e_hp_after = e_hp_before - enemy.HP;
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"Sucess! dealt {e_hp_after} damage to {e_NAME}.");
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.SetCursorPosition(44, 22);
                            Console.WriteLine($"{player.NAME}'s HP: {player.HP}   {e_NAME}'s HP: {enemy.HP}");
                            Thread.Sleep(3000);
                            if (enemy.HP >= 1)
                            {
                                Console.SetCursorPosition(42, 24);
                                Console.WriteLine($"{e_NAME} now tries to attack!");
                                Thread.Sleep(2000);
                                EnemyAttack(e_NAME, e_HP, e_DAMAGE, player, inventory);
                                Thread.Sleep(2000);
                            }
                        }
                        else
                        {
                            Console.SetCursorPosition(34, 20);
                            Console.WriteLine($"Super Attack Failed.. {e_NAME} now tries to attack!");
                            Thread.Sleep(3000);
                            EnemyAttack(e_NAME, e_HP, e_DAMAGE, player, inventory);
                            Console.SetCursorPosition(41, 22);
                            Console.WriteLine($"{player.NAME}'s HP: {player.HP}   {e_NAME}'s HP: {enemy.HP}");
                            Thread.Sleep(3000);
                        }
                    }
                }
                else if (input_main == "2")
                {
                    TryToRun();
                }
                else if (input_main == "inv" || input_main == "inventory")
                {
                    InventoryIngame(player, inventory);
                }
                else
                {

                }
            }

            if (player.HP <= 0)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.SetCursorPosition(40, 15);
                SlowWriteLine($"You died! you survived for {round} rounds.", 20);
                Console.ForegroundColor = ConsoleColor.White;
                Console.ReadKey();
                Console.Clear();
                Thread.Sleep(3000);
                MainMenu(player);
            }
            else if (enemy.HP <= 0)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.SetCursorPosition(45, 18);
                SlowWriteLine($"You killed {e_NAME}! it took {round} rounds.", 20);
                Console.ForegroundColor = ConsoleColor.White;
                GiveRewards(e_NAME, fight_starting_hp);
            }
                
        }

        static void AttacksMenu()
        {
            Console.Clear();
            Console.SetCursorPosition(57, 12);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{player.NAME}");

            Console.SetCursorPosition(52, 13);
            Console.WriteLine($"HP: {player.HP} DAMAGE: {player.DAMAGE()}\n");
            Console.ForegroundColor = ConsoleColor.Cyan;
            
            Console.SetCursorPosition(42, 15);
            Console.Write("1. Normal Attack");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write("   2. Super Attack");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.SetCursorPosition(62, 16);
            Console.WriteLine("(30% Chance)");
            Console.SetCursorPosition(43, 16);
            Console.WriteLine("(90% Chance)");
            Console.ForegroundColor = ConsoleColor.White;
        }

        static void MainBattleScreen(string e_NAME, int e_HP, int e_DAMAGE, Player player, Inventory inventory)
        {
            Console.Clear();
            Console.SetCursorPosition(40, 12);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{player.NAME} (you): HP: {player.HP}, DAMAGE: {player.DAMAGE()} [ {inventory.WEAPON} ]");

            Console.SetCursorPosition(42, 13);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"{e_NAME} (enemy): HP: {enemy.HP}, DAMAGE: {e_DAMAGE}");
            Console.ForegroundColor = ConsoleColor.White;

            //Console.SetCursorPosition(42, 15);
            //Console.WriteLine("1. Show Attacks    2. Try to run");

            // Console.Clear();

            /*
            Console.SetCursorPosition(48, 12);
            Console.WriteLine($"{player.NAME}");

            Console.SetCursorPosition(48, 13);
            Console.WriteLine($"HP: {player.HP} DAMAGE: {player.DAMAGE()}\n");
            */

            Console.SetCursorPosition(42, 15);
            Console.WriteLine("1. Show Attacks    2. Try to run");
        }

        static int PlayerAttack(string e_NAME, int e_HP, int e_DAMAGE, Player player, Inventory inventory, bool super_attack)
        {
            Random _rand = new();

            if (super_attack == true)
            {
                enemy.HP -= player.DAMAGE() + _rand.Next(5, 7);
                return enemy.HP;
            }
            else
            {
                enemy.HP -= player.DAMAGE();
                return enemy.HP;
            }
        }

        static int EnemyAttack(string e_NAME, int e_HP, int e_DAMAGE, Player player, Inventory inventory)
        {
            Random random_ = new();
            int chance = random_.Next(0, 100);
            if (chance <= 80)
            {
                int old_hp = player.HP;
                player.HP -= e_DAMAGE;

                total_enemy_damage += e_DAMAGE;

                Console.SetCursorPosition(30, 26);
                Console.WriteLine($"Ouch! the enemy dealt {e_DAMAGE} damage! Before: {old_hp}HP  After: {player.HP}");
                return player.HP;
            }
            else
            {
                Console.SetCursorPosition(30, 26);
                Console.WriteLine($"{e_NAME}'s attack failed! select your attack now again.");
                return player.HP;
            }
        }
        
        static void GiveRewards(string e_NAME, int fight_starting_hp)
        {
            int gold_reward;
            int xp_reward;

            switch (e_NAME)
            {
                case "Firon":
                    gold_reward = random.Next(2, 3);
                    xp_reward = random.Next(42, 57);
                    printRewards(gold_reward, xp_reward);
                    total_enemy_damage = 0;
                    player.GOLD += gold_reward;
                    player.XP += xp_reward;
                    Console.ReadKey();
                    break;
                case "Syvor":
                    gold_reward = random.Next(4, 5);
                    xp_reward = random.Next(57, 92);
                    printRewards(gold_reward, xp_reward);
                    total_enemy_damage = 0;
                    player.GOLD += gold_reward;
                    player.XP += xp_reward;
                    Console.ReadKey();
                    break;
                case "Glyth":
                    gold_reward = random.Next(6, 7);
                    xp_reward = random.Next(92, 143);
                    printRewards(gold_reward, xp_reward);
                    total_enemy_damage = 0;
                    player.GOLD += gold_reward;
                    player.XP += xp_reward;
                    Console.ReadKey();
                    break;
                case "Xylon":
                    gold_reward = random.Next(8, 9);
                    xp_reward = random.Next(143, 187);
                    printRewards(gold_reward, xp_reward);
                    total_enemy_damage = 0;
                    player.GOLD += gold_reward;
                    player.XP += xp_reward;
                    Console.ReadKey();
                    break;
                case "Mylar":
                    gold_reward = random.Next(10, 11);
                    xp_reward = random.Next(187, 215);
                    printRewards(gold_reward, xp_reward);
                    total_enemy_damage = 0;
                    player.GOLD += gold_reward;
                    player.XP += xp_reward;
                    Console.ReadKey();
                    break;
                case "Kivak":
                    gold_reward = random.Next(12, 13);
                    xp_reward = random.Next(215, 260);
                    printRewards(gold_reward, xp_reward);
                    total_enemy_damage = 0;
                    player.GOLD += gold_reward;
                    player.XP += xp_reward;
                    Console.ReadKey();
                    break;
                case "Drayn":
                    gold_reward = random.Next(14, 15);
                    xp_reward = random.Next(260, 300);
                    printRewards(gold_reward, xp_reward);
                    total_enemy_damage = 0;
                    player.GOLD += gold_reward;
                    player.XP += xp_reward;
                    Console.ReadKey();
                    break;
                case "Zarok":
                    gold_reward = random.Next(16, 17);
                    xp_reward = random.Next(300, 400);
                    printRewards(gold_reward, xp_reward);
                    total_enemy_damage = 0;
                    player.GOLD += gold_reward;
                    player.XP += xp_reward;
                    Console.ReadKey();
                    break;
                case "Wyrik":
                    gold_reward = random.Next(18, 19);
                    xp_reward = random.Next(400, 500);
                    printRewards(gold_reward, xp_reward);
                    total_enemy_damage = 0;
                    player.GOLD += gold_reward;
                    player.XP += xp_reward;
                    Console.ReadKey();
                    break;
                case "Hydra":
                    gold_reward = random.Next(20, 21);
                    xp_reward = random.Next(500, 650);
                    printRewards(gold_reward, xp_reward);
                    total_enemy_damage = 0;
                    player.GOLD += gold_reward;
                    player.XP += xp_reward;
                    Console.ReadKey();
                    break;

            }
        }

       static void printRewards(int gold_reward, int xp_reward)
        {
            Console.SetCursorPosition(51, 19);
            Console.WriteLine($"Rewards:");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.SetCursorPosition(51, 21);
            Console.Write($"+");
            Console.SetCursorPosition(52, 21);
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write($"  {gold_reward} Gold\n");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.SetCursorPosition(51, 22);
            Console.Write($"+");
            Console.SetCursorPosition(52, 22);
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write($" {xp_reward} XP\n");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.SetCursorPosition(51, 23);
            Console.Write($"-");
            Console.SetCursorPosition(52, 23);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($" {total_enemy_damage} HP");
        }

        static void TryToRun()
        {
            Console.Clear();
            Console.SetCursorPosition(48, 15);
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("You try to run..");
            Console.ForegroundColor = ConsoleColor.White;
            Thread.Sleep(2000);
            int run = random.Next(1, 4);
            if (run == 2)
            {
                Console.SetCursorPosition(43, 17);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Sucess! you ran away");
                Console.ForegroundColor = ConsoleColor.White;
                Thread.Sleep(2000);
                GameLoop(player, inventory);
            }
            else
            {
                Console.SetCursorPosition(36, 17);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("You didnt manage to run away, and got caught!");
                Console.ForegroundColor = ConsoleColor.White;
                Thread.Sleep(3000);

            }
        }

        


        //////////////////////////////////////////////////////////////////////////////////////////      COMBAT SYSTEM




        static void UtilItemStore(Player player, Inventory inventory)
        {
            while (true)
            {
                UtilItemStoreUI();
                Console.SetCursorPosition(71, 21);

                string product = Console.ReadLine();
                if (product == "1")
                {
                    if (player.GOLD >= 15)
                    {
                        inventory.APPLES += 1;
                        player.GOLD -= 15;
                        Console.WriteLine("You bought 1x apple (-15 gold)");
                        Thread.Sleep(2000);
                        // TODO: OTHER ITEMS AND WRITELINE STATEMENTS
                    }
                    else
                    {
                        Console.WriteLine($"Insuficcent gold! you have {player.GOLD} gold, required is 20 gold!");
                        Thread.Sleep(2000);
                    }
                }

                else if (product == "2")
                {
                    if (player.GOLD >= 20)
                    {
                        inventory.DAMAGE_BOOSTERS += 1;
                        player.GOLD -= 20;
                        Console.WriteLine("You bought 1x damage booster (-20 gold)");
                        Thread.Sleep(2000);
                    }
                    else
                    {
                        Console.WriteLine($"Insuficcent gold! you have {player.GOLD} gold, required is 20 gold!");
                        Thread.Sleep(2000);
                    }
                }

                else if (product == "3")
                {
                    if (player.GOLD >= 25)
                    {
                        inventory.HEALING_POTIONS += 1;
                        player.GOLD -= 25;
                        Console.WriteLine("You bought 1x healing potion (-25 gold)");
                        Thread.Sleep(2000);
                    }
                    else
                    {
                        Console.WriteLine($"Insuficcent gold! you have {player.GOLD} gold, required is 20 gold!");
                        Thread.Sleep(2000);
                    }
                }

                else if (product == "4")
                {
                    if (player.GOLD >= 5)
                    {
                        inventory.SMALL_BANDAGES += 1;
                        player.GOLD -= 5;
                        Console.WriteLine("You bought 1x small bandage (-5 gold)");
                        Thread.Sleep(2000);
                    }
                    else
                    {
                        Console.WriteLine($"Insuficcent gold! you have {player.GOLD} gold, required is 5 gold!");
                        Thread.Sleep(2000);
                    }
                }

                else if (product == "exit")
                {
                    Console.Clear();
                    Console.WriteLine("Exiting store..");
                    Thread.Sleep(1000);
                    return;
                }
            }
            
        }

        static void GearItemStore(Player player, Inventory inventory)
        {
            while (true)
            {
                GearItemStoreUI();
                Console.SetCursorPosition(71, 21);

                string product = Console.ReadLine();
                if (product == "1")
                {
                    if (player.GOLD >= 35)
                    {
                        if (inventory.WEAPON != "Iron Sword")
                        {
                            inventory.WEAPON = "Iron Sword";
                            player.GOLD -= 35;
                            Console.WriteLine("Sucessfully bought an Iron Sword");
                            Thread.Sleep(2000);
                        }
                        else
                        {
                            Console.WriteLine("You already have that weapon!");
                            Thread.Sleep(2000);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Insuficcent gold, required: 35g");
                        Thread.Sleep(2000);
                    }
                }
                else if (product == "2")
                {
                    if (player.GOLD >= 40)
                    {
                        if (inventory.WEAPON != "Magic Wand")
                        {
                            inventory.WEAPON = "Magic Wand";
                            player.GOLD -= 40;
                            Console.WriteLine("Sucessfully bought the Magic Wand");
                            Thread.Sleep(2000);
                        }
                        else
                        {
                            Console.WriteLine("You already have that weapon!");
                            Thread.Sleep(2000);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Insuficcent gold, required: 40g");
                        Thread.Sleep(2000);
                    }
                }
                else if (product == "3")
                {
                    if (player.GOLD >= 50)
                    {
                        if (inventory.WEAPON != "Target Slayer")
                        {
                            inventory.WEAPON = "Target Slayer";
                            player.GOLD -= 35;
                            Console.WriteLine("Sucessfully bought the Target Slayer");
                            Thread.Sleep(2000);
                        }
                        else
                        {
                            Console.WriteLine("You already have that weapon!");
                            Thread.Sleep(2000);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Insuficcent gold, required: 50g");
                        Thread.Sleep(2000);
                    }
                }

                else if (product == "4")
                {
                    if (player.GOLD >= 50)
                    {
                        if (inventory.WEAPON != "Aetherclaw")
                        {
                            inventory.WEAPON = "Aetherclaw";
                            player.GOLD -= 35;
                            Console.WriteLine("Sucessfully bought the Aetherclaw");
                            Thread.Sleep(2000);
                        }
                        else
                        {
                            Console.WriteLine("You already have that weapon!");
                            Thread.Sleep(2000);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Insuficcent gold, required: 70g");
                        Thread.Sleep(2000);
                    }
                }

                else if (product == "exit")
                {
                    Console.Clear();
                    Console.WriteLine("Exiting store..");
                    Thread.Sleep(1000);
                    return;
                }
            }
        }

        static void UtilItemStoreUI()
        {
            Console.Clear();
            Console.Title = "Util Store - buying items";

            Console.SetCursorPosition(45, 12);
            Console.WriteLine("(------- Item Store -------)");
            Console.SetCursorPosition(45, 14);
            Console.WriteLine($"Gold: {player.GOLD}");
            Console.SetCursorPosition(45, 16);
            Console.WriteLine(" 1 - 1x Apple           15g");
            Console.SetCursorPosition(45, 17);
            Console.WriteLine(" 2 - 1x Damage booster  20g");
            Console.SetCursorPosition(45, 18);
            Console.WriteLine(" 3 - 1x Healing Potion  25g");
            Console.SetCursorPosition(45, 19);
            Console.WriteLine(" 4 - 1x Small Bandage    5g");
            Console.SetCursorPosition(45, 21);
            Console.Write("Enter Product, or 'exit': ");
        }


        static void GearItemStoreUI()
        {
            Console.Clear();
            Console.Title = "Gear Store - buying items";

            Console.SetCursorPosition(45, 12);
            Console.WriteLine("(--------- Gear Store ---------)");
            Console.SetCursorPosition(45, 14);
            Console.WriteLine($"Gold: {player.GOLD}");
            Console.SetCursorPosition(45, 16);
            Console.WriteLine(" 1 - Iron Sword (4-7)      35g");
            Console.SetCursorPosition(45, 17);
            Console.WriteLine(" 2 - Magic Wand (6-9)      40g");
            Console.SetCursorPosition(45, 18);
            Console.WriteLine(" 3 - Target Slayer (8-12)  50g");
            Console.SetCursorPosition(45, 19);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(" 4 - Aetherclaw (11-15)    70g");
            Console.ForegroundColor = ConsoleColor.White;
            Console.SetCursorPosition(45, 21);
            Console.Write("Enter Product, or 'exit': ");
        }



        static void SlowWriteLine(string text, int delay)
        {
            foreach (char c in text)
            {
                Console.Write(c);
                Thread.Sleep(delay);
            }
        }
    }
}
