using EcommersProject.BLL.DTOs;
using EcommersProject.BLL.Exceptions;
using EcommersProject.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EcommersProject.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AddressesController : ControllerBase
{
    private readonly IAddressService _service;
    public AddressesController(IAddressService service) => _service = service;

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AddressGetDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await _service.GetAllAsync(ct));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AddressGetDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        try { return Ok(await _service.GetByIdAsync(id, ct)); }
        catch (NotFoundException ex) { return NotFound(ex.Message); }
    }

    [HttpGet("user/{userId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<AddressGetDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByUser(Guid userId, CancellationToken ct)
        => Ok(await _service.GetByUserAsync(userId, ct));

    [HttpPost]
    [ProducesResponseType(typeof(AddressGetDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] AddressCreateDto dto, CancellationToken ct)
    {
        var created = await _service.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(AddressGetDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] AddressUpdateDto dto, CancellationToken ct)
    {
        try { return Ok(await _service.UpdateAsync(id, dto, ct)); }
        catch (NotFoundException ex) { return NotFound(ex.Message); }
    }

    [HttpPatch("{id:guid}/set-default")]
    [ProducesResponseType(typeof(AddressGetDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetDefault(Guid id, [FromQuery] Guid userId, CancellationToken ct)
    {
        try { return Ok(await _service.SetDefaultAsync(id, userId, ct)); }
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
