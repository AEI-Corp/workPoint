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

    /// <summary>
    /// Agrega una nueva foto para un espacio. Recibe la imagen como IFormFile.
    /// </summary>
    [HttpPost("create")]
    public async Task<IActionResult> AddPhoto([FromForm] PhotoUploadFormDto photoForm)
    {
        if (!ModelState.IsValid || photoForm.Photo == null)
        {
            return BadRequest(ModelState);
        }

        // Convertimos IFormFile a Stream para pasarlo al servicio.
        // Usamos 'using' para asegurar que el Stream se desecha correctamente.
        using var photoStream = photoForm.Photo.OpenReadStream();
        
        var photoDto = new PhotoAddDto
        {
            Photo = photoStream,
            SpaceId = photoForm.SpaceId
        };
        
        try
        {
            
            var addedPhoto = await _photoService.AddPhotoAsync(photoDto);

            if (addedPhoto == null)
            {
                // 409 Conflict: Máximo de fotos alcanzado (lógica de MaxQty)
                return Conflict(new { message = "Se ha alcanzado la cantidad máxima de fotos permitidas para este espacio." });
            }

            // 201 Created. Usamos GetAll como referencia.
            return CreatedAtAction(nameof(GetAll), new { id = addedPhoto.Id }, addedPhoto);
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            // Error en la subida a Cloudinary
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