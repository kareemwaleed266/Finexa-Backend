namespace Finexa.Application.Modules.Admin.DTOs
{
    public class AdminBillsOverviewDto
    {
        public AdminBillsStatsDto Summary { get; set; } = new();

        public List<AdminBillsByStatusDto> ByStatus { get; set; } = new();

        public List<AdminTopBillCategoryDto> TopCategories { get; set; } = new();
    }
}