﻿using AddOptimization.Data.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace AddOptimization.Data.Entities
{
    public class Group : BaseEntityNew<Guid>
    {
        public string Name { get; set; }
        public bool IsDeleted { get; set; }

    }

}