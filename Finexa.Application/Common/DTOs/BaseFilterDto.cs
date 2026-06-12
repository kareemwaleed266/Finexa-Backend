using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Finexa.Domain.Enums;

namespace Finexa.Application.Common.DTOs
{
    public class BaseFilterDto
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public SortDirection? SortDirection { get; set; }
    }
}
