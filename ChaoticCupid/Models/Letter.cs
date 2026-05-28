namespace Server.Models
{
    public class Letter
    {
        public Person Sender { get; set; } = new();
        public string Message { get; set; } = string.Empty;
    }
}
