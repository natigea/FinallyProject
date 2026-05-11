using EcommersProject.BLL.DTOs;
using EcommersProject.BLL.Exceptions;
using EcommersProject.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EcommersProject.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CartsController : ControllerBase
{
    private readonly ICartService _service;
    public CartsController(ICartService service) => _service = service;

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CartGetDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await _service.GetAllAsync(ct));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CartGetDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        try { return Ok(await _service.GetByIdAsync(id, ct)); }
        catch (NotFoundException ex) { return NotFound(ex.Message); }
    }

    [HttpGet("user/{userId:guid}")]
    [ProducesResponseType(typeof(CartGetDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByUser(Guid userId, CancellationToken ct)
    {
        var cart = await _service.GetByUserAsync(userId, ct);
        return cart is null ? NotFound() : Ok(cart);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CartGetDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CartCreateDto dto, CancellationToken ct)
    {
        var created = await _service.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPost("{id:guid}/items")]
    [ProducesResponseType(typeof(CartGetDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddItem(Guid id, [FromBody] CartItemCreateDto dto, CancellationToken ct)
    {
        try { return Ok(await _service.AddItemAsync(id, dto, ct)); }
        catch (NotFoundException ex) { return NotFound(ex.Message); }
    }

    [HttpPut("items/{itemId:guid}/quantity")]
    [ProducesResponseType(typeof(CartGetDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateItemQuantity(Guid itemId, [FromBody] CartItemQuantityRequest request, CancellationToken ct)
    {
        try { return Ok(await _service.UpdateItemQuantityAsync(itemId, request.Quantity, ct)); }
        catch (NotFoundException ex) { return NotFound(ex.Message); }
    }

    [HttpDelete("items/{itemId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveItem(Guid itemId, CancellationToken ct)
    {
        try { await _service.RemoveItemAsync(itemId, ct); return NoContent(); }
        catch (NotFoundException ex) { return NotFound(ex.Message); }
    }

    [HttpDelete("{id:guid}/clear")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ClearCart(Guid id, CancellationToken ct)
    {
        try { await _service.ClearCartAsync(id, ct); return NoContent(); }
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

public record CartItemQuantityRequest(int Quantity);
