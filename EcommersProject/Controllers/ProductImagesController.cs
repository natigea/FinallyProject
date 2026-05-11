using EcommersProject.BLL.DTOs;
using EcommersProject.BLL.Exceptions;
using EcommersProject.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EcommersProject.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductImagesController : ControllerBase
{
    private readonly IProductImageService _service;
    public ProductImagesController(IProductImageService service) => _service = service;

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ProductImageGetDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await _service.GetAllAsync(ct));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductImageGetDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        try { return Ok(await _service.GetByIdAsync(id, ct)); }
        catch (NotFoundException ex) { return NotFound(ex.Message); }
    }

    [HttpGet("product/{productId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<ProductImageGetDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByProduct(Guid productId, CancellationToken ct)
        => Ok(await _service.GetByProductAsync(productId, ct));

    [HttpPost]
    [ProducesResponseType(typeof(ProductImageGetDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] ProductImageCreateDto dto, CancellationToken ct)
    {
        var created = await _service.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ProductImageGetDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] ProductImageUpdateDto dto, CancellationToken ct)
    {
        try { return Ok(await _service.UpdateAsync(id, dto, ct)); }
        catch (NotFoundException ex) { return NotFound(ex.Message); }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        try { await _service.DeleteAsync(id, ct); return NoContent(); }
        catch (NotFoundException ex) { return NotFound(ex.Message); }
    }
}
