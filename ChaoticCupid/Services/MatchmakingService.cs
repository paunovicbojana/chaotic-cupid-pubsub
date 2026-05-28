using Server.Models;
using System.Security.Cryptography;

namespace Server.Services
{
    public class MatchmakingService : IMatchmakingService
    {
        private readonly List<Person> _persons = [];
        private readonly object _lock = new();

        public bool AddPerson(Person person)
        {
            lock (_lock)
            {
                if (_persons.Any(p => p.Username == person.Username))
                    return false;

                _persons.Add(person);
                return true;
            }
        }

        public Person? FindBestMatch(Person sender)
        {
            lock (_lock)
            {
                using var rng = RandomNumberGenerator.Create();

                Person? bestMatch = null;
                int bestScore = -1;

                foreach (var candidate in _persons)
                {
                    // skip self
                    if (candidate.Username == sender.Username) continue;

                    if (candidate.BlockedUsers.Contains(sender.Username)) continue;

                    if (sender.BlockedUsers.Contains(candidate.Username)) continue;

                    if (!sender.InterestedIn.Contains(candidate.Gender)) continue;

                    if (!candidate.InterestedIn.Contains(sender.Gender)) continue;

                    int score = 0;

                    if (candidate.City == sender.City)
                        score += 30;

                    if (Math.Abs(candidate.Years - sender.Years) <= 2)
                        score += 20;

                    byte[] buffer = new byte[4];
                    rng.GetBytes(buffer);
                    int random = (int)(BitConverter.ToUInt32(buffer, 0) % 101);

                    score += random;

                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestMatch = candidate;
                    }
                }

                return bestMatch;
            }
        }

        public Person? GetByUsername(string username)
        {
            lock (_lock)
                return _persons.FirstOrDefault(p => p.Username == username);
        }

        public Person? GetByConnectionId(string connectionId)
        {
            lock (_lock)
                return _persons.FirstOrDefault(p => p.ConnectionId == connectionId);
        }

        public List<Person> GetAll()
        {
            lock (_lock)
                return [.. _persons];
        }

        public void RemovePerson(string username)
        {
            lock (_lock)
                _persons.RemoveAll(p => p.Username == username);
        }
    }
}
