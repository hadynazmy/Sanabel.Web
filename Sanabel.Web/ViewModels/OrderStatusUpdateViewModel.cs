using Sanabel.Web.Enum;
using System.ComponentModel.DataAnnotations;

namespace Sanabel.Web.ViewModels
{
    public class OrderStatusUpdateViewModel
    {
        public int OrderId { get; set; }

        [Required(ErrorMessage = "حالة الطلب مطلوبة")]
        public OrderStatus NewStatus { get; set; }

        [StringLength(500, ErrorMessage = "سبب الرفض يجب ألا يتجاوز 500 حرف")]
        public string RejectionReason { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "قيمة الشحن يجب أن تكون رقم موجب")]
        public decimal ShippingCost { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "المبلغ المدفوع يجب أن يكون رقم موجب")]
        public decimal DepositAmount { get; set; }
    }
}