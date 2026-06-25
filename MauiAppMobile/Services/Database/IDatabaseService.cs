using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MauiAppMobile.Models;
using MauiAppMobile.Models.Enums;

namespace MauiAppMobile.Services.Database
{
    public interface IDatabaseService
    {
        // Users
        Task<User> GetUserByUsernameAsync(string username);
        Task<User> GetUserByIdAsync(int userId);
        Task<List<User>> GetAllEmployeesAsync();
        Task<List<User>> GetAllAdministratorsAsync();
        Task UpdateLastLoginAsync(int userId);

        // Orders
        Task<List<Order>> GetOrdersForEmployeeAsync(int employeeId);
        Task<List<Order>> GetOrdersByStatusAsync(OrderStatus status);
        Task<List<Order>> GetAllOrdersAsync();
        Task<Order> GetOrderByIdAsync(int orderId);
        Task<int> CreateOrderAsync(Order order);
        Task UpdateOrderAsync(Order order);
        Task DeleteOrderAsync(int orderId);

        // Vehicles
        Task<List<Vehicle>> GetAllVehiclesAsync();
        Task<List<Vehicle>> GetAvailableVehiclesAsync();
        Task<List<Vehicle>> GetVehiclesByStatusAsync(VehicleStatus status);
        Task<Vehicle> GetVehicleByIdAsync(int vehicleId);
        Task<int> CreateVehicleAsync(Vehicle vehicle);
        Task UpdateVehicleAsync(Vehicle vehicle);
        Task DeleteVehicleAsync(int vehicleId);

        // Order Status History
        Task AddOrderStatusHistoryAsync(int orderId, OrderStatus oldStatus, OrderStatus newStatus,
            int changedByUserId, string comment);
        Task<List<OrderStatusHistory>> GetOrderStatusHistoryAsync(int orderId);
    }
}
