using GameStore.Backend.Data;
using GameStore.Backend.Dtos.Admin;
using GameStore.Backend.Dtos.Orders;
using GameStore.Backend.Dtos.Orders.Timeline;
using GameStore.Backend.Enums;
using GameStore.Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Backend.Services;

public class OrderService(GameStoreContext context, ILogger<OrderService> logger)
{
    private readonly GameStoreContext _context = context;
    private readonly ILogger<OrderService> _logger = logger;

    // CREATE ORDER (CHECKOUT)
    public async Task<OrderResponseDto> CreateOrderAsync(int userId)
    {
        _logger.LogInformation("Starting order creation for UserId={UserId}", userId);

        var basket = await _context.Baskets
            .Include(b => b.Items)
                .ThenInclude(i => i.Game)
            .FirstOrDefaultAsync(b => b.UserId == userId);

        if (basket == null || basket.Items.Count == 0)
        {
            _logger.LogWarning("Checkout failed: Basket empty for UserId={UserId}", userId);
            throw new InvalidOperationException("Basket is empty or not found");
        }

        var order = new Order
        {
            UserId = userId,
            PaymentStatus = OrderPaymentStatus.Pending.ToString(),
            CreatedAt = DateTime.UtcNow,
            PaidAt = null,
            TotalAmount = basket.Items.Sum(i => i.UnitPrice * i.Quantity),
            Items = []
        };

        foreach (var basketItem in basket.Items)
        {
            var orderItem = new OrderItem
            {
                GameID = basketItem.Game.ID,
                GameName = basketItem.Game.Name,
                Quantity = basketItem.Quantity,
                UnitPrice = basketItem.UnitPrice
            };

            order.Items.Add(orderItem);
        }

        _context.Orders.Add(order);
        _context.BasketItems.RemoveRange(basket.Items);

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Order created successfully. OrderId={OrderId}, UserId={UserId}, Total={Total}",
            order.Id, userId, order.TotalAmount
        );

        return new OrderResponseDto
        {
            OrderId = order.Id,
            TotalAmount = order.TotalAmount,
            PaymentStatus = order.PaymentStatus,
            CreatedAt = order.CreatedAt,
            Items = order.Items.Select(i => new OrderItemResponseDto
            {
                GameId = i.GameID,
                GameName = i.GameName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                SubTotal = i.UnitPrice * i.Quantity
            }).ToList()
        };
    }

    // GET MY ORDERS
    public async Task<List<OrderResponseDto>> GetMyOrdersAsync(int userId)
    {
        _logger.LogInformation("Fetching orders for UserId={UserId}", userId);

        var orders = await _context.Orders
            .Include(o => o.Items)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        var response = new List<OrderResponseDto>();

        foreach (var order in orders)
        {
            response.Add(new OrderResponseDto
            {
                OrderId = order.Id,
                TotalAmount = order.TotalAmount,
                PaymentStatus = order.PaymentStatus,
                CreatedAt = order.CreatedAt,
                Items = order.Items.Select(i => new OrderItemResponseDto
                {
                    GameId = i.GameID,
                    GameName = i.GameName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    SubTotal = i.Quantity * i.UnitPrice
                }).ToList()
            });
        }

        _logger.LogInformation(
            "Fetched {Count} orders for UserId={UserId}",
            response.Count, userId
        );

        return response;
    }

    // GET ORDER BY ID (ADMIN OR OWNER)
    public async Task<OrderResponseDto> GetOrderByIdAsync(int userId, int orderId, string role)
    {
        _logger.LogInformation(
            "Fetching order OrderId={OrderId} requested by UserId={UserId} Role={Role}",
            orderId, userId, role
        );

        var order = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
        {
            _logger.LogWarning("Order not found. OrderId={OrderId}", orderId);
            throw new InvalidOperationException("Order not found");
        }

        if (role != "Admin" && order.UserId != userId)
        {
            _logger.LogWarning(
                "Unauthorized order access attempt. OrderId={OrderId}, UserId={UserId}",
                orderId, userId
            );
            throw new UnauthorizedAccessException("Forbidden");
        }

        return new OrderResponseDto
        {
            OrderId = order.Id,
            TotalAmount = order.TotalAmount,
            PaymentStatus = order.PaymentStatus,
            CreatedAt = order.CreatedAt,
            Items = order.Items.Select(i => new OrderItemResponseDto
            {
                GameId = i.GameID,
                GameName = i.GameName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                SubTotal = i.UnitPrice * i.Quantity
            }).ToList()
        };
    }

    public async Task<OrderResponseDto> MarkOrderAsPaidAsync(int orderId)
    {
        var order = await _context.Orders.Include(i => i.Items).Where(o => o.Id == orderId).FirstOrDefaultAsync();
        if (order == null)
            throw new InvalidOperationException("Order not found");

        if (order.PaymentStatus == "Paid")
            throw new InvalidOperationException("Order already paid");

        if (order.PaymentStatus == "Cancelled")
            throw new InvalidOperationException("Cancelled order cannot be paid");

        order.PaymentStatus = OrderPaymentStatus.Paid.ToString();
        order.PaidAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        _logger.LogInformation(
        "Order {OrderId} marked as PAID at {PaidAt}",
        order.Id,
        order.PaidAt
    );

        return new OrderResponseDto
        {
            OrderId = order.Id,
            TotalAmount = order.TotalAmount,
            PaymentStatus = order.PaymentStatus,
            CreatedAt = order.CreatedAt,
            Items = order.Items.Select(i => new OrderItemResponseDto
            {
                GameId = i.GameID,
                GameName = i.GameName,
                Quantity = i.Quantity,
                SubTotal = i.UnitPrice * i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList()
        };
    }

    public async Task<OrderResponseDto> CancelOrderAsync(int orderId, int userId, string role)
    {
        var order = await _context.Orders.Include(i => i.Items).Where(o => o.Id == orderId).FirstOrDefaultAsync();

        if (order == null)
            throw new InvalidOperationException("Order not found");

        if (role != "Admin" && order.UserId != userId)
            throw new InvalidOperationException("Forbidden");

        if (order.PaymentStatus == "Paid")
            throw new InvalidOperationException("Paid Orders cannot be cancelled");
        if (order.PaymentStatus == "Cancelled")
            throw new InvalidOperationException("Order already cancelled");

        order.PaymentStatus = OrderPaymentStatus.Cancelled.ToString();
        await _context.SaveChangesAsync();
        _logger.LogInformation(
                "Order {OrderId} cancelled by {Role}",
                order.Id,
                role
            );
        return new OrderResponseDto
        {
            OrderId = order.Id,
            TotalAmount = order.TotalAmount,
            PaymentStatus = order.PaymentStatus,
            CreatedAt = order.CreatedAt,
            Items = order.Items.Select(i => new OrderItemResponseDto
            {
                GameId = i.GameID,
                GameName = i.GameName,
                Quantity = i.Quantity,
                SubTotal = i.UnitPrice * i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList()
        };
    }


    // Admin Service

    public async Task<List<AdminOrderListDto>> GetAllOrdersAdminAsync(AdminOrderFilterDto filter)
    {
        _logger.LogInformation("Admin requested all orders");
        var query = _context.Orders.Include(i => i.Items).Include(o => o.User).AsQueryable();

        if (filter.PaymentStatus.HasValue)
        {
            var status = filter.PaymentStatus.Value.ToString();
            query = query.Where(p => p.PaymentStatus == status);
        }

        if (filter.UserId.HasValue)
            query = query.Where(u => u.UserId == filter.UserId.Value);

        if (filter.UserName != null)
            query = query.Where(u => u.User.Name == filter.UserName);

        if (!string.IsNullOrWhiteSpace(filter.Email))
            query = query.Where(e => e.User.Email == filter.Email);
        if (filter.FromDate.HasValue)
            query = query.Where(o => o.CreatedAt >= filter.FromDate.Value);

        if (filter.ToDate.HasValue)
            query = query.Where(o => o.CreatedAt <= filter.ToDate.Value);

        if (filter.Page > 0 && filter.Limit > 0)
        {
            var skip = (filter.Page - 1) * filter.Limit;
            query = query.Skip(skip).Take(filter.Limit);
        }
        var orders = await query.Select(o => new AdminOrderListDto
        {
            UserId = o.UserId,
            OrderId = o.Id,
            UserName = o.User.Name,
            UserEmail = o.User.Email,
            CreatedAt = o.CreatedAt,
            PaymentMethod = o.PaymentMethod,
            PaymentStatus = o.PaymentStatus,
            PaidAt = o.PaidAt
        }).ToListAsync();

        _logger.LogInformation("Admin fetched {Count} orders", orders.Count);

        return orders;
    }

    public async Task<AdminOrderDetailsDto> GetOrderDetailsForAdminAsync(int orderId)
    {
        var order = await _context.Orders.Include(o => o.Items).Include(o => o.User).FirstOrDefaultAsync(o => o.Id == orderId);
        if (order == null)
            throw new InvalidOperationException("Order Not Found");

        var dto = new AdminOrderDetailsDto
        {
            OrderId = order.Id,
            TotalAmount = order.TotalAmount,
            PaymentStatus = order.PaymentStatus,
            PaymentMethod = order.PaymentMethod,
            CreatedAt = order.CreatedAt,
            PaidAt = order.PaidAt,

            UserId = order.UserId,
            UserEmail = order.User.Email,

            Items = order.Items.Select(i => new AdminOrderItemDto
            {
                GameId = i.GameID,
                GameName = i.GameName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                SubTotal = i.UnitPrice * i.Quantity
            }).ToList()
        };

        return dto;
    }

    //Order Timeline
    public async Task<List<TimelineDto>> GetOrderTimelineAsync(
    int orderId,
    int userId,
    string role
)
    {
        var order = await _context.Orders.Include(o => o.Payments).FirstOrDefaultAsync(o => o.Id == orderId);
        if (order is null)

            throw new InvalidOperationException("No Orders Found");

        if (role != "Admin" && order.UserId != userId)
            throw new UnauthorizedAccessException("You are not Authorized");

        var timeline = new List<TimelineDto>();

        // 1️⃣ ORDER CREATED EVENT (ONCE)
        timeline.Add(new TimelineDto
        {
            Timestamp = order.CreatedAt,
            EventType = "ORDER_CREATED",
            Status = order.PaymentStatus,
            Provider = "N/A",
            Amount=order.TotalAmount,
            Message = "Order Created"
        });

        // 2️⃣ PAYMENT EVENTS
        foreach (var payment in order.Payments)
        {
            string eventType;
            string message;

            if(payment.Status == PaymentStatus.Pending.ToString())
            {
                eventType = "PAYMENT_INITIATED";
                message = "Payment Initiated";
            }

            else if (payment.Status == PaymentStatus.Paid.ToString())
            {
                eventType = "PAYMENT_SUCCESS";
                message = "Payment Success";
            }
            else if (payment.Status == PaymentStatus.Failed.ToString())
            {
                eventType = "PAYMENT_FAILED";
                message = string.IsNullOrWhiteSpace(payment.FailureReason)
                    ? "Payment failed"
                    : $"Payment failed: {payment.FailureReason}";
            }
            else
            {
                eventType= "PAYMENT_UNKNOWN";
                message = "Unknown Payment state";
            }
            timeline.Add(new TimelineDto
            {
                Timestamp = payment.CompletedAt ?? payment.CreatedAt,
                EventType = eventType,
                Status = payment.Status,
                Provider = payment.Provider,
                Amount = payment.Amount,
                Message = message
            });
        }
        // 3️⃣ SORT: NEWEST FIRST
        return timeline
            .OrderByDescending(t => t.Timestamp).ToList();
    }
}
