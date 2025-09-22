namespace Sample.Models;

public record ApiPayload
{
    public string Title { get; set; }
    public string Body { get; set; }
    public int UserId { get; set; }
}
