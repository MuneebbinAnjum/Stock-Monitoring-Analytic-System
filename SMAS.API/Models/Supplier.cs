using System.ComponentModel.DataAnnotations;

namespace SMAS.API.Models
{
    public class Supplier : Entity
    {
        private string _companyName = string.Empty;
        private string _contactName = string.Empty;
        private string _phone = string.Empty;
        private string _city = string.Empty;
        private string _country = "Pakistan";

        [Required]
        [StringLength(100)]
        public string CompanyName
        {
            get => _companyName;
            set => _companyName = value ?? string.Empty;
        }

        [StringLength(100)]
        public string ContactName
        {
            get => _contactName;
            set => _contactName = value ?? string.Empty;
        }

        [StringLength(20)]
        public string Phone
        {
            get => _phone;
            set => _phone = value ?? string.Empty;
        }

        [StringLength(50)]
        public string City
        {
            get => _city;
            set => _city = value ?? string.Empty;
        }

        [StringLength(50)]
        public string Country
        {
            get => _country;
            set => _country = value ?? "Pakistan";
        }
    }
}