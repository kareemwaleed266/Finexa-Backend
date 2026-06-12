namespace Finexa.Application.Modules.Admin.DTOs
{
    public class AdminTopBillCategoryDto
    {
        public Guid CategoryId { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        public int BillSeriesCount { get; set; }

        public decimal ExpectedAmount { get; set; }
    }
}