using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http; // IFormFile
namespace workpoint.Application.DTOs;

public class UploadPhotoDto
{
    public IFormFile Photo { get; set; }
    public int? SpaceId { get; set; }
}

public class PhotoUploadFormDto
{
    [Required(ErrorMessage = "El archivo de la foto es requerido.")]
    public IFormFile Photo { get; set; }
    
    // We use the same name 'SpaceId' than in DTO for the service for consistence.
    public int? SpaceId { get; set; }
}