using System.ComponentModel.DataAnnotations;
using EcommersProject.BLL.DTOs;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace EcommersProject.Models;

public class DeliveryViewModel
{
    [BindNever]
    public ListingGetDto Listing { get; set; } = null!;

    [Required(ErrorMessage = "Val_AddressRequired")]
    public string DeliveryAddress { get; set; } = "";

    [Required(ErrorMessage = "Val_CityRequired")]
    public string DeliveryCity { get; set; } = "";

    [Required(ErrorMessage = "Val_PhoneRequired")]
    public string DeliveryPhone { get; set; } = "";

    [Required]
    public string StripePaymentIntentId { get; set; } = "";
}

public class VipViewModel
{
    [BindNever]
    public ListingGetDto Listing { get; set; } = null!;

    [Required]
    [Range(1, 99, ErrorMessage = "Val_PlanRequired")]
    public int VipDays { get; set; } = 7;

    [Required]
    public string StripePaymentIntentId { get; set; } = "";
}

public class CreatePaymentIntentRequest
{
    public string Type { get; set; } = "delivery";
    public Guid ListingId { get; set; }
    public int VipDays { get; set; } = 7;
}
