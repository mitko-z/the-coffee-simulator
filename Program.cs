using System.Globalization;

using Microsoft.Extensions.DependencyInjection;
using System;
using System.Globalization;
using System.Threading;
using System.Xml.Linq;

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

// 1. Gather all the options into the Builder fluently
var builder = new CoffeeBuilder()
    .ForCustomer(customerName)
    .WithSize(size)
    .WithMilk(milkType)
    .AddEspressoShots(espressoShots)
    .WithSyrup(syrupType, syrupShots) // We can package syrup type & shots together!
    .WithLoyalty(loyaltyLevel);

if(hasWhippedCream) builder.WithWhippedCream();
if(hasSprinkles) builder.WithSprinkles();
if(isIced) builder.Iced();
if(hasCaramelDrizzle) builder.WithCaramelDrizzle();
if(extraHot) builder.IsExtraHot();
if(decaf) builder.IsDecaf();

// 2. Ask the Factory to create the correct drink using our configured Builder
var factory = new CoffeeFactory();
Coffee coffee = factory.CreateCoffee(coffeeChoice, builder);



var printer = new OrderPrinter(bootstrapSettings);
printer.PrintOrder(coffee);

var legacyPaymentMachine = new LegacyPaymentMachine();
var paymentAdapter = new LegacyPaymentAdapter(legacyPaymentMachine, bootstrapSettings);
var checkout = new CheckoutService(paymentAdapter, bootstrapSettings);
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

public class CoffeeFactory
{
    // Look at how clean this signature is now!
    public Coffee CreateCoffee(string choice, CoffeeBuilder builder)
    {
        return choice.Trim().ToLower() switch
        {
            "1" or "espresso" => new Espresso(builder, DateTime.Now),
            "2" or "cappuccino" => new Cappuccino(builder, DateTime.Now),
            "3" or "latte" => new Latte(builder, DateTime.Now),
            _ => throw new ArgumentException($"We don't serve '{choice}'. Please order something else.")
        };
    }
}


public class CoffeeBuilder
{
    // Expose properties so the Coffee constructor can read them
    public string CustomerName { get; private set; } = "Anonymous";
    public string Name { get; private set; } = "Custom Coffee";
    public string Size { get; private set; } = "Medium";
    public string MilkType { get; private set; } = "Whole";
    public int EspressoShots { get; private set; } = 2;
    public string SyrupType { get; private set; } = "None";
    public int SyrupShots { get; private set; } = 0;
    public bool HasWhippedCream { get; private set; }
    public bool HasSprinkles { get; private set; }
    public bool IsIced { get; private set; }
    public bool HasCaramelDrizzle { get; private set; }
    public bool ExtraHot { get; private set; }
    public bool Decaf { get; private set; }
    public string LoyaltyLevel { get; private set; } = "None";

    public CoffeeBuilder ForCustomer(string name) { CustomerName = name; return this; }
    public CoffeeBuilder OfType(string name) { Name = name; return this; }
    public CoffeeBuilder WithSize(string size) { Size = size; return this; }
    public CoffeeBuilder WithMilk(string milkType) { MilkType = milkType; return this; }
    public CoffeeBuilder AddEspressoShots(int count) { EspressoShots = count; return this; }
    public CoffeeBuilder WithSyrup(string syrupType, int syrupShots) { SyrupType = syrupType; SyrupShots = syrupShots; return this; }
    public CoffeeBuilder WithWhippedCream() { HasWhippedCream = true; return this; }
    public CoffeeBuilder WithSprinkles() { HasSprinkles = true; return this; }
    public CoffeeBuilder Iced() { IsIced = true; return this; }
    public CoffeeBuilder WithCaramelDrizzle() { HasCaramelDrizzle = true; return this; }
    public CoffeeBuilder IsExtraHot() { ExtraHot = true; return this; }
    public CoffeeBuilder IsDecaf() { Decaf = true; return this; }
    public CoffeeBuilder WithLoyalty(string loyalty) { LoyaltyLevel = loyalty; return this; }
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
    protected Coffee(CoffeeBuilder builder, DateTime orderedAt, decimal basePrice)
    {
        CustomerName = builder.CustomerName;
        Name = builder.Name;
        Size = builder.Size;
        MilkType = builder.MilkType;
        EspressoShots = builder.EspressoShots;
        SyrupType = builder.SyrupType;
        SyrupShots = builder.SyrupShots;
        HasWhippedCream = builder.HasWhippedCream;
        HasSprinkles = builder.HasSprinkles;
        IsIced = builder.IsIced;
        HasCaramelDrizzle = builder.HasCaramelDrizzle;
        ExtraHot = builder.ExtraHot;
        Decaf = builder.Decaf;
        LoyaltyLevel = builder.LoyaltyLevel;
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
    public Espresso(CoffeeBuilder builder, DateTime orderedAt) : base(builder, orderedAt, 3.80m)
    {
    }
}

public class Cappuccino : Coffee
{
    public Cappuccino(CoffeeBuilder builder, DateTime orderedAt) : base(builder, orderedAt, 4.90m)
    {
    }
}

public class Latte : Coffee
{
    public Latte(CoffeeBuilder builder, DateTime orderedAt) : base(builder, orderedAt, 5.40m)
    {
    }
}

public class CheckoutService
{
    private readonly IPaymentProcessor _paymentProcessor;
    private readonly LocalDatabaseSettings _settings;

    // We add a constructor that accepts our dependency.
    public CheckoutService(IPaymentProcessor paymentProcessor, LocalDatabaseSettings settings)
    {
        _paymentProcessor = paymentProcessor;
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
        // Look! No mention of the Legacy class here.
        // Beautifully simple domain call:
        bool success = _paymentProcessor.ProcessPayment(total);
        if(success)
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


public interface IPaymentProcessor
{
    bool ProcessPayment(decimal amount);
}

public class LegacyPaymentAdapter : IPaymentProcessor
{
    private readonly LegacyPaymentMachine _legacyMachine;
    private readonly LocalDatabaseSettings _settings;

    // We inject the legacy machine and settings into the adapter
    public LegacyPaymentAdapter(LegacyPaymentMachine legacyMachine, LocalDatabaseSettings settings)
    {
        _legacyMachine = legacyMachine;
        _settings = settings;
    }

    public bool ProcessPayment(decimal amount)
    {
        // The Adapter handles the annoying conversion logic here!
        int cents = (int)Math.Round(amount * 100m);

        string result = _legacyMachine.ProcessTransactionInCents(cents, _settings.CurrencyCode);

        return result.StartsWith("APPROVED", StringComparison.OrdinalIgnoreCase);
    }
}
