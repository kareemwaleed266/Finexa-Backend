using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finexa.Application.Modules.Dashboard.DTOs
{
    public class CategoryBreakdownDto
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public ChangeDto? Change { get; set; } 

    }
}
