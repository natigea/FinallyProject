using EcommersProject.BLL.DTOs;
using EcommersProject.BLL.Exceptions;
using EcommersProject.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EcommersProject.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WishlistController : ControllerBase
{
    private readonly IWishlistService _service;
    public WishlistController(IWishlistService service) => _service = service;

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<WishlistGetDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await _service.GetAllAsync(ct));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(WishlistGetDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        try { return Ok(await _service.GetByIdAsync(id, ct)); }
        catch (NotFoundException ex) { return NotFound(ex.Message); }
    }

    [HttpGet("user/{userId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<WishlistGetDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByUser(Guid userId, CancellationToken ct)
        => Ok(await _service.GetByUserAsync(userId, ct));

    [HttpGet("user/{userId:guid}/product/{productId:guid}")]
    [ProducesResponseType(typeof(WishlistGetDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByUserAndProduct(Guid userId, Guid productId, CancellationToken ct)
    {
        var item = await _service.GetByUserAndProductAsync(userId, productId, ct);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    [ProducesResponseType(typeof(WishlistGetDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] WishlistCreateDto dto, CancellationToken ct)
    {
        var created = await _service.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPost("toggle")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Toggle([FromBody] WishlistToggleRequest request, CancellationToken ct)
    {
        await _service.ToggleAsync(request.UserId, request.ProductId, ct);
        return NoContent();
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

public record WishlistToggleRequest(Guid UserId, Guid ProductId);
