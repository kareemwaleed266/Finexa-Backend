using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finexa.Domain.Enums
{
    public enum BillOccurrenceType
    {
        Scheduled = 1,
        EarlyRenewal = 2,
        TopUp = 3,
        ExtraPayment = 4,
        Adjustment = 5
    }
}
