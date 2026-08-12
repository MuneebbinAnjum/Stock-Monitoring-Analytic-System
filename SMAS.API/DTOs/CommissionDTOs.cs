using System.ComponentModel.DataAnnotations;

namespace SMAS.API.DTOs
{
    public class CommissionDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public Guid ProductId { get; set; }
        public decimal CommissionPercentage { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateCommissionDto
    {
        [Required]
        public Guid EmployeeId { get; set; }

        [Required]
        public Guid ProductId { get; set; }

        [Required]
        [Range(0, 100)]
        public decimal CommissionPercentage { get; set; }
    }

    public class UpdateCommissionDto
    {
        [Required]
        [Range(0, 100)]
        public decimal CommissionPercentage { get; set; }
    }

    public class SalesmanCommissionSummaryDto
    {
        public Guid SalesmanId { get; set; }
        public string SalesmanName { get; set; } = string.Empty;
        public string SalesmanEmail { get; set; } = string.Empty;
        public decimal MonthlySalary { get; set; }
        public decimal TotalCommissionEarned { get; set; }
        public decimal TotalAmountDue { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
    }
}
