using AutoMapper;
using EcommersProject.BLL.DTOs;
using EcommersProject.BLL.Interfaces;
using EcommersProject.DAL.Entities;
using EcommersProject.DAL.UnitOfWork;

namespace EcommersProject.BLL.Services;

public class PaymentService
    : GenericService<Payment, PaymentGetDto, PaymentCreateDto, PaymentUpdateDto>, IPaymentService
{
    public PaymentService(IUnitOfWork unitOfWork, IMapper mapper)
        : base(unitOfWork, mapper, unitOfWork.Payments) { }

    public async Task<PaymentGetDto?> GetByOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var payments = await Repository.FindAsync(p => p.OrderId == orderId, cancellationToken);
        var payment = payments.FirstOrDefault();
        return payment is null ? null : Mapper.Map<PaymentGetDto>(payment);
    }
}
