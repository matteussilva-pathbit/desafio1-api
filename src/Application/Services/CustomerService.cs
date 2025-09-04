using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain;
using Domain.Entities;
using Domain.Repositories;

namespace Application.Services
{
    public interface ICustomerService
    {
        Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken ct);
        Task<Guid> CreateAsync(string nome, string email, CancellationToken ct);
        Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct);
    }

    public sealed class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customers;
        private readonly IUnitOfWork _uow;

        public CustomerService(ICustomerRepository customers, IUnitOfWork uow)
        {
            _customers = customers;
            _uow = uow;
        }

        public Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken ct)
            => _customers.GetAllAsync(ct);

        public async Task<Guid> CreateAsync(string nome, string email, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(nome)) throw new DomainException("Nome é obrigatório.");
            if (string.IsNullOrWhiteSpace(email)) throw new DomainException("Email é obrigatório.");

            var existente = await _customers.GetByEmailAsync(email.Trim(), ct);
            if (existente is not null) throw new DomainException("Já existe cliente com esse e-mail.");

            var customer = Customer.Create(nome.Trim(), email.Trim());

            await _uow.BeginTransactionAsync(ct);
            try
            {
                await _customers.AddAsync(customer, ct);
                await _uow.SaveChangesAsync(ct);
                await _uow.CommitAsync(ct);
                return customer.Id;
            }
            catch
            {
                await _uow.RollbackAsync(ct);
                throw;
            }
        }

        public Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct)
            => _customers.GetByIdAsync(id, ct);
    }
}
