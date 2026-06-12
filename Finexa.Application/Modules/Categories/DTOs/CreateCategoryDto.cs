using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Finexa.Domain.Enums;

namespace Finexa.Application.Modules.Categories.DTOs
{
    public class CreateCategoryDto
    {
        public string Name { get; set; } = null!;
        public TransactionType CategoryType { get; set; }
        public bool IsBillCategory { get; set; } = false;
    }
}
