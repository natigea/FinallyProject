using EcommersProject.BLL.DTOs;
using EcommersProject.BLL.Interfaces;
using EcommersProject.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Security.Claims;

namespace EcommersProject.Controllers;

[Authorize]
[ApiExplorerSettings(IgnoreApi = true)]
public class ReviewController(
    IReviewService reviews,
    IListingService listings,
    IStringLocalizer<SharedResource> localizer) : Controller
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Guid listingId, int rating, string comment)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        ListingGetDto listing;
        try { listing = await listings.GetByIdAsync(listingId); }
        catch { return NotFound(); }

        if (listing.UserId == userId)
        {
            TempData["Error"] = localizer["Review_CannotReviewOwn"].Value;
            return RedirectToAction("Detail", "Listing", new { id = listingId });
        }

        if (await reviews.HasReviewedAsync(userId, listingId))
        {
            TempData["Error"] = localizer["Review_Already"].Value;
            return RedirectToAction("Detail", "Listing", new { id = listingId });
        }

        if (rating < 1 || rating > 5)
        {
            TempData["Error"] = "Неверная оценка.";
            return RedirectToAction("Detail", "Listing", new { id = listingId });
        }

        await reviews.CreateAsync(new ReviewCreateDto(listingId, userId, listing.UserId, rating, comment ?? ""));
        TempData["Success"] = localizer["Review_Submitted"].Value;
        return RedirectToAction("Detail", "Listing", new { id = listingId });
    }
}
