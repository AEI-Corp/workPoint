using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using workpoint.Application.Interfaces;

namespace workpoint.Api.Controllers;

[Authorize]
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
        var courses = await _spaceService.GetAllAsync();

        return Ok(courses);
    }
}