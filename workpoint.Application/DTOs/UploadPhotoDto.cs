using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http; // IFormFile
namespace workpoint.Application.DTOs;

public class UploadPhotoDto
{
    public Stream Photo { get; set; }
    public int? SpaceId { get; set; }
}

public class PhotoUploadFormDto
{
    [Required(ErrorMessage = "El archivo de la foto es requerido.")]
    public IFormFile Photo { get; set; }
    
    // Usamos el mismo nombre 'SpaceId' que en el DTO del servicio para consistencia
    public int? SpaceId { get; set; }
}