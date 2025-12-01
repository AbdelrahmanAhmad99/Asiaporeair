using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Route
{
    public class UpdateOperatorDto
    {
        [Required(ErrorMessage = "Codeshare status is required.")]
        public bool IsCodeshare { get; set; }
    }
}