using System.ComponentModel.DataAnnotations;

namespace Sejily.API.DTOs.Auth
{
    public class ResendOtpRequest
    {
        [Required]
        public Guid? RequestId { get; set; }
    }
}
