namespace DTOs.Response.Accounts
{
    public class GetTotalAccount
    {
        public int totalAccount { get; set; }
        public int managersAccount { get; set; }
        public int customersAccount { get; set; }
        public int staffsAccount { get; set; }
        public int consultantAccount { get; set; }
    }
}
