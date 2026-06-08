using System.ComponentModel.DataAnnotations;
using EcommersProject.BLL.DTOs;

namespace EcommersProject.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Введите email")]
    [EmailAddress(ErrorMessage = "Некорректный email")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Введите пароль")]
    public string Password { get; set; } = "";

    public string? ReturnUrl { get; set; }
}

public class RegisterViewModel
{
    [Required(ErrorMessage = "Введите email")]
    [EmailAddress(ErrorMessage = "Некорректный email")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Введите имя")]
    [MinLength(2, ErrorMessage = "Минимум 2 символа")]
    public string FirstName { get; set; } = "";

    [Required(ErrorMessage = "Введите фамилию")]
    [MinLength(2, ErrorMessage = "Минимум 2 символа")]
    public string LastName { get; set; } = "";

    [Required(ErrorMessage = "Введите номер телефона")]
    [Phone(ErrorMessage = "Некорректный номер телефона")]
    public string PhoneNumber { get; set; } = "";

    [Required(ErrorMessage = "Введите пароль")]
    [MinLength(6, ErrorMessage = "Минимум 6 символов")]
    public string Password { get; set; } = "";

    [Required(ErrorMessage = "Подтвердите пароль")]
    [Compare(nameof(Password), ErrorMessage = "Пароли не совпадают")]
    public string ConfirmPassword { get; set; } = "";
}

public class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "Введите email")]
    [EmailAddress(ErrorMessage = "Некорректный email")]
    public string Email { get; set; } = "";
}

public class ResetPasswordViewModel
{
    [Required(ErrorMessage = "Введите email")]
    [EmailAddress(ErrorMessage = "Некорректный email")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Введите код из сообщения")]
    [StringLength(8, MinimumLength = 8, ErrorMessage = "Код состоит из 8 символов")]
    public string Token { get; set; } = "";

    [Required(ErrorMessage = "Введите новый пароль")]
    [MinLength(6, ErrorMessage = "Минимум 6 символов")]
    public string NewPassword { get; set; } = "";

    [Required(ErrorMessage = "Подтвердите пароль")]
    [Compare(nameof(NewPassword), ErrorMessage = "Пароли не совпадают")]
    public string ConfirmPassword { get; set; } = "";
}

public class ProfileViewModel
{
    public UserGetDto User { get; set; } = null!;
    public IReadOnlyList<ListingGetDto> MyListings { get; set; } = [];
    public IReadOnlyList<FavoriteGetDto> Favorites { get; set; } = [];
    public int UnreadMessages { get; set; }
}

public class ProfileEditViewModel
{
    [Required(ErrorMessage = "Введите имя")]
    [MinLength(2, ErrorMessage = "Минимум 2 символа")]
    public string FirstName { get; set; } = "";

    [Required(ErrorMessage = "Введите фамилию")]
    [MinLength(2, ErrorMessage = "Минимум 2 символа")]
    public string LastName { get; set; } = "";

    [Phone(ErrorMessage = "Некорректный номер телефона")]
    public string PhoneNumber { get; set; } = "";

    public string? PhotoUrl { get; set; }
    public IFormFile? Photo { get; set; }
}

public class ChangePasswordViewModel
{
    [Required(ErrorMessage = "Введите текущий пароль")]
    public string CurrentPassword { get; set; } = "";

    [Required(ErrorMessage = "Введите новый пароль")]
    [MinLength(6, ErrorMessage = "Минимум 6 символов")]
    public string NewPassword { get; set; } = "";

    [Required(ErrorMessage = "Подтвердите пароль")]
    [Compare(nameof(NewPassword), ErrorMessage = "Пароли не совпадают")]
    public string ConfirmPassword { get; set; } = "";
}
