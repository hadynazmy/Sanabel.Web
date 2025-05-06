using System.ComponentModel.DataAnnotations;

namespace Sanabel.Web.Models
{
    public class Feedback
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } // إضافة معرف المستخدم

        [Required]
        public string Message { get; set; }

        [Required]
        public string FullName { get; set; }

        public byte[] ProfilePicture { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // علاقة مع المستخدم
        public virtual ApplicationUser User { get; set; }
    }
}