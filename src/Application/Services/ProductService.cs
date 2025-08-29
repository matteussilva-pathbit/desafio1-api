using Application.DTOs.Product;
using Application.Interfaces;   // <- ESSENCIAL
using AutoMapper;
using Domain.Entities;

namespace Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _repo;
    private readonly IMapper _mapper;

    public ProductService(IProductRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<List<ProductReadDto>> GetAllAsync(CancellationToken ct = default)
    {
        var list = await _repo.GetAllAsync(ct);
        return _mapper.Map<List<ProductReadDto>>(list);
    }

    public async Task<ProductReadDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        return entity is null ? null : _mapper.Map<ProductReadDto>(entity);
    }

    public async Task<ProductReadDto> CreateAsync(ProductCreateDto dto, CancellationToken ct = default)
    {
        var entity = _mapper.Map<Product>(dto);
        var created = await _repo.AddAsync(entity, ct);
        return _mapper.Map<ProductReadDto>(created);
    }

    public async Task<bool> UpdateAsync(int id, ProductUpdateDto dto, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null) return false;

        _mapper.Map(dto, entity);
        await _repo.UpdateAsync(entity, ct);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null) return false;

        await _repo.DeleteAsync(entity, ct);
        return true;
    }
}
