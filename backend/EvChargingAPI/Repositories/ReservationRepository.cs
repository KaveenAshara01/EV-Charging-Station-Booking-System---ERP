/*
 * File: ReservationRepository.cs
 * Description: MongoDB implementation of reservation repository
 */
using EvChargingAPI.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EvChargingAPI.Repositories
{
    public class ReservationRepository : IReservationRepository
    {
        private readonly IMongoCollection<Reservation> _collection;

        // Purpose: create repository with injected IMongoDatabase and set collection
        public ReservationRepository(MongoDB.Driver.IMongoDatabase database)
        {
            _collection = database.GetCollection<Reservation>("reservations");
        }

        // Purpose: ensure indexes for faster queries and uniqueness on station/slot/time if desired
        public async Task EnsureIndexesAsync()
        {
            var keys = Builders<Reservation>.IndexKeys
                .Ascending(r => r.StationId)
                .Ascending(r => r.SlotId)
                .Ascending(r => r.ReservationTimeUtc);

            var indexModel = new CreateIndexModel<Reservation>(keys, new CreateIndexOptions { Name = "idx_station_slot_time" });
            await _collection.Indexes.CreateOneAsync(indexModel);
        }

        // Purpose: insert a new reservation into MongoDB
        public async Task CreateAsync(Reservation reservation)
        {
            await _collection.InsertOneAsync(reservation);
        }

        // Purpose: find a reservation by id
        public async Task<Reservation?> GetByIdAsync(string reservationId)
        {
            var filter = Builders<Reservation>.Filter.Eq(r => r.ReservationId, reservationId);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        // Purpose: get all reservations belonging to an owner
        public async Task<IEnumerable<Reservation>> GetByOwnerAsync(string ownerId)
        {
            var filter = Builders<Reservation>.Filter.Eq(r => r.OwnerId, ownerId);
            return await _collection.Find(filter).ToListAsync();
        }

        // Purpose: get reservations for a station in an optional period
        public async Task<IEnumerable<Reservation>> GetByStationAsync(string stationId, DateTime? from = null, DateTime? to = null)
        {
            var filter = Builders<Reservation>.Filter.Eq(r => r.StationId, stationId);
            if (from.HasValue || to.HasValue)
            {
                var rangeFilter = Builders<Reservation>.Filter.And(
                    Builders<Reservation>.Filter.Gte(r => r.ReservationTimeUtc, from ?? DateTime.MinValue),
                    Builders<Reservation>.Filter.Lte(r => r.ReservationTimeUtc, to ?? DateTime.MaxValue)
                );
                filter = Builders<Reservation>.Filter.And(filter, rangeFilter);
            }
            return await _collection.Find(filter).ToListAsync();
        }

        // Purpose: check conflict for exact slot time (used for blocking double-booking)
        public async Task<bool> ExistsConflictAsync(string stationId, string slotId, DateTime reservationTimeUtc)
        {
            var filter = Builders<Reservation>.Filter.And(
                Builders<Reservation>.Filter.Eq(r => r.StationId, stationId),
                Builders<Reservation>.Filter.Eq(r => r.SlotId, slotId),
                Builders<Reservation>.Filter.Eq(r => r.ReservationTimeUtc, reservationTimeUtc),
                Builders<Reservation>.Filter.In(r => r.Status, new[] { "Pending", "Confirmed" })
            );
            var count = await _collection.CountDocumentsAsync(filter);
            return count > 0;
        }

        // Purpose: update an existing reservation
        public async Task UpdateAsync(Reservation reservation)
        {
            var filter = Builders<Reservation>.Filter.Eq(r => r.ReservationId, reservation.ReservationId);
            await _collection.ReplaceOneAsync(filter, reservation);
        }

        // Purpose: delete by id
        public async Task DeleteAsync(string reservationId)
        {
            var filter = Builders<Reservation>.Filter.Eq(r => r.ReservationId, reservationId);
            await _collection.DeleteOneAsync(filter);
        }
    }
}
