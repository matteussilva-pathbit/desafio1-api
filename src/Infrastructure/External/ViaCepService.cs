using System.Net.Http.Json;

namespace Infrastructure.External;

public class ViaCepAddress
{
    public string? logradouro { get; set; }
    public string? bairro { get; set; }
    public string? localidade { get; set; }
    public string? uf { get; set; }
    public bool? erro { get; set; }
    public override string ToString() => $"{logradouro}, {bairro} - {localidade}/{uf}";
}

public interface IViaCepService
{
    Task<ViaCepAddress?> GetAsync(string cep, CancellationToken ct = default);
}

public class ViaCepService : IViaCepService
{
    private readonly HttpClient _http;
    public ViaCepService(HttpClient http) => _http = http;

    public async Task<ViaCepAddress?> GetAsync(string cep, CancellationToken ct = default)
        => await _http.GetFromJsonAsync<ViaCepAddress>($"https://viacep.com.br/ws/{cep}/json", ct);
}
