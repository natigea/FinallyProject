namespace EcommersProject.BLL.DTOs;

public record MessageGetDto(
    Guid Id,
    string Text,
    bool IsRead,
    Guid SenderId,
    string? SenderName,
    Guid ConversationId,
    DateTimeOffset CreatedDate);

public record ConversationGetDto(
    Guid Id,
    Guid ListingId,
    string? ListingTitle,
    decimal? ListingPrice,
    string? ListingImage,
    Guid BuyerId,
    string? BuyerName,
    Guid SellerId,
    string? SellerName,
    int UnreadCount,
    MessageGetDto? LastMessage,
    DateTimeOffset CreatedDate,
    IReadOnlyList<MessageGetDto>? Messages = null);

public record MessageCreateDto(
    Guid ConversationId,
    Guid SenderId,
    string Text);

public record ConversationStartDto(
    Guid ListingId,
    Guid BuyerId,
    string InitialMessage);
