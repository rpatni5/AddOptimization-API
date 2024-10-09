using AddOptimization.Data.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace AddOptimization.Data.Entities
{
    public class Template : BaseEntityNew<Guid>
    {
        public string Name { get; set; }
        public string TemplateKey { get; set; }
        public bool IsDeleted { get; set; }
        public string TemplateKey { get; set; }

    }

}