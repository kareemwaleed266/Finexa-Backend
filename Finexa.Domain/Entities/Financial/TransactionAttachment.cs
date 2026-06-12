using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finexa.Domain.Entities.Financial
{
    public class TransactionAttachment : BaseAuditableEntity<Guid>
    {
        public Guid TransactionId { get; set; }

        public string Url { get; set; } = default!;

        public string PublicId { get; set; } = default!;

        public string FileName { get; set; } = default!;

        public string ContentType { get; set; } = default!;

        public long SizeInBytes { get; set; }

        public AttachmentType Type { get; set; }

        public Transaction Transaction { get; set; } = default!;
    }
}
