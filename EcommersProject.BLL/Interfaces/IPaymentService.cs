using EcommersProject.BLL.DTOs;

namespace EcommersProject.BLL.Interfaces;

public interface IPaymentService : IGenericService<PaymentGetDto, PaymentCreateDto, PaymentUpdateDto>
{
    Task<PaymentGetDto?> GetByOrderAsync(Guid orderId, CancellationToken cancellationToken = default);
}
