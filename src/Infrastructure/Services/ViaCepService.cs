using System.Net.Http.Json;
using Application.Interfaces;

namespace Infrastructure.Services
{
    public class ViaCepService : IViaCepService
    {
        private readonly HttpClient _http;
        public ViaCepService(HttpClient http) => _http = http;

        public async Task<ViaCepAddress?> GetAsync(string cep, CancellationToken ct = default)
        {
            var onlyDigits = new string(cep.Where(char.IsDigit).ToArray());
            return await _http.GetFromJsonAsync<ViaCepAddress>(
                $"https://viacep.com.br/ws/{onlyDigits}/json/",
                ct);
        }
    }
}
