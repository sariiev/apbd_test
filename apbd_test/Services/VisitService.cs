using apbd_test.Exceptions;
using apbd_test.Models;
using apbd_test.Models.DTOs;
using Microsoft.Data.SqlClient;

namespace apbd_test.Services;

public class VisitService : IVisitService
{
    private readonly string _connectionString;

    public VisitService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Default") ?? throw new ArgumentException("Connecting string not found");
    }

    public async Task<VisitDTO> GetVisitById(int id)
    {
        String sql1 = "SELECT * FROM Visit WHERE visit_id = @id";
        String sql2 = "SELECT * FROM Client WHERE client_id = @id";
        String sql3 = "SELECT * FROM Mechanic WHERE mechanic_id = @id";
        String sql4 = "SELECT * FROM Visit_Service WHERE visit_id = @id";
        String sql5 = "SELECT * FROM Service WHERE service_id = @id";

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            
            int visitId;
            int clientId;
            int mechanicId;
            DateTime visitDate;
            
            using (SqlCommand command = new SqlCommand(sql1, connection))
            {
                command.Parameters.AddWithValue("@id", id);

                var reader = await command.ExecuteReaderAsync();
                
                if (await reader.ReadAsync())
                {
                    visitId = reader.GetInt32(reader.GetOrdinal("visit_id"));
                    clientId = reader.GetInt32(reader.GetOrdinal("client_id"));
                    mechanicId = reader.GetInt32(reader.GetOrdinal("mechanic_id"));
                    visitDate = reader.GetDateTime(reader.GetOrdinal("date"));
                }
                else
                {
                    throw new VisitNotFoundException();
                }

                await reader.CloseAsync();
            }

            string firstName;
            string lastName;
            DateTime dateOfBirth;
            using (SqlCommand command = new SqlCommand(sql2, connection))
            {
                command.Parameters.AddWithValue("@id", clientId);
                
                var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    // client_id = reader.GetInt32(reader.GetOrdinal("client_id"));
                    firstName = reader.GetString(reader.GetOrdinal("first_name"));
                    lastName = reader.GetString(reader.GetOrdinal("last_name"));
                    dateOfBirth = reader.GetDateTime(reader.GetOrdinal("date_of_birth"));
                }
                else
                {
                    throw new ClientNotFoundException();
                }
                
                await reader.CloseAsync();
            }
            
            string licenceNumber;
            using (SqlCommand command = new SqlCommand(sql3, connection))
            {
                command.Parameters.AddWithValue("@id", mechanicId);
                
                var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    licenceNumber = reader.GetString(reader.GetOrdinal("licence_number"));
                }
                else
                {
                    throw new MechanicNotFoundException();
                }
                
                await reader.CloseAsync();
            }

            int serviceId;
            List<int> serviceIds = new List<int>();
            using (SqlCommand command = new SqlCommand(sql4, connection))
            {
                command.Parameters.AddWithValue("@id", id);
                
                var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    serviceId = reader.GetInt32(reader.GetOrdinal("service_id"));
                    serviceIds.Add(serviceId);
                }
                
                await reader.CloseAsync();
            }
            
            string serviceName;
            double baseFee;
            List<VisitServiceDTO> serviceDtos = new List<VisitServiceDTO>();
            using (SqlCommand command = new SqlCommand(sql5, connection))
            {
                foreach (int _serviceId in serviceIds)
                {
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("id", _serviceId);
                    
                    var reader = await command.ExecuteReaderAsync();
            
                    if (await reader.ReadAsync())
                    {
                        serviceName = reader.GetString(reader.GetOrdinal("name"));
                        // baseFee = reader.GetDouble(reader.GetOrdinal("base_fee"));
                        baseFee = Convert.ToDouble(reader.GetDecimal(reader.GetOrdinal("base_fee")));
                        
                        serviceDtos.Add(new VisitServiceDTO()
                        {
                            Name = serviceName,
                            ServiceFee = baseFee
                        });
                    }
            
                    await reader.CloseAsync();
                }
            }

            return new VisitDTO()
            {
                Client = new ClientDTO()
                {
                    FirstName = firstName,
                    LastName = lastName,
                    DateOfBirth = dateOfBirth
                },
                Date = visitDate,
                Mechanic = new MechanicDTO()
                {
                    Id = mechanicId,
                    LicenceNumber = licenceNumber
                },
                Services = serviceDtos
            };
        }
    }

    public async Task<VisitDTO> AddVisit(AddVisitDTO addVisitDto)
    {
        // select
        string sql1 = "SELECT COUNT(*) FROM Visit WHERE visit_id = @id";
        string sql2 = "SELECT * FROM Client WHERE client_id = @id";
        string sql3 = "SELECT * FROM Mechanic WHERE licence_number = @licence_number";
        string sql4 = "SELECT * FROM Service WHERE name = @serviceName";
        
        // insert
        string sql5 = @"INSERT INTO Visit (visit_id, client_id, mechanic_id, date) 
                        VALUES (@visitId, @clientId, @mechanicId, @date);
                        SELECT SCOPE_IDENTITY();";
        string sql6 = @"INSERT INTO Visit_Service (visit_id, service_id, service_fee)
                        VALUES (@visitId, @serviceId, @serviceFee)";
        
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            using (SqlCommand command = new SqlCommand(sql1, connection))
            {
                command.Parameters.AddWithValue("@id", addVisitDto.VisitId);

                int count = (int)await command.ExecuteScalarAsync();

                if (count > 0)
                {
                    throw new VisitAlreadyExistsException();
                }
            }
            
            string firstName;
            string lastName;
            DateTime dateOfBirth;
            using (SqlCommand command = new SqlCommand(sql2, connection))
            {
                command.Parameters.AddWithValue("@id", addVisitDto.ClientId);
                
                var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    // client_id = reader.GetInt32(reader.GetOrdinal("client_id"));
                    firstName = reader.GetString(reader.GetOrdinal("first_name"));
                    lastName = reader.GetString(reader.GetOrdinal("last_name"));
                    dateOfBirth = reader.GetDateTime(reader.GetOrdinal("date_of_birth"));
                }
                else
                {
                    throw new ClientNotFoundException();
                }
                
                await reader.CloseAsync();
            }
            
            int mechanicId;
            using (SqlCommand command = new SqlCommand(sql3, connection))
            {
                command.Parameters.AddWithValue("@licence_number", addVisitDto.MechanicLicenceNumber);
                
                var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    mechanicId = reader.GetInt32(reader.GetOrdinal("mechanic_id"));
                }
                else
                {
                    throw new MechanicNotFoundException();
                }
                
                await reader.CloseAsync();
            }
            
            List<int> serviceIds = new List<int>();
            List<double> serviceFees = new List<double>();
            using (SqlCommand command = new SqlCommand(sql4, connection))
            {
                foreach (RequestVisitServiceDTO service in addVisitDto.Services)
                {
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("serviceName", service.ServiceName);
                    
                    var reader = await command.ExecuteReaderAsync();
            
                    if (await reader.ReadAsync())
                    {
                        int serviceId = reader.GetInt32(reader.GetOrdinal("service_id"));
                        double baseFee = Convert.ToDouble(reader.GetDecimal(reader.GetOrdinal("base_fee")));
                        
                        serviceIds.Add(serviceId);
                        serviceFees.Add(baseFee);
                    }
            
                    await reader.CloseAsync();
                }
            }

            SqlTransaction transaction = connection.BeginTransaction();

            try
            {
                using (SqlCommand command = new SqlCommand(sql5, connection, transaction))
                {
                    command.Parameters.AddWithValue("@visitId", addVisitDto.VisitId);
                    command.Parameters.AddWithValue("@clientId", addVisitDto.ClientId);
                    command.Parameters.AddWithValue("@mechanicId", mechanicId);
                    command.Parameters.AddWithValue("@date", DateTime.Now);

                    var result = await command.ExecuteScalarAsync();
                    int createdVisitId = Convert.ToInt32(result);
                }

                using (SqlCommand command = new SqlCommand(sql6, connection, transaction))
                {
                    for (int i = 0; i < serviceIds.Count; i++)
                    {
                        command.Parameters.Clear();

                        int serviceId = serviceIds[i];
                        double serviceFee = serviceFees[i];

                        command.Parameters.AddWithValue("@visitId", addVisitDto.VisitId);
                        command.Parameters.AddWithValue("@serviceId", serviceId);
                        command.Parameters.AddWithValue("@serviceFee", serviceFee);

                        var result = await command.ExecuteScalarAsync();
                        int createdVisitServiceId = Convert.ToInt32(result);
                    }
                }
                
                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw ex;
            }

            List<VisitServiceDTO> services = new List<VisitServiceDTO>();
            foreach (RequestVisitServiceDTO serviceDto in addVisitDto.Services)
            {
                services.Add(new VisitServiceDTO()
                {
                    Name = serviceDto.ServiceName,
                    ServiceFee = serviceDto.ServiceFee
                });
            }
            
            return new VisitDTO()
            {
                Client = new ClientDTO()
                {
                    FirstName = firstName,
                    LastName = lastName,
                    DateOfBirth = dateOfBirth
                },
                Date = DateTime.Now,
                Mechanic = new MechanicDTO()
                {
                    Id = mechanicId,
                    LicenceNumber = addVisitDto.MechanicLicenceNumber
                },
                Services = services
            };
        }
    }
}