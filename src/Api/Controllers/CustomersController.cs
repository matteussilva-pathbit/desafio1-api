using Microsoft.AspNetCore.Mvc;
using Application.Customers;
using Domain.Entities;
using System.Collections.Generic;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly CustomerService _customerService;

        public CustomersController(CustomerService customerService)
        {
            _customerService = customerService;
        }

        // GET: api/customers
        [HttpGet]
        public ActionResult<IEnumerable<Customer>> ListarClientes()
        {
            var clientes = _customerService.ListarClientes();
            return Ok(clientes);
        }

        // POST: api/customers
        [HttpPost]
        public ActionResult AdicionarCliente([FromBody] Customer customer)
        {
            if (customer == null)
                return BadRequest("Cliente inválido");

            _customerService.AdicionarCliente(customer);
            return Ok("Cliente adicionado com sucesso!");
        }
    }
}
