using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Contracts.Dto
{
    public class NotificationDto
    {
        public int? Id { get; set; }
        [Required]
        public string Subject { get; set; }
        public string Content { get; set; }
        public string Link { get; set; }
        public string GroupKey { get; set; }
        public string Meta { get; set; }
        public bool IsRead { get; set; }
        public int? AppplicationUserId { get; set; }
        public NotificationUserDto CreatedByUser { get; set; }
        public DateTime? ReadAt { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? CreatedByUserId { get; set; }

    }
    public class NotificationUserDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
    }
}
