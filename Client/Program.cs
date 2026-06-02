using Microsoft.AspNetCore.SignalR.Client;
using Shared.Models;

var connection = new HubConnectionBuilder()
    .WithUrl("https://localhost:7250/cupidHub")
    .Build();

connection.On<string>("InitConfirmed", (msg) =>
{
    Console.WriteLine($"[SERVER] {msg}");
});

connection.On<string>("InitFailed", (msg) =>
{
    Console.WriteLine($"[SERVER] {msg}");
    Environment.Exit(0);
});

connection.On<string>("UserBlocked", (msg) =>
{
    Console.WriteLine($"[SERVER] {msg}");
});

connection.On<string>("UserBlockFailed", (msg) =>
{
    Console.WriteLine($"[SERVER] {msg}");
});

var letterConfirmation = new ManualResetEventSlim(false);

connection.On<LetterDTO>("ReceiveLetter", async (letter) =>
{
    letterConfirmation.Reset();

    Console.WriteLine("\nYou've got a love letter!");
    Console.WriteLine($"   From:    {letter.Sender.Username}");
    Console.WriteLine($"   City:    {letter.Sender.City}");
    Console.WriteLine($"   Age:     {letter.Sender.Years}");
    Console.WriteLine($"   Gender:  {letter.Sender.Gender}");

    if (!letter.Sender.PhoneNumber.Equals(""))
        Console.WriteLine($"   Phone:   {letter.Sender.PhoneNumber}");

    Console.WriteLine($"   Message: {letter.Message}");
    Console.WriteLine("\nPress Enter to confirm receipt...");

    letterConfirmation.Wait();

    await connection.InvokeAsync("ConfirmLetter");
    Console.WriteLine("Letter confirmed.\n");
});

string username = EnterString("Enter username: ", allowDigits: false);
string city = EnterString("Enter city: ", allowDigits: false);
int years = EnterPositiveInt("Enter age: ");
string phone = EnterString("Enter phone number: ", allowDigits: true);
Gender gender = EnterGender("Enter your gender");
List<Gender> interestedIn = EnterInterestedIn("Interested in");

await connection.StartAsync();
await connection.InvokeAsync("InitSinglePerson", username, city, years, phone,
    gender.ToString(),
    interestedIn.Select(g => g.ToString()).ToList());

Console.WriteLine("\nType /block <username> to block someone, or wait for letters...\n");

while (true)
{
    string? input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input))
    {
        if (!letterConfirmation.IsSet)
            letterConfirmation.Set();
        continue;
    }

    if (input.StartsWith("/block "))
    {
        string usernameToBlock = input[7..].Trim();
        if (string.IsNullOrWhiteSpace(usernameToBlock))
        {
            Console.WriteLine("Please enter a username to block.");
            continue;
        }
        await connection.InvokeAsync("BlockUser", usernameToBlock);
    }
    else
    {
        Console.WriteLine("Unknown command. Use /block <username>.");
    }
}

static string EnterString(string prompt, bool allowDigits)
{
    while (true)
    {
        Console.Write(prompt);
        string? input = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(input))
        {
            Console.WriteLine("Field cannot be empty.");
            continue;
        }

        if (!allowDigits && input.Any(char.IsDigit))
        {
            Console.WriteLine("This field cannot contain digits.");
            continue;
        }

        return input;
    }
}

static int EnterPositiveInt(string prompt)
{
    while (true)
    {
        Console.Write(prompt);
        string? input = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(input))
        {
            Console.WriteLine("Field cannot be empty.");
            continue;
        }

        if (!int.TryParse(input, out int value))
        {
            Console.WriteLine("Must be a number.");
            continue;
        }

        if (value <= 0)
        {
            Console.WriteLine("Must be a positive number.");
            continue;
        }

        return value;
    }
}

static Gender EnterGender(string prompt)
{
    while (true)
    {
        string options = string.Join(", ", Enum.GetNames<Gender>());
        Console.Write($"{prompt} ({options}): ");
        string? input = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(input))
        {
            Console.WriteLine("Field cannot be empty.");
            continue;
        }

        if (Enum.TryParse<Gender>(input, ignoreCase: true, out Gender gender))
            return gender;

        Console.WriteLine($"Invalid option. Choose from: {options}");
    }
}

static List<Gender> EnterInterestedIn(string prompt)
{
    while (true)
    {
        string options = string.Join(", ", Enum.GetNames<Gender>());
        Console.Write($"{prompt} ({options}) - comma separated: ");
        string? input = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(input))
        {
            Console.WriteLine("Field cannot be empty.");
            continue;
        }

        var result = new List<Gender>();
        bool valid = true;

        foreach (var part in input.Split(','))
        {
            if (Enum.TryParse<Gender>(part.Trim(), ignoreCase: true, out Gender g))
                result.Add(g);
            else
            {
                Console.WriteLine($"Invalid option '{part.Trim()}'. Choose from: {options}");
                valid = false;
                break;
            }
        }

        if (valid && result.Count > 0)
            return result;
    }
}