using System.ComponentModel.DataAnnotations;

namespace Sanabel.Web.ViewModels
{
    public class RoleFormViewModel
    {
        [Required, MaxLength(256)]
        public string Name { get; set; }
    }
}
