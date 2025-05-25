namespace BlazorApp5.Models
{
	public class UserMessage
	{
        public string Username { get; set; }        // Sender
        public string Message { get; set; }
        public bool CurrentUser { get; set; }
        public DateTime DateSent { get; set; }
        public string ReceiverCode { get; set; }    // New
        public string SenderCode { get; set; }      // New
    }
}
