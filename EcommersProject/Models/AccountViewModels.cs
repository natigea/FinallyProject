using System.ComponentModel.DataAnnotations;
using EcommersProject.BLL.DTOs;

namespace EcommersProject.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Val_EmailRequired")]
    [EmailAddress(ErrorMessage = "Val_EmailInvalid")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Val_PasswordRequired")]
    public string Password { get; set; } = "";

    public string? ReturnUrl { get; set; }
}

public class RegisterViewModel
{
    [Required(ErrorMessage = "Val_EmailRequired")]
    [EmailAddress(ErrorMessage = "Val_EmailInvalid")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Val_FirstNameRequired")]
    [MinLength(2, ErrorMessage = "Val_MinLength2")]
    public string FirstName { get; set; } = "";

    [Required(ErrorMessage = "Val_LastNameRequired")]
    [MinLength(2, ErrorMessage = "Val_MinLength2")]
    public string LastName { get; set; } = "";

    [Required(ErrorMessage = "Val_PhoneRequired")]
    [Phone(ErrorMessage = "Val_PhoneInvalid")]
    public string PhoneNumber { get; set; } = "";

    [Required(ErrorMessage = "Val_PasswordRequired")]
    [MinLength(6, ErrorMessage = "Val_MinLength6")]
    public string Password { get; set; } = "";

    [Required(ErrorMessage = "Val_ConfirmPasswordRequired")]
    [Compare(nameof(Password), ErrorMessage = "Val_PasswordsMismatch")]
    public string ConfirmPassword { get; set; } = "";
}

public class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "Val_EmailRequired")]
    [EmailAddress(ErrorMessage = "Val_EmailInvalid")]
    public string Email { get; set; } = "";
}

public class ResetPasswordViewModel
{
    [Required(ErrorMessage = "Val_EmailRequired")]
    [EmailAddress(ErrorMessage = "Val_EmailInvalid")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Val_CodeRequired")]
    [StringLength(8, MinimumLength = 8, ErrorMessage = "Val_CodeLength8")]
    public string Token { get; set; } = "";

    [Required(ErrorMessage = "Val_NewPasswordRequired")]
    [MinLength(6, ErrorMessage = "Val_MinLength6")]
    public string NewPassword { get; set; } = "";

    [Required(ErrorMessage = "Val_ConfirmPasswordRequired")]
    [Compare(nameof(NewPassword), ErrorMessage = "Val_PasswordsMismatch")]
    public string ConfirmPassword { get; set; } = "";
}

public class ProfileViewModel
{
    public UserGetDto User { get; set; } = null!;
    public IReadOnlyList<ListingGetDto> MyListings { get; set; } = [];
    public IReadOnlyList<FavoriteGetDto> Favorites { get; set; } = [];
    public int UnreadMessages { get; set; }
}

public class ProfilePageViewModel
{
    public UserGetDto User { get; set; } = null!;
    public ProfileEditViewModel EditForm { get; set; } = new();
    public IReadOnlyList<ListingGetDto> MyListings { get; set; } = [];
    public IReadOnlyList<PurchaseGetDto> Purchases { get; set; } = [];
    public IReadOnlyList<PurchaseGetDto> IncomingOrders { get; set; } = [];
    public IReadOnlyList<ReviewGetDto> ReviewsReceived { get; set; } = [];
    public double? AvgRating { get; set; }
    public string ActiveTab { get; set; } = "overview";

    public int ActiveListingsCount   => MyListings.Count(l => l.Status == "Active");
    public int PendingListingsCount  => MyListings.Count(l => l.Status == "Pending");
    public int ClosedListingsCount   => MyListings.Count(l => l.Status == "Closed");
    public decimal TotalEarnings     => MyListings.Where(l => l.Status == "Closed").Sum(l => l.Price);
    public int CompletedBuysCount    => Purchases.Count(p => p.Status == "Completed");
    public int PendingOrdersCount    => IncomingOrders.Count(o => o.SellerApprovalStatus == "Pending");
    public int ApprovedOrdersCount   => IncomingOrders.Count(o => o.SellerApprovalStatus == "Approved");
    public decimal DeliveryRevenue   => IncomingOrders.Where(o => o.SellerApprovalStatus == "Approved").Sum(o => o.TotalAmount);
}

public class ProfileEditViewModel
{
    [Required(ErrorMessage = "Val_FirstNameRequired")]
    [MinLength(2, ErrorMessage = "Val_MinLength2")]
    public string FirstName { get; set; } = "";

    [Required(ErrorMessage = "Val_LastNameRequired")]
    [MinLength(2, ErrorMessage = "Val_MinLength2")]
    public string LastName { get; set; } = "";

    [Phone(ErrorMessage = "Val_PhoneInvalid")]
    public string PhoneNumber { get; set; } = "";

    public string? PhotoUrl { get; set; }
    public IFormFile? Photo { get; set; }
}

public class ChangePasswordViewModel
{
    [Required(ErrorMessage = "Val_CurrentPasswordRequired")]
    public string CurrentPassword { get; set; } = "";

    [Required(ErrorMessage = "Val_NewPasswordRequired")]
    [MinLength(6, ErrorMessage = "Val_MinLength6")]
    public string NewPassword { get; set; } = "";

    [Required(ErrorMessage = "Val_ConfirmPasswordRequired")]
    [Compare(nameof(NewPassword), ErrorMessage = "Val_PasswordsMismatch")]
    public string ConfirmPassword { get; set; } = "";
}

public class TwoFactorViewModel
{
    [Required(ErrorMessage = "Val_CodeRequired")]
    public string Code { get; set; } = "";
    public string? Email { get; set; }
}
