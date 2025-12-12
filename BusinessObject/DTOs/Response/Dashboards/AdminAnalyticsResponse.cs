namespace BusinessObject.DTOs.Response.Dashboards
{
    /// <summary>
    /// Response cho Admin Analytics (sử dụng pattern chung)
    /// </summary>
    public class AdminAnalyticsResponse : BaseAnalyticsResponse<AdminAnalyticsMetadata, IncomeChartDataPoint>
    {
    }

    // Keep old class for backward compatibility (deprecated)
    [Obsolete("Use AccountCountMetadata instead")]
    public class AccountAnalyticsResponse
    {
        public int TotalAccounts { get; set; }

        public int Teacher { get; set; }

        public int Parent {  get; set; }

        public int Student { get; set; }
    }
}
