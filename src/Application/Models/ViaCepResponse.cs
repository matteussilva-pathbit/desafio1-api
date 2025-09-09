namespace Application.Models
{
    public sealed record ViaCepResponse
    {
        public string? Cep { get; init; }
        public string? Logradouro { get; init; }
        public string? Bairro { get; init; }
        public string? Localidade { get; init; }
        public string? Uf { get; init; }
    }
}
