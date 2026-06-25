using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MauiAppMobile.Models.Enums;

namespace MauiAppMobile.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime OrderDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public string RouteFrom { get; set; }
        public string RouteTo { get; set; }
        public int VehicleId { get; set; }
        public int AssignedEmployeeId { get; set; }
        public int CreatedByAdminId { get; set; }
        public OrderStatus Status { get; set; }
        public string AdditionalInfo { get; set; }
        public string CrewInfo { get; set; }
        public string CompanyInfo { get; set; }
        public DateTime? AcceptedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int? WorkDurationMinutes { get; set; }
        public string RejectionReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public Vehicle Vehicle { get; set; }
        public User AssignedEmployee { get; set; }
        public User CreatedByAdmin { get; set; }

        // Computed properties
        public string Route => $"{RouteFrom} → {RouteTo}";

        public string StatusText => Status switch
        {
            OrderStatus.Sent => "Wysłane",
            OrderStatus.Accepted => "Przyjęte",
            OrderStatus.InProgress => "W trakcie",
            OrderStatus.Completed => "Zakończone",
            OrderStatus.Rejected => "Odrzucone",
            _ => "Nieznany"
        };

        public Color StatusColor => Status switch
        {
            OrderStatus.Sent => Colors.Orange,
            OrderStatus.Accepted => Colors.Blue,
            OrderStatus.InProgress => Colors.Green,
            OrderStatus.Completed => Colors.Gray,
            OrderStatus.Rejected => Colors.Red,
            _ => Colors.Black
        };

        public string WorkDurationText => WorkDurationMinutes.HasValue
            ? $"{WorkDurationMinutes.Value / 60}h {WorkDurationMinutes.Value % 60}min"
            : "—";
    }
}
