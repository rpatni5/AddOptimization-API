using AddOptimization.Contracts.Constants;
using AddOptimization.Data.Common;
using System.ComponentModel.DataAnnotations;

namespace AddOptimization.Data.Entities
{
    public class ApplicationUser :BaseEntityNew<int>
    {
        [MaxLength(200)]
        public string FirstName { get; set; }

        [MaxLength(200)]
        public string LastName { get; set; }

        [MaxLength(200)]
        public string Email { get; set; }

        [MaxLength(500)]
        public string FullName { get; set; }

        [Required]
        [MaxLength(200)]
        public string UserName { get; set; }

        [MaxLength(200)]
        public string Password { get; set; }
        public bool IsActive { get; set; }
        public bool? IsLocked { get; set; }
        public bool? IsEmailsEnabled { get; set; }
        public int? FailedLoginAttampts { get; set; }
        public DateTime? LastLogin { get; set; }
        public virtual ICollection<UserRole> UserRoles { get; set; }

    }
}
