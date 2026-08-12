using SMAS.API.DTOs;
using SMAS.API.Models;

namespace SMAS.API.Services
{
    public interface IOrderService
    {
        Task<OrderResponseDto> CreateOrderAsync(OrderCreateDto dto);
        Task<OrderResponseDto> GetOrderByNumberAsync(string orderNumber);
        Task<OrderResponseDto> UpdateOrderStatusAsync(Guid orderId, string status);
        Task<OrderResponseDto> ApproveOrderAsync(Guid orderId);
        Task<OrderResponseDto> RejectOrderAsync(Guid orderId);
        Task<OrderResponseDto> CancelOrderAsync(Guid orderId);
        Task<IEnumerable<OrderResponseDto>> GetOrdersWithDetailsAsync();
        Task<OrderResponseDto> GetOrderByIdAsync(Guid id);
        Task DispatchViaCourierAsync(Guid orderId, string courierType);
        Task<OrderResponseDto> ReceiveOrderAsync(Guid orderId);
    }
}