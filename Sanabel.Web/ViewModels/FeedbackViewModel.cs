using System.ComponentModel.DataAnnotations;

namespace Sanabel.Web.ViewModels
{
    public class FeedbackViewModel
    {
        [Required(ErrorMessage = "الرسالة مطلوبة")]
        [Display(Name = "تقييمك")]
        [StringLength(500, ErrorMessage = "يجب ألا يتجاوز التقييم 500 حرف")]
        public string Message { get; set; }
    }
}