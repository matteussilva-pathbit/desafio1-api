using System.Net.Http;
using System.Text.Json;

namespace Infrastructure.External;

public class ViaCepService : IViaCepService
{
    private readonly HttpClient _http;
    public ViaCepService(HttpClient http) => _http = http;

    public async Task<ViaCepAddress?> GetAddressAsync(string cep, CancellationToken ct = default)
    {
        var resp = await _http.GetAsync($"https://viacep.com.br/ws/{cep}/json/", ct);
        if (!resp.IsSuccessStatusCode) return null;

        await using var stream = await resp.Content.ReadAsStreamAsync(ct);
        return await JsonSerializer.DeserializeAsync<ViaCepAddress>(stream, cancellationToken: ct);
    }
}
