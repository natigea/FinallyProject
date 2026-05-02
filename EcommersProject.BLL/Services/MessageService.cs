using EcommersProject.BLL.DTOs;
using EcommersProject.BLL.Exceptions;
using EcommersProject.BLL.Interfaces;
using EcommersProject.DAL.Entities;
using EcommersProject.DAL.UnitOfWork;

namespace EcommersProject.BLL.Services;

public class MessageService(IUnitOfWork uow) : IMessageService
{
    public async Task<ConversationGetDto> GetOrCreateConversationAsync(
        ConversationStartDto dto, CancellationToken ct = default)
    {
        // Check if conversation already exists
        var existing = await uow.Conversations.FindAsync(
            c => c.ListingId == dto.ListingId && c.BuyerId == dto.BuyerId,
            ct,
            c => c.Listing!, c => c.Listing!.Images, c => c.Buyer!, c => c.Seller!, c => c.Messages);

        if (existing.FirstOrDefault() is { } conv)
        {
            if (!string.IsNullOrWhiteSpace(dto.InitialMessage))
            {
                var msg = new Message
                {
                    ConversationId = conv.Id,
                    SenderId = dto.BuyerId,
                    Text = dto.InitialMessage
                };
                await uow.Messages.AddAsync(msg, ct);
                await uow.SaveChangesAsync(ct);
            }
            return await GetConversationAsync(conv.Id, ct);
        }

        // Get listing to find seller
        var listings = await uow.Listings.FindAsync(l => l.Id == dto.ListingId, ct);
        var listing = listings.FirstOrDefault()
            ?? throw new NotFoundException(nameof(Listing), dto.ListingId);

        var newConv = new Conversation
        {
            ListingId = dto.ListingId,
            BuyerId = dto.BuyerId,
            SellerId = listing.UserId
        };
        await uow.Conversations.AddAsync(newConv, ct);
        await uow.SaveChangesAsync(ct);

        if (!string.IsNullOrWhiteSpace(dto.InitialMessage))
        {
            var msg = new Message
            {
                ConversationId = newConv.Id,
                SenderId = dto.BuyerId,
                Text = dto.InitialMessage
            };
            await uow.Messages.AddAsync(msg, ct);
            await uow.SaveChangesAsync(ct);
        }

        return await GetConversationAsync(newConv.Id, ct);
    }

    public async Task<ConversationGetDto> GetConversationAsync(
        Guid conversationId, CancellationToken ct = default)
    {
        var results = await uow.Conversations.FindAsync(
            c => c.Id == conversationId, ct,
            c => c.Listing!, c => c.Listing!.Images, c => c.Buyer!, c => c.Seller!, c => c.Messages);
        var conv = results.FirstOrDefault()
            ?? throw new NotFoundException(nameof(Conversation), conversationId);

        var msgs = await uow.Messages.FindAsync(
            m => m.ConversationId == conv.Id, ct, m => m.Sender!);

        return MapConversation(conv, msgs.OrderBy(m => m.CreatedDate).ToList());
    }

    public async Task<IReadOnlyList<ConversationGetDto>> GetUserConversationsAsync(
        Guid userId, CancellationToken ct = default)
    {
        var convs = await uow.Conversations.FindAsync(
            c => c.BuyerId == userId || c.SellerId == userId, ct,
            c => c.Listing!, c => c.Listing!.Images, c => c.Buyer!, c => c.Seller!, c => c.Messages);

        var result = new List<ConversationGetDto>();
        foreach (var conv in convs.OrderByDescending(c => c.UpdatedDate))
        {
            var msgs = await uow.Messages.FindAsync(
                m => m.ConversationId == conv.Id, ct, m => m.Sender!);
            result.Add(MapConversation(conv, msgs.OrderBy(m => m.CreatedDate).ToList(), userId));
        }
        return result;
    }

    public async Task<MessageGetDto> SendMessageAsync(
        MessageCreateDto dto, CancellationToken ct = default)
    {
        var msg = new Message
        {
            ConversationId = dto.ConversationId,
            SenderId = dto.SenderId,
            Text = dto.Text
        };
        await uow.Messages.AddAsync(msg, ct);

        // Update conversation timestamp
        var convs = await uow.Conversations.FindAsync(c => c.Id == dto.ConversationId, ct);
        if (convs.FirstOrDefault() is { } conv)
            await uow.Conversations.UpdateAsync(conv, ct);

        await uow.SaveChangesAsync(ct);

        var result = await uow.Messages.FindAsync(m => m.Id == msg.Id, ct, m => m.Sender!);
        return MapMessage(result.First());
    }

    public async Task MarkAsReadAsync(
        Guid conversationId, Guid userId, CancellationToken ct = default)
    {
        var msgs = await uow.Messages.FindAsync(
            m => m.ConversationId == conversationId && m.SenderId != userId && !m.IsRead, ct);
        foreach (var msg in msgs)
        {
            msg.IsRead = true;
            await uow.Messages.UpdateAsync(msg, ct);
        }
        if (msgs.Any()) await uow.SaveChangesAsync(ct);
    }

    public async Task<int> GetUnreadCountAsync(Guid userId, CancellationToken ct = default)
    {
        var convs = await uow.Conversations.FindAsync(
            c => c.BuyerId == userId || c.SellerId == userId, ct);
        var count = 0;
        foreach (var conv in convs)
        {
            var msgs = await uow.Messages.FindAsync(
                m => m.ConversationId == conv.Id && m.SenderId != userId && !m.IsRead, ct);
            count += msgs.Count;
        }
        return count;
    }

    private static ConversationGetDto MapConversation(
        Conversation c, IList<Message> msgs, Guid? currentUserId = null)
    {
        var unread = currentUserId.HasValue
            ? msgs.Count(m => m.SenderId != currentUserId && !m.IsRead)
            : 0;
        var last = msgs.LastOrDefault();
        var msgDtos = msgs.Select(MapMessage).ToList();
        return new ConversationGetDto(
            c.Id, c.ListingId, c.Listing?.Title, c.Listing?.Price,
            c.Listing?.Images.OrderBy(i => i.SortOrder).FirstOrDefault()?.Url,
            c.BuyerId, c.Buyer != null ? c.Buyer.FirstName + " " + c.Buyer.LastName : null,
            c.SellerId, c.Seller != null ? c.Seller.FirstName + " " + c.Seller.LastName : null,
            unread,
            last is null ? null : MapMessage(last),
            c.CreatedDate,
            msgDtos);
    }

    private static MessageGetDto MapMessage(Message m) => new(
        m.Id, m.Text, m.IsRead, m.SenderId,
        m.Sender != null ? m.Sender.FirstName + " " + m.Sender.LastName : null,
        m.ConversationId, m.CreatedDate);
}
