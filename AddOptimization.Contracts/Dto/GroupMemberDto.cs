using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Contracts.Dto
{
    public class GroupMemberDto
        
    {
        public Guid Id { get; set; }
        public string GroupId { get; set; }
        public int UserId { get; set; }
        public DateTime? JoinedDate { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }


    public class CombineGroupModelDto

    {
        public GroupDto group { get; set; }
        public List<GroupMemberDto> groupMembers { get; set; }
      
    }
}
