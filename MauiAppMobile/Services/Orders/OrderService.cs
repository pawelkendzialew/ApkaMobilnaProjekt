using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MauiAppMobile.Models;
using MauiAppMobile.Models.Enums;
using MauiAppMobile.Services.Database;

namespace MauiAppMobile.Services.Orders
{
    public class OrderService : IOrderService
    {
        private readonly IDatabaseService _databaseService;

        public OrderService(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task<List<Order>> GetOrdersForEmployeeAsync(int employeeId)
        {
            return await _databaseService.GetOrdersForEmployeeAsync(employeeId);
        }

        public async Task<List<Order>> GetOrdersByStatusAsync(OrderStatus status)
        {
            return await _databaseService.GetOrdersByStatusAsync(status);
        }

        public async Task<List<Order>> GetAllOrdersAsync()
        {
            return await _databaseService.GetAllOrdersAsync();
        }

        public async Task<Order> GetOrderByIdAsync(int orderId)
        {
            return await _databaseService.GetOrderByIdAsync(orderId);
        }

        public async Task<int> CreateOrderAsync(Order order)
        {
            order.Status = OrderStatus.Sent;
            order.CreatedAt = DateTime.Now;
            order.UpdatedAt = DateTime.Now;
            return await _databaseService.CreateOrderAsync(order);
        }

        public async Task<bool> AcceptOrderAsync(int orderId, int employeeId)
        {
            var order = await _databaseService.GetOrderByIdAsync(orderId);
            if (order == null || order.AssignedEmployeeId != employeeId)
                return false;

            if (order.Status != OrderStatus.Sent)
                return false;

            var oldStatus = order.Status;
            order.Status = OrderStatus.Accepted;
            order.AcceptedAt = DateTime.Now;
            order.UpdatedAt = DateTime.Now;

            await _databaseService.UpdateOrderAsync(order);
            await _databaseService.AddOrderStatusHistoryAsync(
                orderId, oldStatus, OrderStatus.Accepted, employeeId, "Zlecenie przyjęte");

            return true;
        }

        public async Task<bool> RejectOrderAsync(int orderId, int employeeId, string reason)
        {
            var order = await _databaseService.GetOrderByIdAsync(orderId);
            if (order == null || order.AssignedEmployeeId != employeeId)
                return false;

            if (order.Status != OrderStatus.Sent)
                return false;

            var oldStatus = order.Status;
            order.Status = OrderStatus.Rejected;
            order.RejectionReason = reason;
            order.UpdatedAt = DateTime.Now;

            await _databaseService.UpdateOrderAsync(order);
            await _databaseService.AddOrderStatusHistoryAsync(
                orderId, oldStatus, OrderStatus.Rejected, employeeId, $"Odrzucone: {reason}");

            return true;
        }

        public async Task<bool> StartOrderAsync(int orderId, int employeeId)
        {
            var order = await _databaseService.GetOrderByIdAsync(orderId);
            if (order == null || order.AssignedEmployeeId != employeeId)
                return false;

            if (order.Status != OrderStatus.Accepted)
                return false;

            var oldStatus = order.Status;
            order.Status = OrderStatus.InProgress;
            order.StartedAt = DateTime.Now;
            order.UpdatedAt = DateTime.Now;

            await _databaseService.UpdateOrderAsync(order);
            await _databaseService.AddOrderStatusHistoryAsync(
                orderId, oldStatus, OrderStatus.InProgress, employeeId, "Rozpoczęto realizację");

            return true;
        }

        public async Task<bool> CompleteOrderAsync(int orderId, int employeeId)
        {
            var order = await _databaseService.GetOrderByIdAsync(orderId);
            if (order == null || order.AssignedEmployeeId != employeeId)
                return false;

            if (order.Status != OrderStatus.InProgress)
                return false;

            var oldStatus = order.Status;
            order.Status = OrderStatus.Completed;
            order.CompletedAt = DateTime.Now;
            order.UpdatedAt = DateTime.Now;

            // Oblicz czas pracy
            if (order.StartedAt.HasValue)
            {
                var duration = order.CompletedAt.Value - order.StartedAt.Value;
                order.WorkDurationMinutes = (int)duration.TotalMinutes;
            }

            await _databaseService.UpdateOrderAsync(order);
            await _databaseService.AddOrderStatusHistoryAsync(
                orderId, oldStatus, OrderStatus.Completed, employeeId,
                $"Zakończono - czas pracy: {order.WorkDurationText}");

            return true;
        }

        public async Task UpdateOrderAsync(Order order)
        {
            order.UpdatedAt = DateTime.Now;
            await _databaseService.UpdateOrderAsync(order);
        }

        public async Task DeleteOrderAsync(int orderId)
        {
            await _databaseService.DeleteOrderAsync(orderId);
        }
    }
}
