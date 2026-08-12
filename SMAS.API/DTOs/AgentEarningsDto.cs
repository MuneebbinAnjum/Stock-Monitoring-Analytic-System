namespace SMAS.API.DTOs
{
    public class AgentEarningsDto
    {
        public Guid SalesmanId { get; set; }
        public string SalesmanName { get; set; } = string.Empty;
        public decimal TotalEarnings { get; set; }
        public int Days { get; set; }
        public List<DailyEarningDto> DailyBreakdown { get; set; } = new List<DailyEarningDto>();
    }

    public class DailyEarningDto
    {
        public string Date { get; set; } = string.Empty;
        public decimal Earnings { get; set; }
    }
}
