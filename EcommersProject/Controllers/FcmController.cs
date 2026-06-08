using EcommersProject.DAL.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EcommersProject.Controllers;

[Authorize]
[ApiExplorerSettings(IgnoreApi = true)]
public class FcmController(AppDbContext db) : Controller
{
    // POST /Fcm/Token  { "token": "..." }
    // Called by the client-side Firebase SDK after obtaining an FCM registration token.
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Token([FromBody] FcmTokenDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto?.Token)) return BadRequest();

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
        if (user == null) return NotFound();

        user.FcmToken = dto.Token;
        await db.SaveChangesAsync();
        return Ok();
    }

    public record FcmTokenDto(string Token);
}
