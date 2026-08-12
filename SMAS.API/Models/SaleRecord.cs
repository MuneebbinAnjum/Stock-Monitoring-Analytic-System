using System.ComponentModel.DataAnnotations;

namespace SMAS.API.Models
{
    public class SaleRecord : Entity
    {
        public Guid ProductId { get; set; }
        public Product? Product { get; set; }

        public Guid EmployeeId { get; set; }
        public Employee? Employee { get; set; }

        public DateTime SaleDate { get; set; }

        [Range(1, int.MaxValue)]
        public int QuantitySold { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal Revenue { get; set; }
    }
}