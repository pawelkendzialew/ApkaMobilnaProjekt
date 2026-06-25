using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using MauiAppMobile.Models;
using MauiAppMobile.Models.Enums;

namespace MauiAppMobile.Services.Database
{
    public class DatabaseService : IDatabaseService
    {
        
        private readonly string _connectionString =
            "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=TransportManagementDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=True";

        public DatabaseService()
        {
            TestConnection();
        }

        private void TestConnection()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    System.Diagnostics.Debug.WriteLine("✅ Połączono z SQL Server!");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Błąd połączenia: {ex.Message}");
            }
        }

        #region Users

        public async Task<User> GetUserByUsernameAsync(string username)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand(
                    @"SELECT Id, Username, PasswordHash, FirstName, LastName, Email, PhoneNumber, 
                      Role, IsActive, CreatedAt, LastLoginAt 
                      FROM Users WHERE Username = @Username",
                    connection);

                command.Parameters.AddWithValue("@Username", username);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return MapUser(reader);
                    }
                }
            }
            return null;
        }

        public async Task<User> GetUserByIdAsync(int userId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand(
                    @"SELECT Id, Username, PasswordHash, FirstName, LastName, Email, PhoneNumber, 
                      Role, IsActive, CreatedAt, LastLoginAt 
                      FROM Users WHERE Id = @Id",
                    connection);

                command.Parameters.AddWithValue("@Id", userId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return MapUser(reader);
                    }
                }
            }
            return null;
        }

        public async Task<List<User>> GetAllEmployeesAsync()
        {
            var users = new List<User>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand(
                    @"SELECT Id, Username, PasswordHash, FirstName, LastName, Email, PhoneNumber, 
                      Role, IsActive, CreatedAt, LastLoginAt 
                      FROM Users WHERE Role = 'Employee' AND IsActive = 1
                      ORDER BY LastName, FirstName",
                    connection);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        users.Add(MapUser(reader));
                    }
                }
            }
            return users;
        }

        public async Task<List<User>> GetAllAdministratorsAsync()
        {
            var users = new List<User>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand(
                    @"SELECT Id, Username, PasswordHash, FirstName, LastName, Email, PhoneNumber, 
                      Role, IsActive, CreatedAt, LastLoginAt 
                      FROM Users WHERE Role = 'Administrator' AND IsActive = 1
                      ORDER BY LastName, FirstName",
                    connection);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        users.Add(MapUser(reader));
                    }
                }
            }
            return users;
        }

        public async Task UpdateLastLoginAsync(int userId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand(
                    "UPDATE Users SET LastLoginAt = @LastLoginAt WHERE Id = @Id",
                    connection);

                command.Parameters.AddWithValue("@Id", userId);
                command.Parameters.AddWithValue("@LastLoginAt", DateTime.Now);

                await command.ExecuteNonQueryAsync();
            }
        }

        private User MapUser(SqlDataReader reader)
        {
            return new User
            {
                Id = reader.GetInt32(0),
                Username = reader.GetString(1),
                PasswordHash = reader.GetString(2),
                FirstName = reader.GetString(3),
                LastName = reader.GetString(4),
                Email = reader.IsDBNull(5) ? null : reader.GetString(5),
                PhoneNumber = reader.IsDBNull(6) ? null : reader.GetString(6),
                Role = Enum.Parse<UserRole>(reader.GetString(7)),
                IsActive = reader.GetBoolean(8),
                CreatedAt = reader.GetDateTime(9),
                LastLoginAt = reader.IsDBNull(10) ? null : reader.GetDateTime(10)
            };
        }

        #endregion

        #region Orders

        public async Task<List<Order>> GetOrdersForEmployeeAsync(int employeeId)
        {
            var orders = new List<Order>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand(
                    @"SELECT o.Id, o.Title, o.OrderDate, o.StartTime, o.RouteFrom, o.RouteTo, 
                      o.VehicleId, o.AssignedEmployeeId, o.CreatedByAdminId, o.Status, 
                      o.AdditionalInfo, o.CrewInfo, o.CompanyInfo, o.AcceptedAt, o.StartedAt, 
                      o.CompletedAt, o.WorkDurationMinutes, o.RejectionReason, o.CreatedAt, o.UpdatedAt,
                      v.Id, v.Brand, v.Model, v.RegistrationNumber, v.Status,
                      u.Id, u.FirstName, u.LastName
                      FROM Orders o
                      INNER JOIN Vehicles v ON o.VehicleId = v.Id
                      INNER JOIN Users u ON o.AssignedEmployeeId = u.Id
                      WHERE o.AssignedEmployeeId = @EmployeeId
                      ORDER BY o.OrderDate DESC, o.StartTime DESC",
                    connection);

                command.Parameters.AddWithValue("@EmployeeId", employeeId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        orders.Add(MapOrderWithVehicle(reader));
                    }
                }
            }
            return orders;
        }

        public async Task<List<Order>> GetOrdersByStatusAsync(OrderStatus status)
        {
            var orders = new List<Order>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand(
                    @"SELECT o.Id, o.Title, o.OrderDate, o.StartTime, o.RouteFrom, o.RouteTo, 
                      o.VehicleId, o.AssignedEmployeeId, o.CreatedByAdminId, o.Status, 
                      o.AdditionalInfo, o.CrewInfo, o.CompanyInfo, o.AcceptedAt, o.StartedAt, 
                      o.CompletedAt, o.WorkDurationMinutes, o.RejectionReason, o.CreatedAt, o.UpdatedAt,
                      v.Id, v.Brand, v.Model, v.RegistrationNumber, v.Status,
                      u.Id, u.FirstName, u.LastName
                      FROM Orders o
                      INNER JOIN Vehicles v ON o.VehicleId = v.Id
                      INNER JOIN Users u ON o.AssignedEmployeeId = u.Id
                      WHERE o.Status = @Status
                      ORDER BY o.OrderDate DESC, o.StartTime DESC",
                    connection);

                command.Parameters.AddWithValue("@Status", status.ToString());

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        orders.Add(MapOrderWithVehicle(reader));
                    }
                }
            }
            return orders;
        }

        public async Task<List<Order>> GetAllOrdersAsync()
        {
            var orders = new List<Order>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand(
                    @"SELECT o.Id, o.Title, o.OrderDate, o.StartTime, o.RouteFrom, o.RouteTo, 
                      o.VehicleId, o.AssignedEmployeeId, o.CreatedByAdminId, o.Status, 
                      o.AdditionalInfo, o.CrewInfo, o.CompanyInfo, o.AcceptedAt, o.StartedAt, 
                      o.CompletedAt, o.WorkDurationMinutes, o.RejectionReason, o.CreatedAt, o.UpdatedAt,
                      v.Id, v.Brand, v.Model, v.RegistrationNumber, v.Status,
                      u.Id, u.FirstName, u.LastName
                      FROM Orders o
                      INNER JOIN Vehicles v ON o.VehicleId = v.Id
                      INNER JOIN Users u ON o.AssignedEmployeeId = u.Id
                      ORDER BY o.OrderDate DESC, o.StartTime DESC",
                    connection);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        orders.Add(MapOrderWithVehicle(reader));
                    }
                }
            }
            return orders;
        }

        public async Task<Order> GetOrderByIdAsync(int orderId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand(
                    @"SELECT o.Id, o.Title, o.OrderDate, o.StartTime, o.RouteFrom, o.RouteTo, 
                      o.VehicleId, o.AssignedEmployeeId, o.CreatedByAdminId, o.Status, 
                      o.AdditionalInfo, o.CrewInfo, o.CompanyInfo, o.AcceptedAt, o.StartedAt, 
                      o.CompletedAt, o.WorkDurationMinutes, o.RejectionReason, o.CreatedAt, o.UpdatedAt,
                      v.Id, v.Brand, v.Model, v.RegistrationNumber, v.Status,
                      u.Id, u.FirstName, u.LastName
                      FROM Orders o
                      INNER JOIN Vehicles v ON o.VehicleId = v.Id
                      INNER JOIN Users u ON o.AssignedEmployeeId = u.Id
                      WHERE o.Id = @Id",
                    connection);

                command.Parameters.AddWithValue("@Id", orderId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return MapOrderWithVehicle(reader);
                    }
                }
            }
            return null;
        }

        public async Task<int> CreateOrderAsync(Order order)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand(
                    @"INSERT INTO Orders (Title, OrderDate, StartTime, RouteFrom, RouteTo, VehicleId, 
                      AssignedEmployeeId, CreatedByAdminId, Status, AdditionalInfo, CrewInfo, CompanyInfo, 
                      CreatedAt, UpdatedAt)
                      VALUES (@Title, @OrderDate, @StartTime, @RouteFrom, @RouteTo, @VehicleId, 
                      @AssignedEmployeeId, @CreatedByAdminId, @Status, @AdditionalInfo, @CrewInfo, @CompanyInfo, 
                      @CreatedAt, @UpdatedAt);
                      SELECT CAST(SCOPE_IDENTITY() as int)",
                    connection);

                command.Parameters.AddWithValue("@Title", order.Title);
                command.Parameters.AddWithValue("@OrderDate", order.OrderDate);
                command.Parameters.AddWithValue("@StartTime", order.StartTime);
                command.Parameters.AddWithValue("@RouteFrom", order.RouteFrom);
                command.Parameters.AddWithValue("@RouteTo", order.RouteTo);
                command.Parameters.AddWithValue("@VehicleId", order.VehicleId);
                command.Parameters.AddWithValue("@AssignedEmployeeId", order.AssignedEmployeeId);
                command.Parameters.AddWithValue("@CreatedByAdminId", order.CreatedByAdminId);
                command.Parameters.AddWithValue("@Status", order.Status.ToString());
                command.Parameters.AddWithValue("@AdditionalInfo", order.AdditionalInfo ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@CrewInfo", order.CrewInfo ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@CompanyInfo", order.CompanyInfo ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@CreatedAt", order.CreatedAt);
                command.Parameters.AddWithValue("@UpdatedAt", order.UpdatedAt);

                return (int)await command.ExecuteScalarAsync();
            }
        }

        public async Task UpdateOrderAsync(Order order)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand(
                    @"UPDATE Orders SET 
                      Title = @Title, 
                      OrderDate = @OrderDate, 
                      StartTime = @StartTime, 
                      RouteFrom = @RouteFrom, 
                      RouteTo = @RouteTo, 
                      Status = @Status, 
                      AdditionalInfo = @AdditionalInfo, 
                      CrewInfo = @CrewInfo, 
                      CompanyInfo = @CompanyInfo,
                      AcceptedAt = @AcceptedAt,
                      StartedAt = @StartedAt,
                      CompletedAt = @CompletedAt,
                      WorkDurationMinutes = @WorkDurationMinutes,
                      RejectionReason = @RejectionReason,
                      UpdatedAt = @UpdatedAt
                      WHERE Id = @Id",
                    connection);

                command.Parameters.AddWithValue("@Id", order.Id);
                command.Parameters.AddWithValue("@Title", order.Title);
                command.Parameters.AddWithValue("@OrderDate", order.OrderDate);
                command.Parameters.AddWithValue("@StartTime", order.StartTime);
                command.Parameters.AddWithValue("@RouteFrom", order.RouteFrom);
                command.Parameters.AddWithValue("@RouteTo", order.RouteTo);
                command.Parameters.AddWithValue("@Status", order.Status.ToString());
                command.Parameters.AddWithValue("@AdditionalInfo", order.AdditionalInfo ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@CrewInfo", order.CrewInfo ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@CompanyInfo", order.CompanyInfo ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@AcceptedAt", order.AcceptedAt ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@StartedAt", order.StartedAt ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@CompletedAt", order.CompletedAt ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@WorkDurationMinutes", order.WorkDurationMinutes ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@RejectionReason", order.RejectionReason ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@UpdatedAt", order.UpdatedAt);

                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task DeleteOrderAsync(int orderId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand(
                    "DELETE FROM Orders WHERE Id = @Id",
                    connection);

                command.Parameters.AddWithValue("@Id", orderId);
                await command.ExecuteNonQueryAsync();
            }
        }

        private Order MapOrderWithVehicle(SqlDataReader reader)
        {
            return new Order
            {
                Id = reader.GetInt32(0),
                Title = reader.GetString(1),
                OrderDate = reader.GetDateTime(2),
                StartTime = reader.GetTimeSpan(3),
                RouteFrom = reader.GetString(4),
                RouteTo = reader.GetString(5),
                VehicleId = reader.GetInt32(6),
                AssignedEmployeeId = reader.GetInt32(7),
                CreatedByAdminId = reader.GetInt32(8),
                Status = Enum.Parse<OrderStatus>(reader.GetString(9)),
                AdditionalInfo = reader.IsDBNull(10) ? null : reader.GetString(10),
                CrewInfo = reader.IsDBNull(11) ? null : reader.GetString(11),
                CompanyInfo = reader.IsDBNull(12) ? null : reader.GetString(12),
                AcceptedAt = reader.IsDBNull(13) ? null : reader.GetDateTime(13),
                StartedAt = reader.IsDBNull(14) ? null : reader.GetDateTime(14),
                CompletedAt = reader.IsDBNull(15) ? null : reader.GetDateTime(15),
                WorkDurationMinutes = reader.IsDBNull(16) ? null : reader.GetInt32(16),
                RejectionReason = reader.IsDBNull(17) ? null : reader.GetString(17),
                CreatedAt = reader.GetDateTime(18),
                UpdatedAt = reader.GetDateTime(19),
                Vehicle = new Vehicle
                {
                    Id = reader.GetInt32(20),
                    Brand = reader.GetString(21),
                    Model = reader.GetString(22),
                    RegistrationNumber = reader.GetString(23),
                    Status = Enum.Parse<VehicleStatus>(reader.GetString(24))
                },
                AssignedEmployee = new User
                {
                    Id = reader.GetInt32(25),
                    FirstName = reader.GetString(26),
                    LastName = reader.GetString(27)
                }
            };
        }

        #endregion

        #region Vehicles

        public async Task<List<Vehicle>> GetAllVehiclesAsync()
        {
            var vehicles = new List<Vehicle>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand(
                    @"SELECT Id, Brand, Model, RegistrationNumber, VIN, YearOfProduction, Capacity, 
                      Status, Notes, CreatedAt, UpdatedAt 
                      FROM Vehicles 
                      ORDER BY Brand, Model",
                    connection);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        vehicles.Add(MapVehicle(reader));
                    }
                }
            }
            return vehicles;
        }

        public async Task<List<Vehicle>> GetAvailableVehiclesAsync()
        {
            return await GetVehiclesByStatusAsync(VehicleStatus.Available);
        }

        public async Task<List<Vehicle>> GetVehiclesByStatusAsync(VehicleStatus status)
        {
            var vehicles = new List<Vehicle>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand(
                    @"SELECT Id, Brand, Model, RegistrationNumber, VIN, YearOfProduction, Capacity, 
                      Status, Notes, CreatedAt, UpdatedAt 
                      FROM Vehicles 
                      WHERE Status = @Status
                      ORDER BY Brand, Model",
                    connection);

                command.Parameters.AddWithValue("@Status", status.ToString());

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        vehicles.Add(MapVehicle(reader));
                    }
                }
            }
            return vehicles;
        }

        public async Task<Vehicle> GetVehicleByIdAsync(int vehicleId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand(
                    @"SELECT Id, Brand, Model, RegistrationNumber, VIN, YearOfProduction, Capacity, 
                      Status, Notes, CreatedAt, UpdatedAt 
                      FROM Vehicles 
                      WHERE Id = @Id",
                    connection);

                command.Parameters.AddWithValue("@Id", vehicleId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return MapVehicle(reader);
                    }
                }
            }
            return null;
        }

        public async Task<int> CreateVehicleAsync(Vehicle vehicle)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand(
                    @"INSERT INTO Vehicles (Brand, Model, RegistrationNumber, VIN, YearOfProduction, 
                      Capacity, Status, Notes, CreatedAt, UpdatedAt)
                      VALUES (@Brand, @Model, @RegistrationNumber, @VIN, @YearOfProduction, 
                      @Capacity, @Status, @Notes, @CreatedAt, @UpdatedAt);
                      SELECT CAST(SCOPE_IDENTITY() as int)",
                    connection);

                command.Parameters.AddWithValue("@Brand", vehicle.Brand);
                command.Parameters.AddWithValue("@Model", vehicle.Model);
                command.Parameters.AddWithValue("@RegistrationNumber", vehicle.RegistrationNumber);
                command.Parameters.AddWithValue("@VIN", vehicle.VIN ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@YearOfProduction", vehicle.YearOfProduction ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Capacity", vehicle.Capacity ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Status", vehicle.Status.ToString());
                command.Parameters.AddWithValue("@Notes", vehicle.Notes ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@CreatedAt", vehicle.CreatedAt);
                command.Parameters.AddWithValue("@UpdatedAt", vehicle.UpdatedAt);

                return (int)await command.ExecuteScalarAsync();
            }
        }

        public async Task UpdateVehicleAsync(Vehicle vehicle)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand(
                    @"UPDATE Vehicles SET 
                      Brand = @Brand, 
                      Model = @Model, 
                      RegistrationNumber = @RegistrationNumber, 
                      VIN = @VIN, 
                      YearOfProduction = @YearOfProduction, 
                      Capacity = @Capacity, 
                      Status = @Status, 
                      Notes = @Notes, 
                      UpdatedAt = @UpdatedAt
                      WHERE Id = @Id",
                    connection);

                command.Parameters.AddWithValue("@Id", vehicle.Id);
                command.Parameters.AddWithValue("@Brand", vehicle.Brand);
                command.Parameters.AddWithValue("@Model", vehicle.Model);
                command.Parameters.AddWithValue("@RegistrationNumber", vehicle.RegistrationNumber);
                command.Parameters.AddWithValue("@VIN", vehicle.VIN ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@YearOfProduction", vehicle.YearOfProduction ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Capacity", vehicle.Capacity ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Status", vehicle.Status.ToString());
                command.Parameters.AddWithValue("@Notes", vehicle.Notes ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);

                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task DeleteVehicleAsync(int vehicleId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand(
                    "DELETE FROM Vehicles WHERE Id = @Id",
                    connection);

                command.Parameters.AddWithValue("@Id", vehicleId);
                await command.ExecuteNonQueryAsync();
            }
        }

        private Vehicle MapVehicle(SqlDataReader reader)
        {
            return new Vehicle
            {
                Id = reader.GetInt32(0),
                Brand = reader.GetString(1),
                Model = reader.GetString(2),
                RegistrationNumber = reader.GetString(3),
                VIN = reader.IsDBNull(4) ? null : reader.GetString(4),
                YearOfProduction = reader.IsDBNull(5) ? null : reader.GetInt32(5),
                Capacity = reader.IsDBNull(6) ? null : reader.GetInt32(6),
                Status = Enum.Parse<VehicleStatus>(reader.GetString(7)),
                Notes = reader.IsDBNull(8) ? null : reader.GetString(8),
                CreatedAt = reader.GetDateTime(9),
                UpdatedAt = reader.GetDateTime(10)
            };
        }

        #endregion

        #region Order Status History

        public async Task AddOrderStatusHistoryAsync(int orderId, OrderStatus oldStatus,
            OrderStatus newStatus, int changedByUserId, string comment)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand(
                    @"INSERT INTO OrderStatusHistory (OrderId, OldStatus, NewStatus, ChangedByUserId, ChangedAt, Comment)
                      VALUES (@OrderId, @OldStatus, @NewStatus, @ChangedByUserId, @ChangedAt, @Comment)",
                    connection);

                command.Parameters.AddWithValue("@OrderId", orderId);
                command.Parameters.AddWithValue("@OldStatus", oldStatus.ToString());
                command.Parameters.AddWithValue("@NewStatus", newStatus.ToString());
                command.Parameters.AddWithValue("@ChangedByUserId", changedByUserId);
                command.Parameters.AddWithValue("@ChangedAt", DateTime.Now);
                command.Parameters.AddWithValue("@Comment", comment ?? (object)DBNull.Value);

                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task<List<OrderStatusHistory>> GetOrderStatusHistoryAsync(int orderId)
        {
            var history = new List<OrderStatusHistory>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand(
                    @"SELECT h.Id, h.OrderId, h.OldStatus, h.NewStatus, h.ChangedByUserId, h.ChangedAt, h.Comment
                      FROM OrderStatusHistory h
                      WHERE h.OrderId = @OrderId
                      ORDER BY h.ChangedAt DESC",
                    connection);

                command.Parameters.AddWithValue("@OrderId", orderId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        history.Add(new OrderStatusHistory
                        {
                            Id = reader.GetInt32(0),
                            OrderId = reader.GetInt32(1),
                            OldStatus = reader.IsDBNull(2) ? null : Enum.Parse<OrderStatus>(reader.GetString(2)),
                            NewStatus = Enum.Parse<OrderStatus>(reader.GetString(3)),
                            ChangedByUserId = reader.GetInt32(4),
                            ChangedAt = reader.GetDateTime(5),
                            Comment = reader.IsDBNull(6) ? null : reader.GetString(6)
                        });
                    }
                }
            }
            return history;
        }

        #endregion
    }
}
