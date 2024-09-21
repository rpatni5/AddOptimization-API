using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Contracts.Dto
{
    public class SharedEntryRequestDto
    {
        public Guid Id { get; set; }
        public Guid EntryId { get; set; }
        public int SharedByUserId { get; set; }

        public List<SharedFieldDto> sharedField { get; set; }
        public string PermissionLevel { get; set; }
        public DateTime SharedDate { get; set; }

     
    }

    public class SharedFieldDto
    {
        public string Id { get; set; }
        public string Type { get; set; }
    }

    public class PermissionLevelDto
    {
        public Guid EntryId { get; set; }
        public List<PermissionLevelEntriesDto> PermissionLevelEntries { get; set; }
    }

    public class PermissionLevelEntriesDto
    {
        public Guid Id { get; set; }
        public string PermissionLevel { get; set; }
    }

}
