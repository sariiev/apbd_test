namespace apbd_test.Models.DTOs;

public class VisitDTO
{
    public DateTime Date { get; set; }
    public ClientDTO Client { get; set; }
    public MechanicDTO Mechanic { get; set; }
    public List<VisitServiceDTO> Services { get; set; }
}