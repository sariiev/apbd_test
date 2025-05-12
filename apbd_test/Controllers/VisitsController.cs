using apbd_test.Exceptions;
using apbd_test.Models.DTOs;
using apbd_test.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace apbd_test.Controllers;

[Route("api/[controller]")]
[ApiController]
public class VisitsController : Controller
{
    private readonly IVisitService _visitService;

    public VisitsController(IVisitService visitService)
    {
        _visitService = visitService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetVisitById(int id)
    {
        try
        {
            var result = await _visitService.GetVisitById(id);
            return Ok(result);
        }
        catch (ClientNotFoundException)
        {
            return NotFound("Client not found");
        }
        catch (MechanicNotFoundException)
        {
            return NotFound("Mechanic not found");
        }
        catch (VisitNotFoundException)
        {
            return NotFound("Visit not found");
        }
    }

    [HttpPost]
    public async Task<IActionResult> AddVisit(AddVisitDTO addVisitDto)
    {
        try
        {
            var result = await _visitService.AddVisit(addVisitDto);
            return Ok(addVisitDto);
        }
        catch (ClientNotFoundException)
        {
            return NotFound("Client not found");
        }
        catch (MechanicNotFoundException)
        {
            return NotFound("Mechanic not found");
        }
        catch (VisitAlreadyExistsException)
        {
            return Conflict("Visit already exists");
        }
        catch (ServiceError)
        {
            return new ObjectResult("Service error")
            {
                StatusCode = StatusCodes.Status500InternalServerError
            }
        }
    }
}