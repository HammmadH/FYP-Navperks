using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data.SqlClient;
using FYP_Navperks.Models.Announcements;
using FYP_Navperks.Models.Parking;
using FYP_Navperks.Models.Reservations;
using FYP_Navperks.Models.Statistics;

namespace FYP_Navperks.Models.Database
{
    public class DbHelper
    {
        private readonly string _connectionString;

        public DbHelper(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        //Admin
        public async Task<bool> ValidateCredentials(string username, string password)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT COUNT(*) FROM AdminTable WHERE Username = @Username AND Password = @Password";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Username", username);
                    cmd.Parameters.AddWithValue("@Password", password);

                    await conn.OpenAsync();
                    int count = (int)await cmd.ExecuteScalarAsync();
                    return count > 0;
                }
            }
        }

        public async Task UpdatePassword(string username, string newPassword)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("UpdateAdminPassword", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Username", username);
                    cmd.Parameters.AddWithValue("@NewPassword", newPassword);

                    await conn.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<bool> AddAUser(string carnumber)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("InsertUserA", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@CarNumber", carnumber);

                    var result = await command.ExecuteNonQueryAsync();
                    return result > 0;
                }
            }
        }

        public async Task<int?> GetUserByCarNumber(string carnumber)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("SELECT UserAId FROM UsersA WHERE CarNumber = @CarNumber", connection))
                {
                    command.Parameters.AddWithValue("@CarNumber", carnumber);

                    var result = await command.ExecuteScalarAsync();
                    return result != null ? Convert.ToInt32(result) : null;
                }
            }
        }

        //---------------------------------------------------------------------------//

        //User

        public async Task<string> AddUser(string ipAddress)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("InsertUser", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@IPAddress", ipAddress);

                    var statusParam = new SqlParameter("@StatusMessage", SqlDbType.NVarChar, 100)
                    {
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add(statusParam);

                    await command.ExecuteNonQueryAsync();

                    return statusParam.Value.ToString();
                }
            }
        }

        public async Task<int?> GetUserByIPAddress(string ipAddress)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("SELECT UserId FROM Users WHERE IPAddress = @IPAddress", connection))
                {
                    command.Parameters.AddWithValue("@IPAddress", ipAddress);

                    var result = await command.ExecuteScalarAsync();
                    return result != null ? Convert.ToInt32(result) : null;
                }
            }
        }

        //---------------------------------------------------------------------------//

        //Parking

        public async Task<bool> AddParkingSlots(List<ParkingSlotDTO> parkingSlots)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                foreach (var slot in parkingSlots)
                {
                    using (var command = new SqlCommand("InsertParkingSlots", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@SlotCode", slot.SlotCode);
                        command.Parameters.AddWithValue("@IsFree", slot.IsFree);

                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
            return true;
        }

        public async Task<List<ParkingSlotDTO>> GetParkingSlotsAsync()
        {
            var parkingSlots = new List<ParkingSlotDTO>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand("SELECT SlotId, SlotCode, Is_Free FROM Parkingslotstable", connection);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        parkingSlots.Add(new ParkingSlotDTO
                        {
                            SlotId = reader.GetInt32(0),
                            SlotCode = reader.GetString(1),
                            IsFree = reader.GetBoolean(2)
                        });
                    }
                }
            }

            return parkingSlots;
        }

        //---------------------------------------------------------------------------//

        //Reservation

        public async Task<int> ReserveSlot(int userId, string slotCode, string carType, DateTime reservedTime)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                int slotId = await GetSlotIdByCode(slotCode);

                if (slotId == 0)
                    return 0;

                using (var command = new SqlCommand("InsertReservation", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@UserId", userId);
                    command.Parameters.AddWithValue("@SlotId", slotId);
                    command.Parameters.AddWithValue("@CarType", carType);
                    command.Parameters.AddWithValue("@ReservedTime", reservedTime);

                    var result = await command.ExecuteScalarAsync(); 
                    return result != null ? Convert.ToInt32(result) : 0; 
                }
            }
        }

        public async Task<int?> GetReservationByUserID(int userId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("SELECT ReservationId FROM Reservedslottable WHERE UserId = @UserId  AND ReleasedTime IS NULL", connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);

                    var result = await command.ExecuteScalarAsync();

                    return result != null ? Convert.ToInt32(result) : null;
                }
            }
        }

        public async Task<List<Reservation>> GetReservationsByUserID(int userId)
        {
            var reservations = new List<Reservation>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand(
                    "SELECT ReservationId, ReleasedTime FROM Reservedslottable WHERE UserId = @UserId", connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            reservations.Add(new Reservation
                            {
                                ReservationId = reader.GetInt32(0),
                                ReleasedTime = reader.IsDBNull(1) ? null : reader.GetDateTime(1)
                            });
                        }
                    }
                }
            }

            return reservations;
        }

        public async Task<DateTime?> GetReleasedTimeByUserID(int userId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("SELECT ReleasedTime FROM Reservedslottable WHERE UserId = @UserId", connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);

                    var result = await command.ExecuteScalarAsync();
                    return result != DBNull.Value ? Convert.ToDateTime(result) : null;
                }
            }
        }

        public async Task<int> ReserveASlot(int userAId, string slotCode, string CarType, DateTime reservedTime)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                int slotId = await GetSlotIdByCode(slotCode);

                if (slotId == 0)
                    return 0; 

                using (var command = new SqlCommand("InsertAReservation", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@UserAId", userAId);
                    command.Parameters.AddWithValue("@SlotId", slotId);
                    command.Parameters.AddWithValue("@CarType", CarType);
                    command.Parameters.AddWithValue("@ReservedTime", reservedTime);

                    var result = await command.ExecuteScalarAsync(); // Use ExecuteScalarAsync to retrieve a single value (ReservationId)
                    return result != null ? Convert.ToInt32(result) : 0; // Return the ReservationId or 0 if the insertion failed
                }
            }
        }

        public async Task<int?> GetReservationByUserAID(int userAId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("SELECT ReservationId FROM Reservedslottable WHERE UserAId = @UserAId", connection))
                {
                    command.Parameters.AddWithValue("@UserAId", userAId);

                    var result = await command.ExecuteScalarAsync();
                    return result != null ? Convert.ToInt32(result) : null;
                }
            }
        }

        public async Task<int> GetSlotIdByCode(string slotCode)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("SELECT SlotId FROM Parkingslotstable WHERE SlotCode = @SlotCode", connection))
                {
                    command.Parameters.AddWithValue("@SlotCode", slotCode);

                    var result = await command.ExecuteScalarAsync();
                    return result != DBNull.Value ? Convert.ToInt32(result) : 0;
                }
            }
        }

        public async Task<bool> ReleaseSlot(int reservationId, string slotCode, DateTime releasedTime)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("UpdateReleasedTime", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@ReservationId", reservationId);
                    command.Parameters.AddWithValue("@SlotCode", slotCode);
                    command.Parameters.AddWithValue("@ReleasedTime", releasedTime);

                    var returnParameter = new SqlParameter("@ReturnVal", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.ReturnValue
                    };
                    command.Parameters.Add(returnParameter);

                    await command.ExecuteNonQueryAsync();

                    int result = (int)returnParameter.Value;
                    return result == 1;
                }
            }
        }


        public async Task<List<ReservationDetails>> GetReservationsWithSlotCode()
        {
            var reservations = new List<ReservationDetails>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("GetReservationsWithSlotCode", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            reservations.Add(new ReservationDetails
                            {
                                ReservationId = reader.GetInt32(reader.GetOrdinal("ReservationId")),
                                SlotCode = reader.GetString(reader.GetOrdinal("SlotCode")),
                                CarType = reader.GetString(reader.GetOrdinal("CarType")),
                                CarSpeed = reader.IsDBNull(reader.GetOrdinal("CarSpeed")) ? null : (float)reader.GetDouble(reader.GetOrdinal("CarSpeed")),
                                ReservedTime = reader.GetDateTime(reader.GetOrdinal("ReservedTime")),
                                ReleasedTime = reader.IsDBNull(reader.GetOrdinal("ReleasedTime")) ? null : reader.GetDateTime(reader.GetOrdinal("ReleasedTime"))
                            });
                        }
                    }
                }
            }

            return reservations;
        }

        //---------------------------------------------------------------------------//

        //Statistics

        public async Task<bool> NoteCarSpeed(int reservationId, float carSpeed)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("UpdateCarSpeed", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@ReservationId", reservationId);
                    command.Parameters.AddWithValue("@CarSpeed", carSpeed);

                    var result = await command.ExecuteNonQueryAsync();
                    return result > 0;
                }
            }
        }

        public async Task<List<RushStatistic>> GetRushStatistic()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("CalculateRushStatistics", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        var results = new List<RushStatistic>();
                        while (await reader.ReadAsync())
                        {
                            results.Add(new RushStatistic
                            {
                                StatisticType = reader.GetString(reader.GetOrdinal("StatisticType")),
                                Value = reader["Value"].ToString(),
                                ReservationCount = reader.GetInt32(reader.GetOrdinal("ReservationCount"))
                            });
                        }
                        return results;
                    }
                }
            }
        }

        //---------------------------------------------------------------------------//

        //Announcements

        public async Task<List<Announcement>> GetAnnouncementsAsync()
        {
            var announcements = new List<Announcement>();

            using (var connection = new SqlConnection(_connectionString))
            {
                const string query = "SELECT Id, AnnouncementText FROM Announcements";
                var command = new SqlCommand(query, connection);

                await connection.OpenAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        announcements.Add(new Announcement
                        {
                            Id = reader.GetInt32(0),
                            AnnouncementText = reader.GetString(1)
                        });
                    }
                }
            }

            return announcements;
        }

        public async Task<int> AddAnnouncementAsync(string announcementText)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                const string query = "INSERT INTO Announcements (AnnouncementText) OUTPUT INSERTED.Id VALUES (@AnnouncementText)";
                var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@AnnouncementText", announcementText);

                await connection.OpenAsync();
                var newId = (int)await command.ExecuteScalarAsync();
                return newId;
            }
        }

        public async Task<bool> UpdateAnnouncementAsync(int id, string announcementText)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                const string query = "UPDATE Announcements SET AnnouncementText = @AnnouncementText WHERE Id = @Id";
                var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@AnnouncementText", announcementText);
                command.Parameters.AddWithValue("@Id", id);

                await connection.OpenAsync();
                var rowsAffected = await command.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
        }

        public async Task<bool> DeleteAnnouncementAsync(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                const string query = "DELETE FROM Announcements WHERE Id = @Id";
                var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Id", id);

                await connection.OpenAsync();
                var rowsAffected = await command.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
        }

        //---------------------------------------------------------------------------//
    }
}
