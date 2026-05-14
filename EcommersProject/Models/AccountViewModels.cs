using System.ComponentModel.DataAnnotations;
using EcommersProject.BLL.DTOs;

namespace EcommersProject.Models;

public class LoginViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = "";

    [Required]
    public string Password { get; set; } = "";

    public string? ReturnUrl { get; set; }
}

public class RegisterViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = "";

    [Required, MinLength(2)]
    public string FirstName { get; set; } = "";

    [Required, MinLength(2)]
    public string LastName { get; set; } = "";

    [Required, Phone]
    public string PhoneNumber { get; set; } = "";

    [Required, MinLength(6)]
    public string Password { get; set; } = "";

    [Required, Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = "";

    public bool IsSeller { get; set; }
    public string? ShopName { get; set; }
    public string? ShopDescription { get; set; }
}

public class ProfileViewModel
{
    public UserGetDto User { get; set; } = null!;
    public IReadOnlyList<OrderGetDto> Orders { get; set; } = [];
    public IReadOnlyList<AddressGetDto> Addresses { get; set; } = [];
}

public class FavoritesViewModel
{
    public IReadOnlyList<WishlistGetDto> Items { get; set; } = [];
}
