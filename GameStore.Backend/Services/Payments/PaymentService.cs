using GameStore.Backend.Data;
using GameStore.Backend.Dtos.Payment;
using GameStore.Backend.Enums;
using GameStore.Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Backend.Services.Payments;

public class PaymentService(GameStoreContext context, ILogger<PaymentService> logger, IPaymentProvider paymentProvider)
{

    public readonly GameStoreContext _context = context;
    public readonly ILogger<PaymentService> _logger = logger;
    public readonly IPaymentProvider _paymentProvider = paymentProvider;



    public async Task<PaymentResultDto> StartPaymentAsync(int orderId, int userId)
    {

        var order = await _context.Orders.Include(o => o.Items).Where(o => o.UserId == userId).Where(o => o.PaymentStatus == PaymentStatus.Pending.ToString()).Where(o => o.Id == orderId).FirstOrDefaultAsync();

        if (order == null)
            throw new InvalidOperationException("No orders found for this user");
        if (order.PaymentStatus == PaymentStatus.Paid.ToString())
            throw new InvalidOperationException("Order Paid Already");
        var existingPendingPayment = await _context.Payments.AnyAsync(p => p.OrderId == order.Id && p.Status == PaymentStatus.Pending.ToString());

        if (existingPendingPayment)
            throw new InvalidOperationException("Payment already in progress for this order");

        var getPayment = await _paymentProvider.CreatePaymentAsync(order.Id, order.UserId, order.TotalAmount);

        var payment = new Payment
        {
            OrderId = orderId,
            UserId = userId,
            Amount = getPayment.Amount,
            Provider = getPayment.Provider,
            Status = getPayment.Status,
            ProviderPaymentId = getPayment.ProviderPaymentId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        return new PaymentResultDto
        {
            Status = getPayment.Status,
            Message = getPayment.Message,
            PaymentId = getPayment.ProviderPaymentId,
            Amount = getPayment.Amount,
            OrderId = order.Id

        };

    }
    public async Task<PaymentResultDto> ConfirmPaymentAsync(string providerPaymentId)
    {
        var payment = await _context.Payments.Include(p => p.Order).FirstOrDefaultAsync(p => p.ProviderPaymentId == providerPaymentId);

        if (payment == null)
            throw new InvalidOperationException("Payment not found");

        if (payment.Status == PaymentStatus.Paid.ToString())
            throw new InvalidOperationException("Payment already confirmed");

        if (payment.Status == PaymentStatus.Failed.ToString())
            throw new InvalidOperationException("Failed payment cannot be confirmed");

        var result = await _paymentProvider.ConfirmPaymentAsync(payment.ProviderPaymentId!);

        if (result.Success)

        {
            payment.Status = PaymentStatus.Paid.ToString();
            payment.CompletedAt = DateTime.UtcNow;
            payment.Order.PaymentStatus = PaymentStatus.Paid.ToString();
            payment.Order.PaidAt = DateTime.UtcNow;
        }
        else
        {
            payment.FailureReason = result.Message;
            payment.Status = PaymentStatus.Failed.ToString();
            payment.Order.PaymentStatus = PaymentStatus.Failed.ToString();
        }
        await _context.SaveChangesAsync();

        return new PaymentResultDto
        {
            Status = payment.Status,
            Message = result.Message,
            PaymentId = result.ProviderPaymentId,
            Amount = payment.Amount,
            OrderId = payment.OrderId,
            Provider = payment.Provider
        };
    }
    public async Task<PaymentResultDto> FailPaymentAsync(string providerPaymentId, string reason)
    {
        var payment = await _context.Payments.Include(p => p.Order).FirstOrDefaultAsync(p => p.ProviderPaymentId == providerPaymentId);

        if (payment == null)
            throw new InvalidOperationException("Payment not found");

        if (payment.Status == PaymentStatus.Paid.ToString())
            throw new InvalidOperationException("Payment already confirmed");

        if (payment.Status == PaymentStatus.Failed.ToString())
            throw new InvalidOperationException("Payment already marked as failed");

        var result = await _paymentProvider.FailPaymentAsync(payment.ProviderPaymentId!, reason);

        if (result.Success == false)
        {
            payment.FailureReason = result.Message;
            payment.Status = PaymentStatus.Failed.ToString();
            payment.Order.PaymentStatus = PaymentStatus.Pending.ToString();
            payment.CompletedAt = DateTime.UtcNow;
        }
        else
            throw new InvalidOperationException("Fail Payment returned success unexpectedly");
        await _context.SaveChangesAsync();

        return new PaymentResultDto
        {
            OrderId = payment.OrderId,
            PaymentId = providerPaymentId,
            Status = payment.Status,
            Message = result.Message,
            Amount = payment.Amount,
            Provider = payment.Provider
        };
    }

    public async Task<RetryPaymentDto> RetryPaymentAsync(int orderId, int userId)
    {
        var order = await _context.Orders.Include(o => o.Payments).FirstOrDefaultAsync(o => o.UserId == userId && o.Id == orderId);
        if (order == null)
            throw new InvalidOperationException("No Orders Found");
        if (order.Payments == null || order.Payments == null)
            throw new InvalidOperationException("No previous payment found to retry");

        var lastPayment = order.Payments.OrderByDescending(p => p.CreatedAt).First();

        if (lastPayment.Status != PaymentStatus.Failed.ToString())
            throw new InvalidOperationException("Only failed payments can be retried");
        var newPayment = new Payment
        {
            OrderId = orderId,
            UserId = userId,
            Amount = order.TotalAmount,
            CreatedAt = DateTime.UtcNow,
            Status = PaymentStatus.Pending.ToString(),
            Provider = lastPayment.Provider,
            ProviderPaymentId = lastPayment.ProviderPaymentId,
            CompletedAt = null
        };
        _context.Payments.Add(newPayment);
        await _context.SaveChangesAsync();

        return new RetryPaymentDto
        {
            ProviderPaymentId = newPayment.ProviderPaymentId

        };
    }

    public async Task<List<PaymentHistoryItemDto>> GetMyPaymentsAsync(int userId)
    {
        var payment = await _context.Payments.Where(p => p.UserId == userId).OrderByDescending(p => p.CompletedAt).ToListAsync();
        return payment.Select(p => new PaymentHistoryItemDto
        {
            PaymentId = p.Id,
            OrderId = p.OrderId,
            Amount = p.Amount,
            Provider = p.Provider,
            Status = p.Status,
            FailureReason = p.FailureReason,
            CreatedAt = p.CreatedAt,
            CompletedAt = p.CompletedAt
        }).ToList();
    }

    public async Task<PaymentHistoryDto> GetOrderPaymentsAsync(int userId, int orderId)
    {
        var payments = await _context.Payments
            .Where(p => p.UserId == userId && p.OrderId == orderId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return new PaymentHistoryDto
        {
            OrderId = orderId,
            Payments = payments.Select(p => new PaymentHistoryItemDto
            {
                PaymentId = p.Id,
                OrderId = p.OrderId,
                Amount = p.Amount,
                Provider = p.Provider,
                Status = p.Status,
                FailureReason = p.FailureReason,
                CreatedAt = p.CreatedAt,
                CompletedAt = p.CompletedAt
            }).ToList()
        };
    }
}
