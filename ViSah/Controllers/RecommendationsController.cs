using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ViSah.Services;
using ViSah.Interfaces;

namespace ViSah.Controllers;

[ApiController, Authorize, Route("api/recommendations")]
public class RecommendationsController(ICatalogService catalog) : ControllerBase
{
    [HttpGet]
    public Task<object> Get([FromQuery] int moodId, [FromQuery] int? genreId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return catalog.Recommend(moodId, genreId, userId);
    }
}