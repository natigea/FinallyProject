using EcommersProject.BLL.DTOs;
using EcommersProject.BLL.Exceptions;
using EcommersProject.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EcommersProject.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CouponsController : ControllerBase
{
    private readonly ICouponService _service;
    public CouponsController(ICouponService service) => _service = service;

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CouponGetDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await _service.GetAllAsync(ct));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CouponGetDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        try { return Ok(await _service.GetByIdAsync(id, ct)); }
        catch (NotFoundException ex) { return NotFound(ex.Message); }
    }

    [HttpGet("by-code/{code}")]
    [ProducesResponseType(typeof(CouponGetDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCode(string code, CancellationToken ct)
    {
        var coupon = await _service.GetByCodeAsync(code, ct);
        return coupon is null ? NotFound() : Ok(coupon);
    }

    [HttpGet("validate/{code}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<IActionResult> Validate(string code, CancellationToken ct)
        => Ok(await _service.ValidateAsync(code, ct));

    [HttpPost]
    [ProducesResponseType(typeof(CouponGetDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CouponCreateDto dto, CancellationToken ct)
    {
        var created = await _service.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CouponGetDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] CouponUpdateDto dto, CancellationToken ct)
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
