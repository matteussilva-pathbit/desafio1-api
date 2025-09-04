using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Application.Interface;
using Infrastructure.Services;


namespace Infrastructure.Services
{
    public sealed class ViaCepService : IViaCepService
    {
        private readonly HttpClient _http;

        public ViaCepService(HttpClient httpClient)
        {
            _http = httpClient;
            if (_http.BaseAddress == null)
                _http.BaseAddress = new System.Uri("https://viacep.com.br/");
        }

        private sealed class ViaCepResponse
        {
            public string? cep { get; set; }
            public string? logradouro { get; set; }
            public string? bairro { get; set; }
            public string? localidade { get; set; }
            public string? uf { get; set; }
            public bool? erro { get; set; }
        }

        public async Task<string> GetEnderecoByCepAsync(string cep, CancellationToken ct)
        {
            cep = (cep ?? string.Empty).Trim().Replace("-", "");

            if (string.IsNullOrWhiteSpace(cep) || cep.Length < 8)
                return string.Empty;

            using var resp = await _http.GetAsync($"/ws/{cep}/json/", ct);
            if (!resp.IsSuccessStatusCode) return string.Empty;

            var json = await resp.Content.ReadAsStringAsync(ct);
            var data = JsonSerializer.Deserialize<ViaCepResponse>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (data == null || data.erro == true) return string.Empty;

            var partes = new[]
            {
                data.logradouro,
                data.bairro,
                (data.localidade, data.uf) is (string c, string u) ? $"{c} - {u}" : null
            };

            var endereco = string.Join(", ", partes ?? new string?[] { }).Replace("  ", " ").Trim(' ', ',');
            return string.IsNullOrWhiteSpace(endereco) ? string.Empty : endereco;
        }
    }
}
