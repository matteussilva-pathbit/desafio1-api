namespace Application.Interfaces
{
    public class ViaCepAddress
    {
        public string? cep { get; set; }
        public string? logradouro { get; set; }
        public string? bairro { get; set; }
        public string? localidade { get; set; }
        public string? uf { get; set; }
        public bool? erro { get; set; }
    }

    public interface IViaCepService
    {
        Task<ViaCepAddress?> GetAsync(string cep, CancellationToken ct = default);
    }
}
