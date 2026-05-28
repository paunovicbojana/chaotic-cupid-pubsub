namespace Shared.Models
{
    public class LetterDTO
    {
        public PersonDTO Sender { get; set; } = new();
        public string Message { get; set; } = string.Empty;
    }
}
