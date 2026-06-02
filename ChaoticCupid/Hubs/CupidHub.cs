using Microsoft.AspNetCore.SignalR;
using Server.Models;
using Shared.Models;
using Server.Services;

namespace Server.Hubs
{
    public class CupidHub : Hub
    {
        private readonly IMatchmakingService _matchmakingService;

        public CupidHub(IMatchmakingService matchmakingService)
        {
            _matchmakingService = matchmakingService;
        }

        public async Task InitSinglePerson(string username, string city, int years,
                                                string phone, string genderStr, List<string> interestedInStr)
        {
            Gender gender = Enum.Parse<Gender>(genderStr, ignoreCase: true);
            List<Gender> interestedIn = interestedInStr
                .Select(g => Enum.Parse<Gender>(g, ignoreCase: true))
                .ToList();

            var person = new Person
            {
                Username = username,
                City = city,
                Years = years,
                PhoneNumber = phone,
                Gender = gender,
                InterestedIn = interestedIn,
                ConnectionId = Context.ConnectionId
            };

            bool added = _matchmakingService.AddPerson(person);

            if (added)
            {
                Console.WriteLine($"[SERVER] {username} registered from {city}, {years} yrs.");
                await Clients.Caller.SendAsync("InitConfirmed", $"Successfully registered, {username}!");
            }
            else
            {
                Console.WriteLine($"[SERVER] {username} already exists!");
                await Clients.Caller.SendAsync("InitFailed", $"Username '{username}' is already taken.");
            }
        }

        public async Task ConfirmLetter()
        {
            var person = _matchmakingService
                .GetByConnectionId(Context.ConnectionId);

            if (person != null)
            {
                person.IsWaitingConfirmation = false;

                Console.WriteLine(
                    $"[SERVER] {person.Username} confirmed letter reception.");
            }

            await Task.CompletedTask;
        }

        public async Task BlockUser(string usernameToBlock)
        {
            var person = _matchmakingService
                .GetByConnectionId(Context.ConnectionId);

            if (person != null)
            {
                if (person.Username == usernameToBlock)
                {
                    Console.WriteLine(
                        $"[SERVER] User {person.Username} tried to block self.");
                    await Clients.Caller.SendAsync(
                        "UserBlockFailed",
                        $"You cannot block yourself!");
                    return;
                }
                person.BlockedUsers.Add(usernameToBlock);

                Console.WriteLine(
                    $"[SERVER] {person.Username} blocked {usernameToBlock}.");

                await Clients.Caller.SendAsync(
                    "UserBlocked",
                    $"{usernameToBlock} is now blocked.");
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var person = _matchmakingService.GetByConnectionId(Context.ConnectionId);
            if (person != null)
            {
                _matchmakingService.RemovePerson(person.Username);
                Console.WriteLine($"[SERVER] {person.Username} disconnected and removed.");
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}