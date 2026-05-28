namespace Shared.Models
{
    public class PersonDTO
    {
        public string Username { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public int Years { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public Gender Gender { get; set; }
        public List<Gender> InterestedIn { get; set; } = [];
    }
}
