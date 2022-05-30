public class Message
{
    public string eventName { get; set;}
    public string socketId { get; set; }
    public Dictionary<string,string>? users { get; set;}
    public string? name { get; set; }
    public long? timestamp { get; set; }
    public string? text { get; set; }
}