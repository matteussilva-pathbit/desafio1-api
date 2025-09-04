using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Services; // IProductService
using Domain.Entities;
using Domain.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/products")]
    public sealed class ProductsController : ControllerBase
    {
        private readonly IProductService _products;
        private readonly IProductRepository _repo; // para criação

        public ProductsController(IProductService products, IProductRepository repo)
        {
            _products = products;
            _repo = repo;
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
            public int Delta { get; set; } // positivo ou negativo
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
            var p = await _products.GetByIdAsync(id, ct);
            return Ok(p);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProductRequest req, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(req.Nome) || req.Preco <= 0 || req.QuantityAvailable < 0)
                return BadRequest("Dados inválidos.");

            var product = Product.Create(req.Nome.Trim(), req.Preco, req.QuantityAvailable);
            await _repo.AddAsync(product, ct);
            // precisa salvar transação via UoW; para simplificar, assuma SaveChanges no pipeline (ou mova para um ProductService.Create)
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, new { id = product.Id });
        }

        [HttpPut("{id:guid}/price")]
        public async Task<IActionResult> UpdatePrice(Guid id, [FromBody] UpdatePriceRequest req, CancellationToken ct)
        {
            await _products.UpdatePriceAsync(id, req.Preco, ct);
            return NoContent();
        }

        [HttpPut("{id:guid}/inventory")]
        public async Task<IActionResult> AdjustInventory(Guid id, [FromBody] AdjustInventoryRequest req, CancellationToken ct)
        {
            await _products.AdjustInventoryAsync(id, req.Delta, ct);
            return NoContent();
        }
    }
}
