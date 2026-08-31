using NotRunescape;
using OsrsTracker;

var bossLogs = new List<BossLog>();
var player = new Player();

Console.WriteLine("=== OSRS Boss & Combat Tracker ===");
int DropCounter = 0;

Console.WriteLine("Name:\n");
string name = Console.ReadLine();
while (name == null || name.Trim().Length == 0)
{
    Console.Write("Name cannot be empty");
    Console.Write("Name: \n");
    name = Console.ReadLine();
}
Console.Write($"Hello {name}");
while (true)
{
    Console.WriteLine($"\n[HP: {player.CurrentHp}/{player.MaxHp} | Gold: {player.StartingGold} GP]");
    Console.Write("[1] Log Boss Kill  [2] View Drop Log  [3] View Inventory  [4] Drop Item  [99] Fight Hill Giant  [0] Exit\nChoice: ");
    var input = Console.ReadLine()?.Trim();

    if (input == "0") break;

    if (input == "1")
    {
        Console.Write("Boss Name (e.g., Zulrah, Vorkath): ");
        string boss = Console.ReadLine() ?? "Unknown";

        Console.Write("Valuable Drop (e.g., Tanzanite Fang, None): ");
        string drop = Console.ReadLine() ?? "None";

        Console.Write("Did you get a unique drop? (y/n): ");
        bool isUnique = Console.ReadLine()?.Trim().ToLower() == "y";

        bossLogs.Add(new BossLog { BossName = boss, DropName = drop, IsUnique = isUnique });
        Console.WriteLine("Kill logged!");
    }
    else if (input == "2")
    {
        Console.WriteLine("\n--- Drop Log ---");
        DropCounter++;
        Console.WriteLine($"Numbers of Dropped: {DropCounter}.");
        if (bossLogs.Count == 0) Console.WriteLine("No drops logged yet!");
        for (int i = 0; i < bossLogs.Count; i++)
        {
            var log = bossLogs[i];
            string status = log.IsUnique ? "UNIQUE DROP!" : "Normal Drop";
            Console.WriteLine($"#{i + 1}: {log.BossName} - Drop: {log.DropName} [{status}] ({log.Timestamp:HH:mm})");
        }
    }
    else if (input == "3")
    {
        player.PrintInventory();
    }
    else if (input == "4")
    {
        HandleDropItem(player);
    }
    else if (input == "5")
    {
        player.CurrentHp = player.MaxHp;
    }
    else if (input == "99")
    {
        StartGiantFight(player, bossLogs);
    }
}

static void HandleDropItem(Player player)
{
    player.PrintInventory();
    if (player.Inventory.Count == 0) return;

    Console.Write("\nEnter the exact name of the item to drop: ");
    string itemToDrop = Console.ReadLine()?.Trim() ?? "";

    Console.Write("How many to drop?: ");
    if (int.TryParse(Console.ReadLine(), out int amount) && amount > 0)
    {
        if (player.DropItem(itemToDrop, amount))
        {
            Console.WriteLine($"Dropped {amount}x {itemToDrop}.");
        }
        else
        {
            Console.WriteLine("You don't have enough of that item to drop.");
        }
    }
    else
    {
        Console.WriteLine("Invalid amount.");
    }
}

static void StartGiantFight(Player player, List<BossLog> bossLogs)
{
    if (player.CurrentHp <= 0)
    {
        Console.WriteLine("\nYou are too weak to fight! Respawning at Lumbridge...");
        player.CurrentHp = player.MaxHp;
        return;
    }

    Console.Clear();
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("=== HILL GIANT CAVE ===");
    Console.WriteLine("A wild Hill Giant (Level 28) blocks your path!\n");
    Console.ResetColor();

    int giantHp = 35;
    var rng = new Random();

    while (player.CurrentHp > 0 && giantHp > 0)
    {
        Console.WriteLine($"Your HP: {player.CurrentHp}/{player.MaxHp} | Hill Giant HP: {giantHp}");
        Console.Write("Action: [1] Slash with Rune Scimitar  [2] Eat Lobster\nChoice: ");
        var choice = Console.ReadLine()?.Trim();

        if (choice == "1")
        {
            int playerHit = rng.Next(0, 15);
            giantHp -= playerHit;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\nYou slash the Hill Giant for a {playerHit}!");
            Console.ResetColor();
        }
        else if (choice == "2")
        {
            if (player.Inventory.ContainsKey("Lobster") && player.Inventory["Lobster"] > 0)
            {
                player.Inventory["Lobster"]--;
                player.CurrentHp = Math.Min(player.MaxHp, player.CurrentHp + 12);
                Console.WriteLine($"\nYou ate a Lobster! Restored HP to {player.CurrentHp}.");
            }
            else
            {
                Console.WriteLine("\nYou don't have any Lobsters in your inventory!");
            }
        }

        if (giantHp > 0)
        {
            int giantHit = rng.Next(0, 6);
            player.CurrentHp -= giantHit;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"The Hill Giant swings his club for {giantHit} damage!\n");
            Console.ResetColor();
        }
    }

    if (player.CurrentHp > 0)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("\nVICTORY! The Hill Giant collapses!");
        Console.ResetColor();

        // Selective Loot Prompt
        var droppedItems = new List<(string Name, bool IsUnique)>
        {
            ("Big Bones", false),
            ("Limpwurt Root", false),
            ("Giant Key", true)
        };

        Console.WriteLine("\n--- Ground Loot ---");
        foreach (var drop in droppedItems)
        {
            Console.Write($"Pick up {drop.Name}? (y/n): ");
            var choice = Console.ReadLine()?.Trim().ToLower();

            if (choice == "y")
            {
                player.AddItem(drop.Name, 1);
                bossLogs.Add(new BossLog
                {
                    BossName = "Hill Giant",
                    DropName = drop.Name,
                    IsUnique = drop.IsUnique
                });
                Console.WriteLine($"Picked up 1x {drop.Name} and logged it!");
            }
            else
            {
                Console.WriteLine($"Left {drop.Name} on the ground.");
            }
        }
    }
    else
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("\nOh dear, you are dead! Teleporting back to Lumbridge...");
        player.CurrentHp = player.MaxHp;
        Console.ResetColor();
    }
}