using System.ComponentModel.DataAnnotations;

namespace Sanabel.Web.ViewModels
{
    public class MyOrderViewModel
    {
        public int OrderId { get; set; }
        public string UserId { get; set; }
        public string UserFullName { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal OrderTotal { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal DepositAmount { get; set; }
        public decimal RemainingAmount => OrderTotal + ShippingCost - DepositAmount;
        public string Status { get; set; }
        public string PaymentStatus { get; set; }
        public string ShippingAddress { get; set; }
        public string Notes { get; set; }
        public string RejectionReason { get; set; }

        [Display(Name = "صورة إثبات الدفع")]
        public IFormFile PaymentProofImage { get; set; }

        public string ExistingPaymentProofUrl { get; set; }
        public DateTime? PaymentProofUploadDate { get; set; }

        public List<MyOrderItemViewModel> Items { get; set; } = new List<MyOrderItemViewModel>();
    }
}