namespace BusinessObject.DTOs.Response.Dashboards
{
    public class AdminAnalyticsResponse
    {
        public CourseCountResponse Course { get; set; }

        public IncomeAnalyticsResponse Income { get; set; }

        public AccountAnalyticsResponse Accounts { get; set; }
    }

    public class AccountAnalyticsResponse
    {
        public int TotalAccounts { get; set; }

        public int Teacher { get; set; }

        public int Parent {  get; set; }

        public int Student { get; set; }
    }
}
