using System.ComponentModel.DataAnnotations;

namespace Sanabel.Web.ViewModels
{
    public class OrderPaymentViewModel
    {
        public int OrderId { get; set; }

        [Required(ErrorMessage = "صورة إثبات الدفع مطلوبة")]
        public IFormFile PaymentProofImage { get; set; }

        [Required(ErrorMessage = "المبلغ المدفوع مطلوب")]
        [Range(0.01, double.MaxValue, ErrorMessage = "المبلغ يجب أن يكون أكبر من الصفر")]
        public decimal PaidAmount { get; set; }

        [StringLength(500, ErrorMessage = "ملاحظات الدفع يجب ألا تتجاوز 500 حرف")]
        public string PaymentNotes { get; set; }
    }
}