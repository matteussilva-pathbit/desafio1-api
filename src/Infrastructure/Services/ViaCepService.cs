using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Application.Interface;

namespace Infrastructure.Services
{
    public sealed class ViaCepService : IViaCepService
    {
        private readonly HttpClient _http;

        public ViaCepService(HttpClient http)
        {
            _http = http;
            if (_http.BaseAddress is null)
                _http.BaseAddress = new Uri("https://viacep.com.br/");
        }

        public async Task<ViaCepResponse?> BuscarEnderecoAsync(string cep, CancellationToken ct)
        {
            // manter só os dígitos
            cep = new string(cep.Where(char.IsDigit).ToArray());
            if (cep.Length != 8) return null;

            var resp = await _http.GetAsync($"/ws/{cep}/json/", ct);
            if (!resp.IsSuccessStatusCode) return null;

            var json = await resp.Content.ReadAsStringAsync(ct);
            var dto = JsonSerializer.Deserialize<ViaCepResponse>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (dto is null) return null;
            if (dto.Erro == true) return null; // CEP inexistente

            return dto;
        }
    }
}
