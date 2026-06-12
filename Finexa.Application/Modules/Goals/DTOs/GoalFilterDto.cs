using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Finexa.Application.Common.DTOs;
using Finexa.Domain.Enums;

namespace Finexa.Application.Modules.Goals.DTOs
{
    public class GoalFilterDto : BaseFilterDto
    {
        public GoalStatus? Status { get; set; } // filter

        public GoalSortBy? SortBy { get; set; }
    }
}
