using apbd_test.Models.DTOs;

namespace apbd_test.Services;

public interface IVisitService
{
    Task<VisitDTO> GetVisitById(int id);

    Task<VisitDTO> AddVisit(AddVisitDTO addVisitDto);
}