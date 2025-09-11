using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// injeta a implementação concreta do serviço de produtos (métodos com Guid)
using ProductServiceImpl = Application.Services.ProductService;

namespace API.Controllers
{
    [ApiController]
    [Route("api/products")]
    public sealed class ProductsController : ControllerBase
    {
        private readonly ProductServiceImpl _products;
        private readonly IProductRepository _repo;
        private readonly IUnitOfWork _uow;

        public ProductsController(ProductServiceImpl products, IProductRepository repo, IUnitOfWork uow)
        {
            _products = products;
            _repo = repo;
            _uow = uow;
        }

        public sealed class CreateProductRequest
        {
            public string Nome { get; set; } = default!;
            public decimal Preco { get; set; }
            public int QuantityAvailable { get; set; }
        }

        public sealed class UpdatePriceRequest
        {
            public decimal Preco { get; set; }
        }

        public sealed class AdjustInventoryRequest
        {
            public int Delta { get; set; }
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
            var p = await _repo.GetByIdAsync(id, ct);
            if (p is null) return NotFound();
            return Ok(new
            {
                p.Id, p.Nome, p.Preco, p.QuantityAvailable
            });
        }

        [Authorize(Policy = "ADMIN")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProductRequest req, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(req.Nome) || req.Preco <= 0 || req.QuantityAvailable < 0)
                return BadRequest("Dados inválidos.");

            var product = Product.Create(req.Nome.Trim(), req.Preco, req.QuantityAvailable);
            await _repo.AddAsync(product, ct);
            await _uow.SaveChangesAsync(ct);

            return CreatedAtAction(nameof(GetById), new { id = product.Id }, new { id = product.Id });
        }

        [Authorize(Policy = "ADMIN")]
        [HttpPut("{id:guid}/price")]
        public async Task<IActionResult> UpdatePrice(Guid id, [FromBody] UpdatePriceRequest req, CancellationToken ct)
        {
            await _products.UpdatePriceAsync(id, req.Preco, ct);
            return NoContent();
        }

        [Authorize(Policy = "ADMIN")]
        [HttpPut("{id:guid}/inventory")]
        public async Task<IActionResult> AdjustInventory(Guid id, [FromBody] AdjustInventoryRequest req, CancellationToken ct)
        {
            await _products.AdjustInventoryAsync(id, req.Delta, ct);
            return NoContent();
        }
    }
}
