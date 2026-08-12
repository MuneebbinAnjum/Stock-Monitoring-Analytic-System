using System.ComponentModel.DataAnnotations;

namespace SMAS.API.DTOs
{
    public class SetSalaryDto
    {
        [Required]
        [Range(0, double.MaxValue)]
        public decimal MonthlySalary { get; set; }
    }

    public class SalarySummaryDto
    {
        public Guid SalesmanId { get; set; }
        public string SalesmanName { get; set; } = string.Empty;
        public string SalesmanEmail { get; set; } = string.Empty;
        public decimal MonthlySalary { get; set; }
        public decimal TotalCommissionEarned { get; set; }
        public decimal TotalAmountDue { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public int SalesRecordsCount { get; set; }
    }
}
