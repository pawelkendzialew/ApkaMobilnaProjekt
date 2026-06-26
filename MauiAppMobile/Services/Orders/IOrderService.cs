using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MauiAppMobile.Models;
using MauiAppMobile.Models.Enums;

namespace MauiAppMobile.Services.Orders
{
    public interface IOrderService
    {
        Task<List<Order>> GetOrdersForEmployeeAsync(int employeeId);
        Task<List<Order>> GetOrdersByStatusAsync(OrderStatus status);
        Task<List<Order>> GetAllOrdersAsync();
        Task<Order> GetOrderByIdAsync(int orderId);
        Task<int> CreateOrderAsync(Order order);
        Task<bool> AcceptOrderAsync(int orderId, int employeeId);
        Task<bool> RejectOrderAsync(int orderId, int employeeId, string reason);
        Task<bool> StartOrderAsync(int orderId, int employeeId);
        Task<bool> CompleteOrderAsync(int orderId, int employeeId);
        Task UpdateOrderAsync(Order order);
        Task DeleteOrderAsync(int orderId);
    }
}
