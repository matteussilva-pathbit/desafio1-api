using Application.DTOs.Product;

namespace Application.Interfaces;

public interface IProductService
{
    Task<List<ProductReadDto>> GetAllAsync(CancellationToken ct = default);
    Task<ProductReadDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ProductReadDto> CreateAsync(ProductCreateDto dto, CancellationToken ct = default);
    Task<bool> UpdateAsync(int id, ProductUpdateDto dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
