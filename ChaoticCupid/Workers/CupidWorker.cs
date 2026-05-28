using Microsoft.AspNetCore.SignalR;
using Server.Hubs;
using Shared.Models;
using Server.Services;
using System.Security.Cryptography;

namespace Server.Workers
{
    public class CupidWorker : BackgroundService
    {
        private readonly IMatchmakingService _matchmakingService;
        private readonly IHubContext<CupidHub> _hubContext;

        private static readonly string[] Messages =
        [
            "Looking forward to meeting you!",
            "I would like to get to know you.",
            "I'm not interested in meeting."
        ];

        public CupidWorker(IMatchmakingService matchmakingService, IHubContext<CupidHub> hubContext)
        {
            _matchmakingService = matchmakingService;
            _hubContext = hubContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("[CUPID] Worker started, sending letters every 60 seconds.");

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

                var allParticipants = _matchmakingService.GetAll();

                if (allParticipants.Count < 2)
                {
                    Console.WriteLine("[CUPID] Not enough registered players for Cupid to send love letters to.");
                    continue;
                }

                foreach (var participant in allParticipants)
                {
                    if (participant.IsWaitingConfirmation) continue;

                    var sender = _matchmakingService.FindBestMatch(participant);
                    if (sender == null) continue;

                    using var rng = RandomNumberGenerator.Create();
                    byte[] buffer = new byte[4];
                    rng.GetBytes(buffer);
                    int index = (int)(BitConverter.ToUInt32(buffer, 0) % Messages.Length);
                    string message = Messages[index];

                    var letter = new LetterDTO
                    {
                        Sender = sender.ToDto(),
                        Message = message
                    };

                    if (message.Equals("I'm not interested in meeting."))   letter.Sender.PhoneNumber = "";

                    participant.IsWaitingConfirmation = true;

                    Console.WriteLine($"[CUPID] Sending letter from {sender.Username} to {participant.Username}");
                    await _hubContext.Clients.Client(participant.ConnectionId)
                        .SendAsync("ReceiveLetter", letter, stoppingToken);
                }
            }
        }
    }
}