/*
 * File: SlotRepository.cs
 * Description: MongoDB implementation of slot repository
 */
using EvChargingAPI.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EvChargingAPI.Repositories
{
    public class SlotRepository : ISlotRepository
    {
        private readonly IMongoCollection<Slot> _collection;

        public SlotRepository(IMongoDatabase database)
        {
            _collection = database.GetCollection<Slot>("slots");
        }

        public async Task EnsureIndexesAsync()
        {
            var keys = Builders<Slot>.IndexKeys
                .Ascending(s => s.StationId)
                .Ascending(s => s.StartUtc)
                .Ascending(s => s.EndUtc);

            var indexModel = new CreateIndexModel<Slot>(keys, new CreateIndexOptions { Name = "idx_station_time" });
            await _collection.Indexes.CreateOneAsync(indexModel);
        }

        public async Task CreateAsync(Slot slot) => await _collection.InsertOneAsync(slot);

        public async Task<Slot?> GetByIdAsync(string slotId)
        {
            var filter = Builders<Slot>.Filter.Eq(s => s.SlotId, slotId);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Slot>> GetByStationAndWindowAsync(string stationId, DateTime fromUtc, DateTime toUtc)
        {
            var filter = Builders<Slot>.Filter.And(
                Builders<Slot>.Filter.Eq(s => s.StationId, stationId),
                Builders<Slot>.Filter.Lte(s => s.StartUtc, toUtc),
                Builders<Slot>.Filter.Gte(s => s.EndUtc, fromUtc)
            );
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<IEnumerable<Slot>> GetAvailableByStationAndWindowAsync(string stationId, DateTime fromUtc, DateTime toUtc)
        {
            var filter = Builders<Slot>.Filter.And(
                Builders<Slot>.Filter.Eq(s => s.StationId, stationId),
                Builders<Slot>.Filter.Eq(s => s.IsAvailable, true),
                Builders<Slot>.Filter.Lte(s => s.StartUtc, toUtc),
                Builders<Slot>.Filter.Gte(s => s.EndUtc, fromUtc)
            );
            return await _collection.Find(filter).ToListAsync();
        }

        // Check if a new slot would overlap existing slot on same station (any overlap).
        public async Task<bool> ExistsOverlapAsync(string stationId, DateTime startUtc, DateTime endUtc)
        {
            // Overlap if start < existing.end && end > existing.start
            var filter = Builders<Slot>.Filter.And(
                Builders<Slot>.Filter.Eq(s => s.StationId, stationId),
                Builders<Slot>.Filter.And(
                    Builders<Slot>.Filter.Lt(s => s.StartUtc, endUtc),
                    Builders<Slot>.Filter.Gt(s => s.EndUtc, startUtc)
                )
            );
            var count = await _collection.CountDocumentsAsync(filter);
            return count > 0;
        }

        public async Task UpdateAsync(Slot slot)
        {
            var filter = Builders<Slot>.Filter.Eq(s => s.SlotId, slot.SlotId);
            await _collection.ReplaceOneAsync(filter, slot);
        }

        public async Task DeleteAsync(string slotId)
        {
            var filter = Builders<Slot>.Filter.Eq(s => s.SlotId, slotId);
            await _collection.DeleteOneAsync(filter);
        }

            public async Task<IEnumerable<Slot>> GetAllAsync()
    {
        return await _collection.Find(Builders<Slot>.Filter.Empty).ToListAsync();
    }
    }
}
