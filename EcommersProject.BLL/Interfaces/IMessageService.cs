using EcommersProject.BLL.DTOs;

namespace EcommersProject.BLL.Interfaces;

public interface IMessageService
{
    Task<ConversationGetDto> GetOrCreateConversationAsync(ConversationStartDto dto, CancellationToken ct = default);
    Task<ConversationGetDto> GetConversationAsync(Guid conversationId, CancellationToken ct = default);
    Task<IReadOnlyList<ConversationGetDto>> GetUserConversationsAsync(Guid userId, CancellationToken ct = default);
    Task<MessageGetDto> SendMessageAsync(MessageCreateDto dto, CancellationToken ct = default);
    Task MarkAsReadAsync(Guid conversationId, Guid userId, CancellationToken ct = default);
    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken ct = default);
}
