using Domain.Entities;
using System;
using System.Collections.Generic;

namespace Domain.Repositories
{
    public interface ICustomerRepository
    {
        void Adicionar(Customer customer);
        Customer? ObterPorId(Guid id);
        IEnumerable<Customer> ObterTodos();
    }
}
