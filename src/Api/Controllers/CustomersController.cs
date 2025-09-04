using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Services; // ICustomerService
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/customers")]
    public sealed class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customers;

        public CustomersController(ICustomerService customers)
        {
            _customers = customers;
        }

        public sealed class CreateCustomerRequest
        {
            public string Nome { get; set; } = default!;
            public string Email { get; set; } = default!;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var list = await _customers.GetAllAsync(ct);
            return Ok(list);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
            var c = await _customers.GetByIdAsync(id, ct);
            return c is null ? NotFound() : Ok(c);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCustomerRequest request, CancellationToken ct)
        {
            var id = await _customers.CreateAsync(request.Nome, request.Email, ct);
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }
    }
}
