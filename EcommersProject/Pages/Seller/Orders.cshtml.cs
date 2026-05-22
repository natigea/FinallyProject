using System.Security.Claims;
using EcommersProject.BLL.DTOs;
using EcommersProject.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EcommersProject.Pages.Seller;

[Authorize]
public class OrdersModel(IMessageService messageService) : PageModel
{
    public IReadOnlyList<ConversationGetDto> Conversations { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdStr, out var userId)) return;
        Conversations = await messageService.GetUserConversationsAsync(userId, cancellationToken);
    }
}
