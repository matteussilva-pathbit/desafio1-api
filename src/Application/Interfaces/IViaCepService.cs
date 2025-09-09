using System.Threading;
using System.Threading.Tasks;
using Application.Models;

namespace Application.Interface
{
    public interface IViaCepService
    {
        Task<ViaCepResponse?> BuscarEnderecoAsync(string cep, CancellationToken ct = default);
    }
}
