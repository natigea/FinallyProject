namespace EcommersProject.DAL.Entities;

public class Message : BaseEntity
{
    public string Text { get; set; } = string.Empty;
    public bool IsRead { get; set; }

    public Guid ConversationId { get; set; }
    public Conversation? Conversation { get; set; }

    public Guid SenderId { get; set; }
    public User? Sender { get; set; }
}
