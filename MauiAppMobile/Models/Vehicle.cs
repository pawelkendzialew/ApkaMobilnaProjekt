using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MauiAppMobile.Models.Enums;

namespace MauiAppMobile.Models
{
    public class Vehicle
    {
        public int Id { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public string RegistrationNumber { get; set; }
        public string VIN { get; set; }
        public int? YearOfProduction { get; set; }
        public int? Capacity { get; set; }
        public VehicleStatus Status { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public string DisplayName => $"{Brand} {Model} ({RegistrationNumber})";

        public string StatusText => Status switch
        {
            VehicleStatus.Available => "Dostępny",
            VehicleStatus.Unavailable => "Niedostępny",
            VehicleStatus.Service => "W serwisie",
            _ => "Nieznany"
        };
    }
}
