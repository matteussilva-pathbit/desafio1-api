using System.Threading;
using System.Threading.Tasks;

namespace Application.Interface
{
    // DTO simples compatível com a resposta do ViaCEP
    public sealed class ViaCepResponse
    {
        public string? Cep { get; set; }
        public string? Logradouro { get; set; }
        public string? Bairro { get; set; }
        public string? Localidade { get; set; }
        public string? Uf { get; set; }

        // ViaCEP envia { "erro": true } quando não encontra
        public bool? Erro { get; set; }
    }

    public interface IViaCepService
    {
        Task<ViaCepResponse?> BuscarEnderecoAsync(string cep, CancellationToken ct);
    }
}
