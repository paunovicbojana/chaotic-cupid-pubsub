using Microsoft.AspNetCore.SignalR.Client;

var persons = new[]
{
    new { Username = "Ana",   City = "Novi Sad", Years = 25, Phone = "0601111111", Gender = "Female",     InterestedIn = new[] { "Male" } },
    new { Username = "Boban",     City = "Novi Sad", Years = 26, Phone = "0602222222", Gender = "Male",       InterestedIn = new[] { "Female" } },
    new { Username = "Radovan", City = "Beograd",  Years = 30, Phone = "0603333333", Gender = "Male",       InterestedIn = new[] { "Female", "NonBinary" } },
    new { Username = "Dragana",   City = "Beograd",  Years = 28, Phone = "0604444444", Gender = "Female",     InterestedIn = new[] { "Male", "Female" } },
    new { Username = "Aleks",    City = "Novi Sad", Years = 24, Phone = "0605555555", Gender = "NonBinary",  InterestedIn = new[] { "Female", "NonBinary" } },
};

var connections = new List<HubConnection>();

foreach (var person in persons)
{
    var connection = new HubConnectionBuilder()
        .WithUrl("https://localhost:7250/cupidHub")
        .Build();

    var name = person.Username;

    connection.On<string>("InitConfirmed", (msg) =>
        Console.WriteLine($"[{name}] {msg}"));

    connection.On<string>("InitFailed", (msg) =>
        Console.WriteLine($"[{name}] {msg}"));

    connection.On<string>("UserBlocked", (msg) =>
        Console.WriteLine($"[{name}] {msg}"));

    connection.On<string>("UserBlockFailed", (msg) =>
        Console.WriteLine($"[{name}] {msg}"));

    connection.On<object>("ReceiveLetter", async (letter) =>
    {
        Console.WriteLine($"[{name}] Letter received: {letter}");
        await Task.Delay(500);
        await connection.InvokeAsync("ConfirmLetter");
        Console.WriteLine($"[{name}] Letter confirmed.");
    });

    await connection.StartAsync();
    Console.WriteLine($"[{name}] Connected.");

    await connection.InvokeAsync("InitSinglePerson",
        person.Username,
        person.City,
        person.Years,
        person.Phone,
        person.Gender,
        person.InterestedIn.ToList());

    connections.Add(connection);

    await Task.Delay(300);
}

Console.WriteLine("\nAll persons registered. Waiting for Cupid to send letters...");
Console.WriteLine("Press Enter to run block tests, or wait for letters.\n");
Console.ReadLine();

Console.WriteLine("[TEST] Ana blocks Boban...");
await connections[0].InvokeAsync("BlockUser", "Boban");

Console.WriteLine("\nPress Enter to run next blocking test or wait for this one to finish...");
Console.ReadLine();

Console.WriteLine("[TEST] Ana blocks self...");
await connections[0].InvokeAsync("BlockUser", "Ana");

Console.WriteLine("\nPress Enter to disconnect all and exit.");
Console.ReadLine();

foreach (var c in connections)
    await c.StopAsync();

Console.WriteLine("All disconnected.");