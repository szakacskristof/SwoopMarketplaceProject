public class MessageDto
{
    public long Id { get; set; }
    public long FromUserId { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? EditedAt { get; set; }
    // Add this property to indicate if the message was edited
    public bool IsEdited => EditedAt.HasValue;
    // ...
    public bool IsRead { get; set; }
}