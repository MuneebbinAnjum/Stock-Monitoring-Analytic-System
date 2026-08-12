using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMAS.API.DTOs;
using SMAS.API.Data;
using SMAS.API.Models;

namespace SMAS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly SmasDbContext _context;

        public CustomersController(SmasDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Admin,Manager,Salesman")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var customers = await _context.Customers.ToListAsync();
            return Ok(new ApiResponse<IEnumerable<CustomerResponseDto>>
            {
                Success = true,
                Data = customers.Select(c => new CustomerResponseDto
                {
                    Id = c.Id,
                    FullName = c.FullName ?? string.Empty,
                    Email = c.Email ?? string.Empty,
                    Phone = c.Phone ?? string.Empty,
                    City = c.City ?? string.Empty,
                    Province = c.Province ?? string.Empty,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                })
            });
        }

        [Authorize(Roles = "Admin,Manager,Buyer")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return NotFound(new ApiResponse<string> { Success = false, Message = "Customer not found" });

            return Ok(new ApiResponse<CustomerResponseDto> { Success = true, Data = new CustomerResponseDto
            {
                Id = customer.Id,
                FullName = customer.FullName ?? string.Empty,
                Email = customer.Email ?? string.Empty,
                Phone = customer.Phone ?? string.Empty,
                City = customer.City ?? string.Empty,
                Province = customer.Province ?? string.Empty,
                CreatedAt = customer.CreatedAt,
                UpdatedAt = customer.UpdatedAt
            }});
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CustomerCreateDto dto)
        {
            var existing = await _context.Customers.AnyAsync(c => c.Email == dto.Email);
            if (existing) return BadRequest(new ApiResponse<string> { Success = false, Message = "Email already exists" });

            var customer = new Customer
            {
                FullName = dto.FullName,
                Email = dto.Email,
                Phone = dto.Phone,
                City = dto.City,
                Province = dto.Province,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<CustomerResponseDto> { Success = true, Data = new CustomerResponseDto
            {
                Id = customer.Id,
                FullName = customer.FullName ?? string.Empty,
                Email = customer.Email ?? string.Empty,
                Phone = customer.Phone ?? string.Empty,
                City = customer.City ?? string.Empty,
                Province = customer.Province ?? string.Empty,
                CreatedAt = customer.CreatedAt,
                UpdatedAt = customer.UpdatedAt
            }, Message = "Customer created" });
        }

        [Authorize(Roles = "Admin,Manager,Buyer")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] CustomerUpdateDto dto)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return NotFound(new ApiResponse<string> { Success = false, Message = "Customer not found" });

            customer.FullName = dto.FullName;
            customer.Email = dto.Email;
            customer.Phone = dto.Phone;
            customer.City = dto.City;
            customer.Province = dto.Province;
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<CustomerResponseDto> { Success = true, Data = new CustomerResponseDto
            {
                Id = customer.Id,
                FullName = customer.FullName ?? string.Empty,
                Email = customer.Email ?? string.Empty,
                Phone = customer.Phone ?? string.Empty,
                City = customer.City ?? string.Empty,
                Province = customer.Province ?? string.Empty,
                CreatedAt = customer.CreatedAt,
                UpdatedAt = customer.UpdatedAt
            }, Message = "Customer updated" });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return NotFound(new ApiResponse<string> { Success = false, Message = "Customer not found" });

            customer.IsDeleted = true;
            await _context.SaveChangesAsync();
            return Ok(new ApiResponse<string> { Success = true, Message = "Customer deleted" });
        }
    }
}