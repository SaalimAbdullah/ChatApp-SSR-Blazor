namespace BlazorApp5.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string SenderCode { get; set; }
        public string ReceiverCode { get; set; }  // or GroupId if you move to group chat
        public string MessageText { get; set; }
        public DateTime SentAt { get; set; }
    }

}
