namespace DomainModels;

public class Message : Common
{
    public string UserSenderId { get; set; }
    public string UserDestinationId { get; set; }
    public string Msg { get; set; }
    public bool IsAdmin { get; set; }
    public virtual User? User { get; set; }
}

public class PostMessageRequestDto
{
    public string Msg { get; set; } = default!;
    public string DestinationId { get; set; } = default!;
    public bool IsAdmin { get; set; }
}