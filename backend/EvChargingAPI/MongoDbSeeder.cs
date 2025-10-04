using MongoDB.Driver;
using Domain.Entities;
using System.Collections.Generic;

namespace EvChargingAPI
{
    public class MongoDbSeeder
    {
        private readonly IMongoDatabase _database;

        public MongoDbSeeder(string connectionString, string databaseName)
        {
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);
        }

        public void SeedOwners()
        {
            var owners = _database.GetCollection<Owner>("owners");
            var seedData = new List<Owner>
            {
                new Owner { Id = "EO100", NIC = "123456789V", Name = "Supun Silva", Email = "supun@example.com", Phone = "0771234567", PasswordHash = "hash1", Roles = new[] { "EVOwner" }, IsActive = true },
                new Owner { Id = "EO101", NIC = "987654321V", Name = "Kasun Perera", Email = "kasun@example.com", Phone = "0779876543", PasswordHash = "hash2", Roles = new[] { "EVOwner" }, IsActive = true },
                new Owner { Id = "EO102", NIC = "957654323V", Name = "Saman Silva", Email = "saman@example.com", Phone = "0779876544", PasswordHash = "hash3", Roles = new[] { "EVOwner" }, IsActive = true }
            };
            owners.InsertMany(seedData);
        }

        public void SeedBooking()
        {
            var bookings = _database.GetCollection<Booking>("bookings");
            var seedData = new List<Booking>
            {
                new Booking { Id = Guid.NewGuid().ToString(), StationId = "ST001", StartUtc = new DateTime(2025, 10, 4, 6, 30, 0, DateTimeKind.Utc), EndUtc = new DateTime(2025, 10, 4, 8, 0, 0, DateTimeKind.Utc), Status = "Completed" },
                new Booking { Id = Guid.NewGuid().ToString(), StationId = "ST002", StartUtc = new DateTime(2025, 10, 5, 9, 0, 0, DateTimeKind.Utc), EndUtc = new DateTime(2025, 10, 5, 11, 30, 0, DateTimeKind.Utc), Status = "Ongoing" },
                new Booking { Id = Guid.NewGuid().ToString(), StationId = "ST003", StartUtc = new DateTime(2025, 10, 6, 14, 0, 0, DateTimeKind.Utc), EndUtc = new DateTime(2025, 10, 6, 16, 15, 0, DateTimeKind.Utc), Status = "Cancelled" }
            };
            bookings.InsertMany(seedData);
        }

        public void SeedStations()
        {
            var stations = _database.GetCollection<Station>("stations");
            var seedData = new List<Station>
            {
                new Station { Id = "ST001", Name = "Colombo SuperCharge Station", IsActive = true },
                new Station { Id = "ST002", Name = "Kandy City EV Hub", IsActive = true },
                new Station { Id = "ST003", Name = "Galle EcoCharge Point", IsActive = false },
                new Station { Id = "ST004", Name = "Negombo FastCharge Center", IsActive = true },
                new Station { Id = "ST005", Name = "Jaffna QuickCharge Station", IsActive = true }
            };
            stations.InsertMany(seedData);
        }

        public void SeedAll()
        {
            SeedOwners();
            SeedStations();
            SeedBooking();
        }
        // Add similar methods for other collections if needed
    }
}
