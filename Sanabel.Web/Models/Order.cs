using Sanabel.Web.Enum;

namespace Sanabel.Web.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; }

        public List<OrderItem> Items { get; set; } = new List<OrderItem>();

        public string? Notes { get; set; }
        public string? Location { get; set; } // ممكن يكون عنوان أو لوكيشن GPS
        public string? PhoneNumber { get; set; } // إذا لم يكن ضمن ApplicationUser وتريد نسخه في الطلب
        public decimal ShippingCost { get; internal set; }
        public decimal DepositAmount { get; internal set; }
        public byte[]? PaymentImage { get; set; } // nullable
        public string? PaymentImageName { get; set; }
        public string? PaymentImageType { get; set; }
    }
}
