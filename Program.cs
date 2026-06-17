using System.Globalization;

using System;
using System.Globalization;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;

// Overpriced Coffee Simulator 2026
// A fully working console app, lovingly architected like it was finished at 1:47 AM before a demo.

Console.OutputEncoding = System.Text.Encoding.UTF8;

Console.WriteLine("===========================================");
Console.WriteLine("     Overpriced Coffee Simulator 2026");
Console.WriteLine("===========================================");
Console.WriteLine();

// 1. SET UP THE DI CONTAINER
// This acts as our "Host" or Bootstrap layer where we register all our services.
var serviceProvider = new ServiceCollection()
    // Register our settings as a Singleton. The DI container will create 
    // exactly ONE instance and share it with anyone who asks.
    .AddSingleton<LocalDatabaseSettings>()
    .BuildServiceProvider();

// 2. RETRIEVE OUR SINGLETON INSTANCE
// We ask the container for the single, shared instance of our settings.
var bootstrapSettings = serviceProvider.GetRequiredService<LocalDatabaseSettings>();

Console.WriteLine($"Connected to local store: {bootstrapSettings.StoreName}");
Console.WriteLine($"Today's bean tax multiplier: {bootstrapSettings.BeanTaxMultiplier:P0}");
Console.WriteLine();

Console.Write("Customer name: ");
string customerName = ReadTextOrDefault("Anonymous Investor");

Console.WriteLine();
Console.WriteLine("Choose your financially questionable beverage:");
Console.WriteLine("1. Espresso");
Console.WriteLine("2. Cappuccino");
Console.WriteLine("3. Latte");
Console.Write("Selection: ");
string coffeeChoice = ReadTextOrDefault("3");

Console.Write("Size (Small / Medium / Large / Ludicrous): ");
string size = ReadTextOrDefault("Medium");

Console.Write("Milk type (Whole / Skim / Oat / Almond / None): ");
string milkType = ReadTextOrDefault("Oat");

Console.Write("Espresso shots: ");
int espressoShots = ReadIntOrDefault(2);

Console.Write("Syrup type (None / Vanilla / Caramel / Hazelnut / Pumpkin): ");
string syrupType = ReadTextOrDefault("None");

Console.Write("Syrup shots: ");
int syrupShots = ReadIntOrDefault(0);

Console.Write("Whipped cream? (y/n): ");
bool hasWhippedCream = ReadYesNoOrDefault(false);

Console.Write("Sprinkles? (y/n): ");
bool hasSprinkles = ReadYesNoOrDefault(false);

Console.Write("Iced? (y/n): ");
bool isIced = ReadYesNoOrDefault(false);

Console.Write("Caramel drizzle? (y/n): ");
bool hasCaramelDrizzle = ReadYesNoOrDefault(false);

Console.Write("Extra hot? (y/n): ");
bool extraHot = ReadYesNoOrDefault(false);

Console.Write("Decaf? (y/n): ");
bool decaf = ReadYesNoOrDefault(false);

Console.Write("Loyalty level (None / Silver / Gold / Platinum): ");
string loyaltyLevel = ReadTextOrDefault("None");

Console.WriteLine();

Coffee coffee;

// [Factory smell] The caffeine hydra: creation logic hardcoded directly into the ordering flow.
// Add a new coffee type and enjoy editing this switch, the menu, pricing assumptions, probably your resume.
switch (coffeeChoice.Trim())
{
    case "1":
    case "espresso":
    case "Espresso":
        coffee = new Espresso(
            customerName,
            size,
            milkType,
            espressoShots,
            syrupType,
            syrupShots,
            hasWhippedCream,
            hasSprinkles,
            isIced,
            hasCaramelDrizzle,
            extraHot,
            decaf,
            loyaltyLevel,
            DateTime.Now);
        break;

    case "2":
    case "cappuccino":
    case "Cappuccino":
        coffee = new Cappuccino(
            customerName,
            size,
            milkType,
            espressoShots,
            syrupType,
            syrupShots,
            hasWhippedCream,
            hasSprinkles,
            isIced,
            hasCaramelDrizzle,
            extraHot,
            decaf,
            loyaltyLevel,
            DateTime.Now);
        break;

    case "3":
    case "latte":
    case "Latte":
    default:
        coffee = new Latte(
            customerName,
            size,
            milkType,
            espressoShots,
            syrupType,
            syrupShots,
            hasWhippedCream,
            hasSprinkles,
            isIced,
            hasCaramelDrizzle,
            extraHot,
            decaf,
            loyaltyLevel,
            DateTime.Now);
        break;
}

var printer = new OrderPrinter(bootstrapSettings);
printer.PrintOrder(coffee);

var checkout = new CheckoutService(bootstrapSettings);
checkout.Checkout(coffee);

var logger = new ReceiptLogger(bootstrapSettings);
logger.SaveReceipt(coffee);

Console.WriteLine();
Console.WriteLine("Thanks for visiting. Your coffee is ready, and so is your mortgage advisor.");

static string ReadTextOrDefault(string defaultValue)
{
    string? value = Console.ReadLine();
    return string.IsNullOrWhiteSpace(value) ? defaultValue : value.Trim();
}

static int ReadIntOrDefault(int defaultValue)
{
    string? value = Console.ReadLine();
    return int.TryParse(value, out int parsed) ? parsed : defaultValue;
}

static bool ReadYesNoOrDefault(bool defaultValue)
{
    string? value = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(value))
    {
        return defaultValue;
    }

    value = value.Trim().ToLowerInvariant();
    return value == "y" || value == "yes" || value == "true";
}

public class LocalDatabaseSettings
{
    public string StoreName { get; }
    public string CurrencyCode { get; }
    public decimal BeanTaxMultiplier { get; }
    public int OrdersProcessed { get; set; }

    // The constructor is now PUBLIC again because the DI container needs to call it.
    public LocalDatabaseSettings()
    {
        Console.WriteLine("[config] Reading local database settings via DI container... this should only happen once!");

        StoreName = "Bosch Campus Coffee Vault";
        CurrencyCode = "EUR";
        BeanTaxMultiplier = 0.21m;
        OrdersProcessed = 0;
    }
}

public abstract class Coffee
{
    public string CustomerName { get; }
    public string Name { get; }
    public string Size { get; }
    public string MilkType { get; }
    public int EspressoShots { get; }
    public string SyrupType { get; }
    public int SyrupShots { get; }
    public bool HasWhippedCream { get; }
    public bool HasSprinkles { get; }
    public bool IsIced { get; }
    public bool HasCaramelDrizzle { get; }
    public bool ExtraHot { get; }
    public bool Decaf { get; }
    public string LoyaltyLevel { get; }
    public DateTime OrderedAt { get; }
    public decimal BasePrice { get; }

    // [Builder smell] Constructor from Hell.
    // One more boolean and this becomes less of an API and more of a personality test.
    protected Coffee(
        string customerName,
        string name,
        string size,
        string milkType,
        int espressoShots,
        string syrupType,
        int syrupShots,
        bool hasWhippedCream,
        bool hasSprinkles,
        bool isIced,
        bool hasCaramelDrizzle,
        bool extraHot,
        bool decaf,
        string loyaltyLevel,
        DateTime orderedAt,
        decimal basePrice)
    {
        CustomerName = customerName;
        Name = name;
        Size = size;
        MilkType = milkType;
        EspressoShots = espressoShots;
        SyrupType = syrupType;
        SyrupShots = syrupShots;
        HasWhippedCream = hasWhippedCream;
        HasSprinkles = hasSprinkles;
        IsIced = isIced;
        HasCaramelDrizzle = hasCaramelDrizzle;
        ExtraHot = extraHot;
        Decaf = decaf;
        LoyaltyLevel = loyaltyLevel;
        OrderedAt = orderedAt;
        BasePrice = basePrice;
    }

    public decimal CalculateTotalPrice(LocalDatabaseSettings settings)
    {
        decimal total = BasePrice;

        // [Decorator smell] Every topping, modifier, and lifestyle choice lives here forever.
        // This method started as "just add oat milk" and is now a pricing-themed escape room.
        if (Size.Equals("Small", StringComparison.OrdinalIgnoreCase))
        {
            total += 0.00m;
        }
        else if (Size.Equals("Medium", StringComparison.OrdinalIgnoreCase))
        {
            total += 1.25m;
        }
        else if (Size.Equals("Large", StringComparison.OrdinalIgnoreCase))
        {
            total += 2.50m;
        }
        else if (Size.Equals("Ludicrous", StringComparison.OrdinalIgnoreCase))
        {
            total += 6.75m;

            if (EspressoShots > 3)
            {
                total += 2.00m;
            }
            else
            {
                total += 1.00m;
            }
        }
        else
        {
            total += 1.00m;
        }

        if (EspressoShots > 1)
        {
            int extraShots = EspressoShots - 1;

            if (Decaf)
            {
                total += extraShots * 1.10m;
            }
            else
            {
                if (extraShots >= 3)
                {
                    total += extraShots * 1.40m;
                }
                else
                {
                    total += extraShots * 1.25m;
                }
            }
        }

        if (MilkType.Equals("Oat", StringComparison.OrdinalIgnoreCase))
        {
            total += 1.30m;

            if (Size.Equals("Ludicrous", StringComparison.OrdinalIgnoreCase))
            {
                total += 0.80m;
            }
        }
        else if (MilkType.Equals("Almond", StringComparison.OrdinalIgnoreCase))
        {
            total += 1.10m;
        }
        else if (MilkType.Equals("Skim", StringComparison.OrdinalIgnoreCase))
        {
            total += 0.40m;
        }
        else if (MilkType.Equals("None", StringComparison.OrdinalIgnoreCase))
        {
            total -= 0.15m;
        }

        if (!SyrupType.Equals("None", StringComparison.OrdinalIgnoreCase) && SyrupShotsAreSuspiciouslyChargeable())
        {
            if (SyrupType.Equals("Pumpkin", StringComparison.OrdinalIgnoreCase))
            {
                total += SyrupShots * 0.95m;

                if (DateTime.Now.Month is 9 or 10 or 11)
                {
                    total += 1.50m;
                }
            }
            else if (SyrupType.Equals("Caramel", StringComparison.OrdinalIgnoreCase))
            {
                total += SyrupShots * 0.75m;
            }
            else
            {
                total += SyrupShots * 0.65m;
            }
        }

        if (HasWhippedCream)
        {
            total += 0.90m;

            if (HasSprinkles)
            {
                total += 0.55m;
            }
        }
        else
        {
            if (HasSprinkles)
            {
                total += 0.75m;
            }
        }

        if (IsIced)
        {
            total += 0.60m;

            if (ExtraHot)
            {
                total += 3.00m; // Physically confusing surcharge.
            }
        }
        else if (ExtraHot)
        {
            total += 0.35m;
        }

        if (HasCaramelDrizzle)
        {
            total += 0.85m;
        }

        if (Decaf)
        {
            total += 0.50m;
        }

        total += total * settings.BeanTaxMultiplier;

        if (LoyaltyLevel.Equals("Silver", StringComparison.OrdinalIgnoreCase))
        {
            total *= 0.98m;
        }
        else if (LoyaltyLevel.Equals("Gold", StringComparison.OrdinalIgnoreCase))
        {
            total *= 0.95m;
        }
        else if (LoyaltyLevel.Equals("Platinum", StringComparison.OrdinalIgnoreCase))
        {
            total *= 0.93m;
            total += 4.99m; // Premium loyalty maintenance fee. Loyalty has shareholders.
        }

        return Math.Round(total, 2);

        bool SyrupShotsAreSuspiciouslyChargeable()
        {
            return SyrupShots > 0;
        }
    }
}

public class Espresso : Coffee
{
    public Espresso(
        string customerName,
        string size,
        string milkType,
        int espressoShots,
        string syrupType,
        int syrupShots,
        bool hasWhippedCream,
        bool hasSprinkles,
        bool isIced,
        bool hasCaramelDrizzle,
        bool extraHot,
        bool decaf,
        string loyaltyLevel,
        DateTime orderedAt)
        : base(customerName, "Espresso", size, milkType, espressoShots, syrupType, syrupShots, hasWhippedCream,
            hasSprinkles, isIced, hasCaramelDrizzle, extraHot, decaf, loyaltyLevel, orderedAt, 3.80m)
    {
    }
}

public class Cappuccino : Coffee
{
    public Cappuccino(
        string customerName,
        string size,
        string milkType,
        int espressoShots,
        string syrupType,
        int syrupShots,
        bool hasWhippedCream,
        bool hasSprinkles,
        bool isIced,
        bool hasCaramelDrizzle,
        bool extraHot,
        bool decaf,
        string loyaltyLevel,
        DateTime orderedAt)
        : base(customerName, "Cappuccino", size, milkType, espressoShots, syrupType, syrupShots, hasWhippedCream,
            hasSprinkles, isIced, hasCaramelDrizzle, extraHot, decaf, loyaltyLevel, orderedAt, 4.90m)
    {
    }
}

public class Latte : Coffee
{
    public Latte(
        string customerName,
        string size,
        string milkType,
        int espressoShots,
        string syrupType,
        int syrupShots,
        bool hasWhippedCream,
        bool hasSprinkles,
        bool isIced,
        bool hasCaramelDrizzle,
        bool extraHot,
        bool decaf,
        string loyaltyLevel,
        DateTime orderedAt)
        : base(customerName, "Latte", size, milkType, espressoShots, syrupType, syrupShots, hasWhippedCream,
            hasSprinkles, isIced, hasCaramelDrizzle, extraHot, decaf, loyaltyLevel, orderedAt, 5.40m)
    {
    }
}

public class CheckoutService
{
    private readonly LocalDatabaseSettings _settings;

    // We add a constructor that accepts our dependency.
    public CheckoutService(LocalDatabaseSettings settings)
    {
        _settings = settings;
    }

    public void Checkout(Coffee coffee)
    {
        decimal total = coffee.CalculateTotalPrice(_settings);
        int cents = (int)Math.Round(total * 100m);

        Console.WriteLine();
        Console.WriteLine("Checkout");
        Console.WriteLine("--------");
        Console.WriteLine($"Total due: {total.ToString("C", CultureInfo.GetCultureInfo("de-DE"))}");

        // [Adapter smell] Directly married to an awkward third-party API.
        // Somewhere, a test double is trying to exist and quietly giving up.
        var paymentMachine = new LegacyPaymentMachine();
        string legacyResult = paymentMachine.ProcessTransactionInCents(cents, _settings.CurrencyCode);

        if (legacyResult.StartsWith("APPROVED", StringComparison.OrdinalIgnoreCase))
        {
            _settings.OrdersProcessed++;
            Console.WriteLine("Payment approved.");
            Console.WriteLine($"Local orders processed according to this brand-new settings object: {_settings.OrdersProcessed}");
        }
        else
        {
            Console.WriteLine("Payment failed. Please try again, or sell one of your extra monitors.");
        }
    }
}

public class LegacyPaymentMachine
{
    public string ProcessTransactionInCents(int cents, string currencyCode)
    {
        Thread.Sleep(400);

        if (cents <= 0)
        {
            return $"DECLINED|{currencyCode}|AMOUNT_TOO_LOW";
        }

        if (currencyCode != "EUR")
        {
            return $"DECLINED|{currencyCode}|CURRENCY_NOT_SUPPORTED";
        }

        return $"APPROVED|{currencyCode}|{cents}|AUTH-{DateTime.Now:HHmmss}";
    }
}

public class OrderPrinter
{
    private readonly LocalDatabaseSettings _settings;
    public OrderPrinter(LocalDatabaseSettings settings)
    {
        _settings = settings;
    }

    public void PrintOrder(Coffee coffee)
    {
        Console.WriteLine("Order Summary");
        Console.WriteLine("-------------");
        Console.WriteLine($"Store: {_settings.StoreName}");
        Console.WriteLine($"Customer: {coffee.CustomerName}");
        Console.WriteLine($"Drink: {coffee.Name}");
        Console.WriteLine($"Size: {coffee.Size}");
        Console.WriteLine($"Milk: {coffee.MilkType}");
        Console.WriteLine($"Espresso shots: {coffee.EspressoShots}");
        Console.WriteLine($"Syrup: {coffee.SyrupType} x{coffee.SyrupShots}");
        Console.WriteLine($"Whipped cream: {coffee.HasWhippedCream}");
        Console.WriteLine($"Sprinkles: {coffee.HasSprinkles}");
        Console.WriteLine($"Iced: {coffee.IsIced}");
        Console.WriteLine($"Caramel drizzle: {coffee.HasCaramelDrizzle}");
        Console.WriteLine($"Extra hot: {coffee.ExtraHot}");
        Console.WriteLine($"Decaf: {coffee.Decaf}");
        Console.WriteLine($"Loyalty: {coffee.LoyaltyLevel}");
    }
}

public class ReceiptLogger
{
    private readonly LocalDatabaseSettings _settings;
    public ReceiptLogger(LocalDatabaseSettings settings)
    {
        _settings = settings;
    }

    public void SaveReceipt(Coffee coffee)
    {
        decimal total = coffee.CalculateTotalPrice(_settings);

        Console.WriteLine();
        Console.WriteLine("[receipt-log]");
        Console.WriteLine($"Saved receipt for {coffee.CustomerName} at {_settings.StoreName}: {coffee.Name}, {total:0.00} {_settings.CurrencyCode}");
    }
}
