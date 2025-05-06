using System.ComponentModel.DataAnnotations;

namespace Sanabel.Web.ViewModels
{
    public class FeedbackViewModel
    {
        public string UserId { get; set; }

        [Display(Name = "الاسم")]
        public string FullName { get; set; }

        public byte[] ProfilePicture { get; set; }

        [Required(ErrorMessage = "الرسالة مطلوبة")]
        [Display(Name = "تقييمك")]
        [StringLength(500, ErrorMessage = "يجب ألا يتجاوز التقييم 500 حرف")]
        public string Message { get; set; }
    }
}