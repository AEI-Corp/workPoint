using Microsoft.AspNetCore.Mvc;
using workpoint.Application.DTOs;
using workpoint.Application.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace workpoint.Api.Controllers;

[ApiController]
[Route("api/photos")] 
public class PhotosController : ControllerBase
{
    private readonly IPhotoService _photoService;

    public PhotosController(IPhotoService photoService)
    {
        _photoService = photoService;
    }

    /// <summary>
    /// Obtiene todas las fotos.
    /// </summary>
    [HttpGet("getAll")]
    public async Task<ActionResult<List<PhotoResponseDto>>> GetAll()
    {
        var photos = await _photoService.GetAllPhotosAsync();
        
        // Devolver una lista vacía si es nula, manteniendo el tipo esperado
        return Ok(photos ?? Enumerable.Empty<PhotoResponseDto>().ToList()); 
    }

    [HttpPost("create")]
    public async Task<IActionResult> AddPhoto([FromForm] PhotoUploadFormDto photoForm)
    {
        if (!ModelState.IsValid || photoForm.Photo == null)
        {
            return BadRequest(ModelState);
        }

        // Ya no necesitas el using ni convertir a Stream
        var photoDto = new PhotoAddDto
        {
            Photo = photoForm.Photo,  // Pasar directamente el IFormFile
            SpaceId = photoForm.SpaceId
        };
    
        try
        {
            var addedPhoto = await _photoService.AddPhotoAsync(photoDto);

            if (addedPhoto == null)
            {
                return Conflict(new { message = "Se ha alcanzado la cantidad máxima de fotos permitidas para este espacio." });
            }

            return CreatedAtAction(nameof(GetAll), new { id = addedPhoto.Id }, addedPhoto);
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message }); 
        }
    }

    /// <summary>
    /// Cambia el estado (activo/inactivo) de una foto por Id.
    /// </summary>
    [HttpPut("status/{id:int}")] 
    public async Task<IActionResult> ChangeStatus(int id)
    {
        var success = await _photoService.ChangePhotoStatusAsync(id);

        if (!success)
        {
            return NotFound(new { message = $"Foto con ID {id} no encontrada." });
        }

        return Ok(new { message = "Estado de la foto cambiado exitosamente." });
    }
    
    /// <summary>
    /// Elimina una foto por Id.
    /// </summary>
    [HttpDelete("delete/{id:int}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        var success = await _photoService.RemovePhotoAsync(id);

        if (!success)
        {
            return NotFound(new { message = $"Registro con ID {id} no encontrado." });
        }

        // 204 No Content
        return NoContent(); 
    }
}