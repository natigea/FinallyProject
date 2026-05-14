using EcommersProject.BLL.DTOs;

namespace EcommersProject.Models;

public class CartViewModel
{
    public CartGetDto? Cart { get; set; }
    public decimal Subtotal => Cart?.Items.Sum(i => i.UnitPrice * i.Quantity) ?? 0;
    public decimal Shipping => Subtotal > 0 ? 5.00m : 0;
    public decimal Total => Subtotal + Shipping;
}

public class CheckoutViewModel
{
    public CartGetDto? Cart { get; set; }
    public IReadOnlyList<AddressGetDto> Addresses { get; set; } = [];
    public decimal Subtotal => Cart?.Items.Sum(i => i.UnitPrice * i.Quantity) ?? 0;
    public decimal Shipping { get; set; } = 5.00m;
    public decimal Discount { get; set; }
    public decimal Total => Subtotal + Shipping - Discount;
    public Guid? SelectedAddressId { get; set; }
    public string? CouponCode { get; set; }
    public string? CouponMessage { get; set; }
    public Guid? CouponId { get; set; }

    public AddressCreateInput NewAddress { get; set; } = new();
}

public class AddressCreateInput
{
    public string Line1 { get; set; } = "";
    public string Line2 { get; set; } = "";
    public string City { get; set; } = "";
    public string State { get; set; } = "";
    public string PostalCode { get; set; } = "";
    public string Country { get; set; } = "";
}

public class OrderConfirmationViewModel
{
    public OrderGetDto Order { get; set; } = null!;
}
