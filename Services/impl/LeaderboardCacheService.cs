using Microsoft.EntityFrameworkCore;
using Real_time_Leaderboard.DTOs;
using Real_time_Leaderboard.Models;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Real_time_Leaderboard.Services.impl
{
    public static class LeaderboardKeys
    {
        public static string LeaderboardAllTime(int leaderboardId) => $"leaderboard:{leaderboardId}:alltime";
        public static string GameAllTime(int gameId) => $"game:{gameId}:alltime";
    }

    public class LeaderboardCacheService : ILeaderboardCacheService
    {
        private readonly IDatabase _db;

        private readonly AppDbContext _context;

        public LeaderboardCacheService(IConnectionMultiplexer redis, AppDbContext context)
        {
            _db = redis.GetDatabase();
            _context = context;
        }

        private async Task<int?> GetPlayerIdUsingAccountIdAsync(int accountId)
        {
            return await _context.Players
                .Where(a => a.AccountId == accountId)
                .Select(a => (int?)a.PlayerId)
                .FirstOrDefaultAsync();
        }

        private async Task<int> GetGameIdByLeaderboardIdAsync(int leaderboardId)
        {
            var gameId = await _context.Leaderboards
                .AsNoTracking()
                .Where(l => l.LeaderboardId == leaderboardId)
                .Select(l => (int?)l.GameId)
                .FirstOrDefaultAsync();

            if (!gameId.HasValue)
                throw new InvalidOperationException($"Leaderboard with ID {leaderboardId} not found.");

            return gameId.Value;
        }

        // 1. ZADD: Overwrites or inserts a player's absolute score
        public async Task UpdateScoreAsync(int leaderboardId, int accountId, double score)
        {
            var playerId = await GetPlayerIdUsingAccountIdAsync(accountId);
            if (!playerId.HasValue)
                throw new InvalidOperationException($"No player found for account ID {accountId}");

            var gameId = await GetGameIdByLeaderboardIdAsync(leaderboardId);

            var leaderboardTask = _db.SortedSetAddAsync(LeaderboardKeys.LeaderboardAllTime(leaderboardId), playerId.Value, score);
            var gameTask = _db.SortedSetAddAsync(LeaderboardKeys.GameAllTime(gameId), playerId.Value, score);

            await Task.WhenAll(leaderboardTask, gameTask);
        }

        // 2. ZINCRBY: Adds points to an existing score 
        public async Task IncrementScoreAsync(int leaderboardId, int accountId, double pointsToAdd)
        {
            var playerId = await GetPlayerIdUsingAccountIdAsync(accountId);
            if (!playerId.HasValue)
                throw new InvalidOperationException($"No player found for account ID {accountId}");

            var gameId = await GetGameIdByLeaderboardIdAsync(leaderboardId);

            var leaderboardTask = _db.SortedSetIncrementAsync(LeaderboardKeys.LeaderboardAllTime(leaderboardId), playerId.Value, pointsToAdd);
            var gameTask = _db.SortedSetIncrementAsync(LeaderboardKeys.GameAllTime(gameId), playerId.Value, pointsToAdd);

            await Task.WhenAll(leaderboardTask, gameTask);
        }

        // 3. ZREVRANGE: Gets the top N players
        public async Task<List<KeyValuePair<int, double>>> GetTopPlayersAsync(int leaderboardId, int topN)
        {
            var entries = await _db.SortedSetRangeByRankWithScoresAsync(LeaderboardKeys.LeaderboardAllTime(leaderboardId), 0, topN - 1, Order.Descending);

            // We explicitly cast the RedisValue back to an int using (int)
            return entries.Select(e => new KeyValuePair<int, double>((int)e.Element, e.Score)).ToList();
        }

        // 4. ZREVRANK: Gets a player's exact rank 
        public async Task<long?> GetPlayerRankAsync(int leaderboardId, int accountId)
        {
            var playerId = await GetPlayerIdUsingAccountIdAsync(accountId);
            if (!playerId.HasValue)
                return null;

            var rank = await _db.SortedSetRankAsync(LeaderboardKeys.LeaderboardAllTime(leaderboardId), playerId.Value, Order.Descending);

            return rank.HasValue ? rank.Value + 1 : null;
        }

        // 5. ZSCORE: Gets just the player's current score
        public async Task<double?> GetPlayerScoreAsync(int leaderboardId, int accountId)
        {
            var playerId = await GetPlayerIdUsingAccountIdAsync(accountId);
            if (!playerId.HasValue)
                return null;

            return await _db.SortedSetScoreAsync(LeaderboardKeys.LeaderboardAllTime(leaderboardId), playerId.Value);
        }

        // 6. ZREM: Removes a player from the leaderboard entirely
        public async Task<bool> RemovePlayerAsync(int leaderboardId, int accountId)
        {
            var playerId = await GetPlayerIdUsingAccountIdAsync(accountId);
            if (!playerId.HasValue)
                return false;

            var gameId = await GetGameIdByLeaderboardIdAsync(leaderboardId);

            var leaderboardTask = _db.SortedSetRemoveAsync(LeaderboardKeys.LeaderboardAllTime(leaderboardId), playerId.Value);
            var gameTask = _db.SortedSetRemoveAsync(LeaderboardKeys.GameAllTime(gameId), playerId.Value);

            var results = await Task.WhenAll(leaderboardTask, gameTask);
            return results[0]; // Return leaderboard removal result
        }

        public async Task<List<LeaderboardEntry>> GetTopPlayersDtoAsync(int leaderboardId, int topN)
        {
            var raw = await GetTopPlayersAsync(leaderboardId, topN);

            var playerIds = raw.Select(e => e.Key).ToList();

            // Single query to get all names, avoids querying the DB once per player
            var names = await _context.Players
                .Where(p => playerIds.Contains(p.PlayerId))
                .ToDictionaryAsync(p => p.PlayerId, p => p.PlayerName);

            return raw.Select((e, i) => new LeaderboardEntry
            {
                PlayerId = e.Key,
                PlayerName = names.TryGetValue(e.Key, out var name) ? name : "Unknown",
                Score = e.Value,
                Rank = i + 1
            }).ToList();
        }

        public async Task<List<LeaderboardEntry>> GetTopPlayersByGameDtoAsync(int gameId, int topN)
        {
            var entries = await _db.SortedSetRangeByRankWithScoresAsync(LeaderboardKeys.GameAllTime(gameId), 0, topN - 1, Order.Descending);

            var raw = entries.Select(e => new KeyValuePair<int, double>((int)e.Element, e.Score)).ToList();

            var playerIds = raw.Select(e => e.Key).ToList();

            var names = await _context.Players
                .Where(p => playerIds.Contains(p.PlayerId))
                .ToDictionaryAsync(p => p.PlayerId, p => p.PlayerName);

            return raw.Select((e, i) => new LeaderboardEntry
            {
                PlayerId = e.Key,
                PlayerName = names.TryGetValue(e.Key, out var name) ? name : "Unknown",
                Score = e.Value,
                Rank = i + 1
            }).ToList();
        }
    }
}