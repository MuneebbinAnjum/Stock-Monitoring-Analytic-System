using FluentValidation;

namespace SMAS.API.DTOs
{
    public class SupplierCreateDto
    {
        public string CompanyName { get; set; } = string.Empty;
        public string ContactName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = "Pakistan";
    }

    public class SupplierUpdateDto
    {
        public string CompanyName { get; set; } = string.Empty;
        public string ContactName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = "Pakistan";
    }

    public class SupplierResponseDto
    {
        public Guid Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string ContactName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = "Pakistan";
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class SupplierCreateDtoValidator : AbstractValidator<SupplierCreateDto>
    {
        public SupplierCreateDtoValidator()
        {
            RuleFor(x => x.CompanyName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.ContactName).MaximumLength(100);
            RuleFor(x => x.Phone).MaximumLength(20);
            RuleFor(x => x.City).MaximumLength(50);
            RuleFor(x => x.Country).MaximumLength(50);
        }
    }

    public class SupplierUpdateDtoValidator : AbstractValidator<SupplierUpdateDto>
    {
        public SupplierUpdateDtoValidator()
        {
            RuleFor(x => x.CompanyName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.ContactName).MaximumLength(100);
            RuleFor(x => x.Phone).MaximumLength(20);
            RuleFor(x => x.City).MaximumLength(50);
            RuleFor(x => x.Country).MaximumLength(50);
        }
    }
}