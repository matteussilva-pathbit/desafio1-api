using Domain.Entities;
using Domain.Repositories;
using System.Collections.Generic;

namespace Application.Customers
{
    public class CustomerService
    {
        private readonly ICustomerRepository _customerRepository;

        public CustomerService(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public IEnumerable<Customer> ListarClientes()
        {
            return _customerRepository.ObterTodos();
        }

        public void AdicionarCliente(Customer customer)
        {
            _customerRepository.Adicionar(customer);
        }
    }
}
