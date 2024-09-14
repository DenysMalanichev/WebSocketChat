public class MessageModel 
{
    public string UserName { get; set; }

    public string Message { get; set; } = null!;

    public string TimeSend { get; set; } = null!;

    public string ChatId { get; set; } = null!;
}