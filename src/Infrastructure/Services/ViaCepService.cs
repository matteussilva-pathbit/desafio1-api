using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Application.Interface;   // IViaCepService
using Application.Models;     // ViaCepResponse

namespace Infrastructure.Services
{
    public sealed class ViaCepService : IViaCepService
    {
        private readonly HttpClient _http;

        public ViaCepService(HttpClient http)
        {
            _http = http;
        }

        public async Task<ViaCepResponse?> BuscarEnderecoAsync(string cep, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(cep))
                return null;

            // mantém só dígitos
            var digits = new string(cep.Where(char.IsDigit).ToArray());
            if (digits.Length != 8)
                return null;

            // GET https://viacep.com.br/ws/{cep}/json/
            var resp = await _http.GetAsync($"ws/{digits}/json/", ct);
            if (!resp.IsSuccessStatusCode)
                return null;

            var json = await resp.Content.ReadAsStringAsync(ct);

            // Se viacep retornar {"erro": true}, considera inválido
            using (var doc = JsonDocument.Parse(json))
            {
                if (doc.RootElement.TryGetProperty("erro", out var erroProp) &&
                    erroProp.ValueKind == JsonValueKind.True)
                    return null;
            }

            // Desserializa para o DTO compartilhado (case-insensitive)
            var dto = JsonSerializer.Deserialize<ViaCepResponse>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return dto;
        }
    }
}
