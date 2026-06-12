using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finexa.Application.Modules.Dashboard.DTOs
{
    public class ChangeDto
    {
        public decimal Value { get; set; }
        public string Label { get; set; } = string.Empty;
        public string Trend { get; set; } = string.Empty; // up / down / neutral
    }
}
