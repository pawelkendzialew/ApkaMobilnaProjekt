using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MauiAppMobile.Models.Enums;

namespace MauiAppMobile.Models
{
    public class OrderStatusHistory
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public OrderStatus? OldStatus { get; set; }
        public OrderStatus NewStatus { get; set; }
        public int ChangedByUserId { get; set; }
        public DateTime ChangedAt { get; set; }
        public string Comment { get; set; }
    }
}
