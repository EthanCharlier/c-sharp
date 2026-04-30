/* MAIN */
using System.Globalization;

class Program
{
    static void Main(string[] args)
    {
        var machine = new VendingMachine(name: "VendBot");

        // Populate
        machine.AddSlot(code: "A1", product: new Food(name: "BBQ Chips", price: 1.50m, foodType: "Snack", slogan: "Crunch your way to happiness!"), quantity: 8);
        machine.AddSlot(code: "A2", product: new Food(name: "Chocolate Bar", price: 1.20m, foodType: "Bar", slogan: "Life is sweeter with every bite."), quantity: 6);
        machine.AddSlot(code: "A3", product: new Food(name: "Tuna Sandwich", price: 3.50m, foodType: "Sandwich", slogan: "Fresh catch, every single time."), quantity: 4);
        machine.AddSlot(code: "A4", product: new Food(name: "Granola Bar", price: 1.80m, foodType: "Bar", slogan: "Fuel your day the natural way."), quantity: 5);
        machine.AddSlot(code: "A5", product: new Food(name: "Salted Pretzels", price: 1.00m, foodType: "Snack", slogan: "Twisted. Salted. Irresistible."), quantity: 7);
        machine.AddSlot(code: "A6", product: new Food(name: "Cheese Crackers", price: 1.30m, foodType: "Snack", slogan: "So cheesy, so good."), quantity: 6);
        machine.AddSlot(code: "A7", product: new Food(name: "Peanut Butter Cup", price: 1.60m, foodType: "Bar", slogan: "Two great tastes in one."), quantity: 5);
        machine.AddSlot(code: "A8", product: new Food(name: "Caesar Wrap", price: 4.00m, foodType: "Sandwich", slogan: "Caesar would be proud."), quantity: 3);
        machine.AddSlot(code: "A9", product: new Food(name: "Trail Mix", price: 2.00m, foodType: "Snack", slogan: "Adventure starts with a handful."), quantity: 4);

        machine.AddSlot(code: "B1", product: new Drink(name: "Coca-Cola", price: 2.00m, volume: 330, isCold: true, drinkType: "Soda", slogan: "Open happiness."), quantity: 5);
        machine.AddSlot(code: "B2", product: new Drink(name: "Orange Juice", price: 2.50m, volume: 250, isCold: false, drinkType: "Juice", slogan: "Squeeze the day!"), quantity: 4);
        machine.AddSlot(code: "B3", product: new Drink(name: "Sparkling Water", price: 1.50m, volume: 500, isCold: true, drinkType: "Water", slogan: "Bubbles that refresh your soul."), quantity: 6);
        machine.AddSlot(code: "B4", product: new Drink(name: "Energy Drink", price: 3.00m, volume: 250, isCold: true, drinkType: "Energy", slogan: "Unleash the beast within."), quantity: 3);
        machine.AddSlot(code: "B5", product: new Drink(name: "Hot Coffee", price: 2.20m, volume: 200, isCold: false, drinkType: "Coffee", slogan: "Your morning, your rules."), quantity: 5);
        machine.AddSlot(code: "B6", product: new Drink(name: "Green Tea", price: 2.00m, volume: 300, isCold: false, drinkType: "Tea", slogan: "Zen in every sip."), quantity: 4);
        machine.AddSlot(code: "B7", product: new Drink(name: "Lemonade", price: 2.30m, volume: 330, isCold: true, drinkType: "Juice", slogan: "Life gave us lemons. You're welcome."), quantity: 5);
        machine.AddSlot(code: "B8", product: new Drink(name: "Chocolate Milk", price: 2.80m, volume: 250, isCold: true, drinkType: "Milk", slogan: "Childhood in a bottle."), quantity: 3);
        machine.AddSlot(code: "B9", product: new Drink(name: "Iced Tea", price: 2.10m, volume: 330, isCold: true, drinkType: "Tea", slogan: "Cool down, one sip at a time."), quantity: 6);

        // Program
        machine.Start();
    }
}

/* PRODUCTS */
public interface IProduct
{
    string Name { get; }
    decimal Price { get; }
    string Slogan { get; }

    void DisplaySlogan();
}

public abstract class Product : IProduct
{
    public string Name { get; protected set; }
    public decimal Price { get; protected set; }
    public abstract string Slogan { get; }

    protected Product(
        string name,
        decimal price
    )
    {
        Name = name;
        Price = price;
    }

    public void DisplaySlogan()
    {
        Console.WriteLine($"[{Name}] → \"{Slogan}\"");
    }

    public override string ToString() => $"{Name} - {Price:C}";
}

public class Food : Product
{
    public string FoodType { get; }
    public override string Slogan { get; }

    public Food(
        string name,
        decimal price,
        string foodType,
        string slogan
    ) : base(
            name = name,
            price = price
        )
    {
        FoodType = foodType;
        Slogan = slogan;
    }
}

public class Drink : Product
{
    public string DrinkType { get; }
    public bool IsCold { get; }
    public int Volume { get; }
    public override string Slogan { get; }

    public Drink(
        string name,
        decimal price,
        string drinkType,
        bool isCold,
        int volume,
        string slogan
    ) : base(
            name = name,
            price = price
        )
    {
        DrinkType = drinkType;
        IsCold = isCold;
        Volume = volume;
        Slogan = slogan;
    }
}

/* VENDING MACHINE */
public class Slot
{
    public string Code { get; }
    public IProduct Product { get; private set; }
    public int Quantity { get; private set; }

    public bool IsAvailable => Quantity > 0;

    public Slot(
        string code,
        IProduct product,
        int quantity
    )
    {
        Code = code;
        Product = product;
        Quantity = quantity;
    }

    public bool Dispense()
    {
        if (!IsAvailable)
        {
            return false;
        } 
        else
        {
            Quantity--;
            return true;
        }
    }

    public void Restock(int quantity) => Quantity += quantity;
}

public class VendingMachine
{
    private CultureInfo Culture;
    public string Name { get; }
    private readonly Dictionary<string, Slot> _slots;
    private decimal _balance;

    public VendingMachine(
        string name,
        CultureInfo? cultureInfo = null
    )
    {
        Culture = cultureInfo ?? CultureInfo.GetCultureInfo("fr-FR");
        Name = name;
        _slots = new Dictionary<string, Slot>();
        _balance = 0;
    }

    public void AddSlot(
        string code,
        IProduct product,
        int quantity
    )
    {
        _slots[code] = new Slot(
            code = code,
            product = product,
            quantity = quantity
        );
    }

    public void InsertMoney(decimal amount)
    {
        _balance += amount;
    }

    public decimal Select(string code)
    {
        if (!_slots.TryGetValue(code, out var slot))
        {
            Console.Clear();
            DisplayHeader();
            Console.WriteLine("---\n\nInvalid code.\n");
            return _balance;
        }

        if (!slot.IsAvailable)
        {
            Console.Clear();
            DisplayHeader();
            Console.WriteLine("---\n\nProduct sold out.\n");
            return _balance;
        }

        var product = slot.Product;

        if (_balance < product.Price)
        {
            Console.Clear();
            DisplayHeader();
            Console.WriteLine($"---\n\nInsufficient balance. Missing: {(product.Price - _balance).ToString("C", Culture)}\n");
            return _balance;
        }

        slot.Dispense();
        _balance -= product.Price;

        Console.Clear();
        DisplayHeader();
        Console.WriteLine($"---\n");

        product.DisplaySlogan();

        Console.WriteLine($"");
        return _balance;
    }

    public void DisplayHeader()
    {
        Console.WriteLine($"[{Name}] - [{_balance.ToString("C", Culture)}]\n");
    }

    public void DisplayCatalogue()
    {
        DisplayHeader();
        foreach (var (code, slot) in _slots)
        {
            string availability = slot.IsAvailable ? $"Qty: {slot.Quantity}" : "SOLD OUT";
            string price = string.Format(Culture, "{0:C}", slot.Product.Price);
            Console.WriteLine($"[{code}] {slot.Product.Name} - {price} ({availability})");
        }
    }

    public void Start()
    {
        bool running = true;

        while (running)
        {
            Console.Clear();
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            DisplayCatalogue();
            Console.WriteLine("\n---\n\n[MENU]");
            Console.WriteLine("[1] Insert money");
            Console.WriteLine("[2] Select a product");
            Console.WriteLine("[3] Cancel & get refund");
            Console.WriteLine("[0] Exit\n\n---\n");
            Console.Write("Select an option (e.g. 1): ");

            string? input = Console.ReadLine();

            switch (input)
            {
                case "1":
                    Console.Clear();
                    DisplayHeader();

                    Console.Write("---\n\nEnter amount (e.g. 2): ");
                    string? amountInput = Console.ReadLine();

                    if (decimal.TryParse(amountInput, NumberStyles.Any, Culture, out decimal amount) && amount > 0)
                    {
                        InsertMoney(amount);
                        Console.Clear();
                        DisplayHeader();
                    }
                    else
                    {
                        Console.Clear();
                        DisplayHeader();
                        Console.WriteLine("---\n\nInvalid amount.\n");
                    }

                    Console.WriteLine("---\n\nPress any key to continue...");
                    Console.ReadKey();
                    break;

                case "2":
                    Console.Clear();
                    DisplayCatalogue();

                    Console.Write("\n---\n\nEnter slot code (e.g. A1): ");
                    string? code = Console.ReadLine()?.ToUpper();

                    if (!string.IsNullOrWhiteSpace(code))
                    {
                        Select(code);
                    }

                    Console.WriteLine("---\n\nPress any key to continue...");
                    Console.ReadKey();
                    break;

                case "3":
                    Console.Clear();
                    DisplayHeader();

                    if (_balance > 0)
                    {
                        decimal balance_save = _balance;
                        _balance = 0;
                        Console.Clear();
                        DisplayHeader();
                        Console.WriteLine($"---\n\nRefund: {balance_save.ToString("C", Culture)}\n");
                    }
                    else
                    {
                        Console.WriteLine("---\n\nNo balance to refund.\n");
                    }
                    Console.WriteLine("---\n\nPress any key to continue...");
                    Console.ReadKey();
                    break;

                case "0":
                    Console.Clear();
                    DisplayHeader();

                    if (_balance > 0)
                    {
                        decimal balance_save = _balance;
                        _balance = 0;
                        Console.Clear();
                        DisplayHeader();
                        Console.WriteLine($"---\n\nRefund before exit: {balance_save.ToString("C", Culture)}\n");
                    }
                    Console.WriteLine("---\n\nGoodbye!");
                    running = false;
                    break;

                default:
                    Console.Clear();
                    DisplayHeader();
                    Console.WriteLine("---\n\nInvalid option.\n");
                    Console.WriteLine("---\n\nPress any key to continue...");
                    Console.ReadKey();
                    break;
            }
        }
    }
}
