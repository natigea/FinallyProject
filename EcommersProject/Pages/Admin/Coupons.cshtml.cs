using EcommersProject.BLL.DTOs;
using EcommersProject.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EcommersProject.Pages.Admin;

[Authorize(Policy = "AdminOnly")]
public class CouponsModel(ICouponService couponService) : PageModel
{
    public IReadOnlyList<CouponGetDto> Coupons { get; private set; } = [];

    [BindProperty] public string NewCode { get; set; } = string.Empty;
    [BindProperty] public decimal NewDiscountAmount { get; set; }
    [BindProperty] public decimal NewDiscountPercent { get; set; }
    [BindProperty] public DateTimeOffset NewValidFrom { get; set; } = DateTimeOffset.UtcNow;
    [BindProperty] public DateTimeOffset NewValidTo { get; set; } = DateTimeOffset.UtcNow.AddMonths(1);
    [BindProperty] public int NewUsageLimit { get; set; } = 100;

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Coupons = await couponService.GetAllAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostCreateAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(NewCode))
        {
            TempData["ErrorMessage"] = "Код купона обязателен.";
            return RedirectToPage();
        }

        var code = NewCode.Trim().ToUpperInvariant();
        await couponService.CreateAsync(new CouponCreateDto(
            code, NewDiscountAmount, NewDiscountPercent,
            NewValidFrom, NewValidTo, NewUsageLimit), cancellationToken);

        TempData["SuccessMessage"] = $"Купон {code} создан.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await couponService.DeleteAsync(id, cancellationToken);
        TempData["SuccessMessage"] = "Купон удалён.";
        return RedirectToPage();
    }
}
