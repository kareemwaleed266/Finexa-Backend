namespace Finexa.Domain.Enums
{
    public enum AdminAuditAction
    {
        UserViewed = 1,
        UserLocked = 2,
        UserUnlocked = 3,
        UserActivated = 4,
        UserDeactivated = 5,
        UserPromotedToAdmin = 6,
        UserRemovedFromAdmin = 7,

        SystemCategoryCreated = 20,
        SystemCategoryUpdated = 21,
        SystemCategoryDeactivated = 22,
        SystemCategoryReactivated = 23,

        JobViewed = 40,
        AuditLogsViewed = 41,

        DashboardViewed = 60,
        BillsOverviewViewed = 61,
        AiUsageViewed = 62
    }
}