using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly AppDbContext _db;
        public CustomerRepository(AppDbContext db) => _db = db;

        // Interface exige void
        public void Adicionar(Customer entity)
        {
            _db.Customers.Add(entity);
            _db.SaveChanges();
        }

        // Ajuste Guid -> int se necessário
        public Customer? ObterPorId(Guid id)
        {
            return _db.Customers
                      .AsNoTracking()
                      .FirstOrDefault(c => c.Id == id);
        }

        public IEnumerable<Customer> ObterTodos()
        {
            return _db.Customers
                      .AsNoTracking()
                      .ToList();
        }
    }
}
