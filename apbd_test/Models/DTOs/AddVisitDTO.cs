namespace apbd_test.Models.DTOs;

public class AddVisitDTO
{
    public int VisitId { get; set; }
    public int ClientId { get; set; }
    public string MechanicLicenceNumber { get; set; }
    public List<RequestVisitServiceDTO> Services { get; set; }
}