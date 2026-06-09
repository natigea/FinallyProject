using EcommersProject.BLL.DTOs;
using EcommersProject.BLL.Interfaces;
using EcommersProject.Models;
using EcommersProject.Resources;
using EcommersProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Security.Claims;

namespace EcommersProject.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class ListingController(
    IListingService listings,
    ICategoryService categories,
    IFavoriteService favorites,
    IReviewService reviewSvc,
    IWebHostEnvironment env,
    CloudinaryService cloudinary,
    IStringLocalizer<SharedResource> localizer) : Controller
{
    private IStringLocalizer<SharedResource> L => localizer;

    // ────── Browse / Search ──────
    [HttpGet]
    public async Task<IActionResult> Index(
        string? q, Guid? categoryId, string? city,
        decimal? priceMin, decimal? priceMax, string? sortBy,
        bool onlyVip = false, bool onlyWithPhoto = false, int page = 1)
    {
        ViewData["ShowFilterBtn"] = true;
        var searchDto = new ListingSearchDto(q, categoryId, city, priceMin, priceMax, sortBy ?? "newest", page, 20, onlyVip, onlyWithPhoto);
        var (items, total) = await listings.SearchAsync(searchDto);
        var cats = await categories.GetAllAsync();

        return View(new BrowseViewModel
        {
            Listings = items,
            Categories = cats,
            Total = total,
            Page = page,
            Query = q,
            CategoryId = categoryId,
            City = city,
            PriceMin = priceMin,
            PriceMax = priceMax,
            SortBy = sortBy,
            OnlyVip = onlyVip,
            OnlyWithPhoto = onlyWithPhoto
        });
    }

    // ────── Detail ──────
    [HttpGet]
    public async Task<IActionResult> Detail(Guid id)
    {
        try
        {
            var listing = await listings.GetByIdAsync(id);
            bool isFav = false;
            bool hasReviewed = false;
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                isFav = await favorites.IsFavoriteAsync(userId, id);
                hasReviewed = await reviewSvc.HasReviewedAsync(userId, id);
            }
            ViewBag.IsFavorite = isFav;
            ViewBag.HasReviewed = hasReviewed;
            ViewBag.Reviews = await reviewSvc.GetForListingAsync(id);
            ViewBag.AvgRating = await reviewSvc.GetAverageRatingAsync(listing.UserId);
            return View(listing);
        }
        catch { return NotFound(); }
    }

    // ────── Create ──────
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        return View(new ListingFormViewModel
        {
            Categories = await categories.GetAllAsync()
        });
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ListingFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            vm.Categories = await categories.GetAllAsync();
            return View(vm);
        }

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var created = await listings.CreateAsync(new ListingCreateDto(
            vm.Title, vm.Description, vm.Price, vm.City,
            vm.ContactPhone, vm.CategoryId, userId,
            vm.DeliveryAvailable, vm.Condition));

        await SaveImages(created.Id, vm.NewImages);

        TempData["Info"] = L["Lst_SentForReview"].Value;
        return RedirectToAction(nameof(MyListings));
    }

    // ────── Edit ──────
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var listing = await listings.GetByIdAsync(id);
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        if (listing.UserId != userId && !User.IsInRole("Admin"))
            return Forbid();

        if (listing.Status is "Pending" or "Closed" && !User.IsInRole("Admin"))
        {
            TempData["Info"] = L["Listing_SentForReview"].Value;
            return RedirectToAction(nameof(MyListings));
        }

        return View(new ListingFormViewModel
        {
            Id                = listing.Id,
            Title             = listing.Title,
            Description       = listing.Description,
            Price             = listing.Price,
            City              = listing.City,
            ContactPhone      = listing.ContactPhone,
            CategoryId        = listing.CategoryId,
            Condition         = listing.Condition,
            DeliveryAvailable = listing.DeliveryAvailable,
            Categories        = await categories.GetAllAsync(),
            ExistingImages    = listing.Images
        });
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, ListingFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            vm.Categories = await categories.GetAllAsync();
            var current = await listings.GetByIdAsync(id);
            vm.ExistingImages = current.Images;
            return View(vm);
        }

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var listing = await listings.GetByIdAsync(id);
        if (listing.UserId != userId && !User.IsInRole("Admin"))
            return Forbid();
        if (listing.Status is "Pending" or "Closed" && !User.IsInRole("Admin"))
            return RedirectToAction(nameof(MyListings));

        bool isAdmin = User.IsInRole("Admin");
        await listings.UpdateAsync(id,
            new ListingUpdateDto(vm.Title, vm.Description, vm.Price, vm.City, vm.ContactPhone, vm.CategoryId, vm.DeliveryAvailable, vm.Condition),
            isAdmin);

        // Delete marked images
        foreach (var imgId in vm.DeleteImageIds)
            await listings.DeleteImageAsync(imgId);

        // Upload new images
        await SaveImages(id, vm.NewImages);

        if (isAdmin)
            TempData["Success"] = L["Lst_Updated"].Value;
        else
            TempData["Info"] = L["Listing_SentForReview"].Value;

        return RedirectToAction(nameof(Detail), new { id });
    }

    // ────── Close / Delete ──────
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Close(Guid id)
    {
        var listing = await listings.GetByIdAsync(id);
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        if (listing.UserId != userId && !User.IsInRole("Admin"))
            return Forbid();

        await listings.CloseAsync(id);
        TempData["Success"] = L["Lst_Closed"].Value;
        return RedirectToAction(nameof(MyListings));
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var listing = await listings.GetByIdAsync(id);
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        if (listing.UserId != userId && !User.IsInRole("Admin"))
            return Forbid();

        await listings.DeleteAsync(id);
        TempData["Success"] = L["Lst_Deleted"].Value;
        return RedirectToAction(nameof(MyListings));
    }

    // ────── Admin quick-actions from Detail page ──────
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AdminApprove(Guid id)
    {
        await listings.ApproveAsync(id);
        TempData["Success"] = L["Admin_ApprovedMsg"].Value;
        return RedirectToAction(nameof(Detail), new { id });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AdminReject(Guid id)
    {
        await listings.RejectAsync(id);
        TempData["Info"] = L["Admin_RejectedMsg"].Value;
        return RedirectToAction(nameof(Detail), new { id });
    }

    // ────── My Listings ──────
    [Authorize]
    public async Task<IActionResult> MyListings()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var myListings = await listings.GetByUserAsync(userId);
        return View(myListings);
    }

    // ────── Favorite toggle (AJAX) ──────
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleFavorite(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var isFav = await favorites.IsFavoriteAsync(userId, id);
        if (isFav)
            await favorites.RemoveAsync(userId, id);
        else
            await favorites.AddAsync(userId, id);
        return Json(new { isFavorite = !isFav });
    }

    // ────── Helpers ──────
    private async Task SaveImages(Guid listingId, List<IFormFile> files)
    {
        if (files == null || !files.Any()) return;
        foreach (var file in files.Take(10))
        {
            if (file.Length == 0) continue;
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (ext is not (".jpg" or ".jpeg" or ".png" or ".webp")) continue;

            var url = await cloudinary.UploadAsync(file, "listings");
            if (url != null)
                await listings.AddImageAsync(listingId, url);
        }
    }
}
