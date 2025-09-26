namespace DomainModels;

public class Message : Common
{
    public string UserId { get; set; }
    public string Content { get; set; } = null!;
    public virtual User? User { get; set; }

}