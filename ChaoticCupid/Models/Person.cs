using Shared.Models;

namespace Server.Models
{
    public class Person
    {
        public string Username { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public int Years { get; set; }
        public Gender Gender { get; set; }
        public List<Gender> InterestedIn { get; set; } = [];
        public string PhoneNumber { get; set; } = string.Empty;
        public string ConnectionId { get; set; } = string.Empty;
        public bool IsWaitingConfirmation { get; set; } = false;
        public HashSet<string> BlockedUsers { get; set; } = [];

        public PersonDTO ToDto() => new()
        {
            Username = Username,
            City = City,
            Years = Years,
            PhoneNumber = PhoneNumber,
            Gender = Gender,
            InterestedIn = InterestedIn
        };
    }
}
