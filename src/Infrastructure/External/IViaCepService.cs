namespace Infrastructure.External;

public interface IViaCepService
{
    Task<ViaCepAddress?> GetAddressAsync(string cep, CancellationToken ct = default);
}

public class ViaCepAddress
{
    public string Cep { get; set; } = string.Empty;
    public string Logradouro { get; set; } = string.Empty;
    public string Bairro { get; set; } = string.Empty;
    public string Localidade { get; set; } = string.Empty;
    public string Uf { get; set; } = string.Empty;
    public bool Erro { get; set; }
}
