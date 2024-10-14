using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Contracts.Dto
{
    public class SharedFolderRequestDto
    {
        public Guid Id { get; set; }
        public Guid FolderId { get; set; }
        public int SharedByUserId { get; set; }

        public List<SharedFolderDto> sharedFolder { get; set; }
        public string PermissionLevel { get; set; }
        public DateTime SharedDate { get; set; }

     
    }
    public class SharedFolderDto
    {
        public string Id { get; set; }
        public string Type { get; set; }
    }

    public class FolderPermissionLevelDto
    {
        public Guid FolderId { get; set; }
        public List<PermissionLevelFolderDto> PermissionLevelFolder { get; set; }
    }

    public class PermissionLevelFolderDto
    {
        public Guid Id { get; set; }
        public string PermissionLevel { get; set; }
    }

}
