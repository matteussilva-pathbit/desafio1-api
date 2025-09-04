using System.Threading;
using System.Threading.Tasks;

namespace Application.Interface
{
    public interface IViaCepService
    {
        /// <summary>
        /// Retorna o endereço completo a partir do CEP. Se inválido, retorna string vazia.
        /// </summary>
        Task<string> GetEnderecoByCepAsync(string cep, CancellationToken ct);
    }
}
