using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMAS.API.DTOs;
using SMAS.API.Models;
using SMAS.API.Data;

namespace SMAS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SuppliersController : ControllerBase
    {
        private readonly SmasDbContext _context;

        public SuppliersController(SmasDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var suppliers = await _context.Suppliers.ToListAsync();
            return Ok(new ApiResponse<IEnumerable<SupplierResponseDto>>
            {
                Success = true,
                Data = suppliers.Select(s => new SupplierResponseDto
                {
                    Id = s.Id,
                    CompanyName = s.CompanyName,
                    ContactName = s.ContactName,
                    Phone = s.Phone,
                    City = s.City,
                    Country = s.Country,
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt
                })
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null) return NotFound(new ApiResponse<string> { Success = false, Message = "Supplier not found" });

            return Ok(new ApiResponse<SupplierResponseDto> { Success = true, Data = new SupplierResponseDto
            {
                Id = supplier.Id,
                CompanyName = supplier.CompanyName,
                ContactName = supplier.ContactName,
                Phone = supplier.Phone,
                City = supplier.City,
                Country = supplier.Country,
                CreatedAt = supplier.CreatedAt,
                UpdatedAt = supplier.UpdatedAt
            }});
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SupplierCreateDto dto)
        {
            var supplier = new Supplier
            {
                CompanyName = dto.CompanyName,
                ContactName = dto.ContactName,
                Phone = dto.Phone,
                City = dto.City,
                Country = dto.Country
            };
            _context.Suppliers.Add(supplier);
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<SupplierResponseDto> { Success = true, Data = new SupplierResponseDto
            {
                Id = supplier.Id,
                CompanyName = supplier.CompanyName,
                ContactName = supplier.ContactName,
                Phone = supplier.Phone,
                City = supplier.City,
                Country = supplier.Country,
                CreatedAt = supplier.CreatedAt,
                UpdatedAt = supplier.UpdatedAt
            }, Message = "Supplier created" });
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] SupplierUpdateDto dto)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null) return NotFound(new ApiResponse<string> { Success = false, Message = "Supplier not found" });

            supplier.CompanyName = dto.CompanyName;
            supplier.ContactName = dto.ContactName;
            supplier.Phone = dto.Phone;
            supplier.City = dto.City;
            supplier.Country = dto.Country;
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<SupplierResponseDto> { Success = true, Data = new SupplierResponseDto
            {
                Id = supplier.Id,
                CompanyName = supplier.CompanyName,
                ContactName = supplier.ContactName,
                Phone = supplier.Phone,
                City = supplier.City,
                Country = supplier.Country,
                CreatedAt = supplier.CreatedAt,
                UpdatedAt = supplier.UpdatedAt
            }, Message = "Supplier updated" });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null) return NotFound(new ApiResponse<string> { Success = false, Message = "Supplier not found" });

            supplier.IsDeleted = true;
            await _context.SaveChangesAsync();
            return Ok(new ApiResponse<string> { Success = true, Message = "Supplier deleted" });
        }
    }
}