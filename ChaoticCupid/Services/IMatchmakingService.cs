using Server.Models;

namespace Server.Services
{
    public interface IMatchmakingService
    {
        bool AddPerson(Person person);
        Person? FindBestMatch(Person sender);
        Person? GetByUsername(string username);
        Person? GetByConnectionId(string connectionId);
        List<Person> GetAll();
        void RemovePerson(string username);
    }
}
