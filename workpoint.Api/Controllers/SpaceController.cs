using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using workpoint.Application.DTOs;
using workpoint.Application.Interfaces;
using workpoint.Domain.Entities;

namespace workpoint.Api.Controllers;

// [Authorize]
[ApiController]
[Route("api/space")]
public class SpaceController : ControllerBase
{
    private readonly ISpaceService _spaceService;

    public SpaceController(ISpaceService spaceService)
    {
        _spaceService = spaceService;
    }
    
    // GET ALL:
    [HttpGet("getAll")]
    public async Task<IActionResult> GetAll()
    {
        var spaces = await _spaceService.GetAllAsync();

        return Ok(spaces);
    }
    
    // GET BY ID:
    [HttpGet("getById/{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var space = await _spaceService.GetByIdAsync(id);

        if (space == null)
            return NotFound(new{ message = $"El espacio con ID {id} no existe."});
        
        return Ok(space);
    }
    
    // CREATE:
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] SpaceCreateDto spaceDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var space = new SpaceCreateDto
        {
            CategorieId = spaceDto.CategorieId,
            MaxCapacity = spaceDto.MaxCapacity,
            SpaceName = spaceDto.SpaceName,
            BranchId = spaceDto.BranchId,
            Price = spaceDto.Price,
            Description = spaceDto.Description,
            UserId = spaceDto.UserId,
            // Photos = spaceDto.Photos,
        };

        var createdSpace = await _spaceService.CreateAsync(space);

        return CreatedAtAction(nameof(GetById), new { id = createdSpace.Id }, createdSpace);
    }
    
    
    // UPDATE:
    [HttpPut("update/{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] SpaceUpdateDto spaceDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        spaceDto.Id = id;
        
        var updated = await _spaceService.UpdateAsync(spaceDto);
        
        if(!updated)
            return NotFound(new { message = $"El registro con ID {id} no existe." });
        
        return Ok(new {message = $"Espacio actualizado correctamente"});
        
    }
    
    
    
    // DELETE:
    [HttpDelete("delete/{id:int}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        var spaceToDelete = await _spaceService.DeleteAsync(id);

        if (!spaceToDelete)
            return NotFound(new { message = $"Register with ID {id} not founded." });

        return NoContent();
    }
}