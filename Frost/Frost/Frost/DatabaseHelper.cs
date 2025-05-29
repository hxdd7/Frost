using SQLite;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Frost.Models;
using System;
using Windows.Storage;

namespace Frost.Helpers
{
    public static class DatabaseHelper
    {
        // SQLite connection object for the database
        private static SQLiteAsyncConnection db;

        // Initialize the database and create necessary tables
        public static async Task InitializeDatabase()
        {
            string dbPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "frost.db");
            db = new SQLiteAsyncConnection(dbPath);

            // Create tables for Game and Plugin if they don't exist
            await db.CreateTableAsync<Game>();
            await db.CreateTableAsync<Plugin>();
            await db.CreateTableAsync<GameSession>();
        }

        public static async Task<List<Game>> GetAllGamesAsync()
        {
            try
            {
                await InitializeDatabase(); // Ensure db is initialized
                return await db.Table<Game>().ToListAsync();
            }
            catch (SQLiteException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching all games: {ex.Message}");
                return new List<Game>();
            }
        }


        #region Game Methods

        // Get all games from the database
        public static async Task<List<Game>> GetGamesAsync()
        {
            try
            {
                return await db.Table<Game>().ToListAsync();
            }
            catch (SQLiteException ex)
            {
                // Handle database read errors here
                System.Diagnostics.Debug.WriteLine($"Error fetching games: {ex.Message}");
                return new List<Game>();
            }
        }

        // Add a new game to the database
        public static async Task AddGameAsync(Game game)
        {
            try
            {
                await db.InsertAsync(game);
            }
            catch (SQLiteException ex)
            {
                // Handle database insert errors here
                System.Diagnostics.Debug.WriteLine($"Error adding game: {ex.Message}");
            }
        }

        // Delete a game from the database
        public static async Task DeleteGameAsync(Game game)
        {
            try
            {
                await db.DeleteAsync(game);
            }
            catch (SQLiteException ex)
            {
                // Handle database delete errors here
                System.Diagnostics.Debug.WriteLine($"Error deleting game: {ex.Message}");
            }
        }

        // Update a game's data in the database
        public static async Task UpdateGameAsync(Game game)
        {
            try
            {
                await db.UpdateAsync(game);
            }
            catch (SQLiteException ex)
            {
                // Handle database update errors here
                System.Diagnostics.Debug.WriteLine($"Error updating game: {ex.Message}");
            }
        }

        #endregion

        #region Plugin Methods

        // Get all plugins from the database
        public static async Task<List<Plugin>> GetPluginsAsync()
        {
            try
            {
                return await db.Table<Plugin>().ToListAsync();
            }
            catch (SQLiteException ex)
            {
                // Handle database read errors here
                System.Diagnostics.Debug.WriteLine($"Error fetching plugins: {ex.Message}");
                return new List<Plugin>();
            }
        }

        // Add a new plugin to the database
        public static async Task AddPluginAsync(Plugin plugin)
        {
            try
            {
                await db.InsertAsync(plugin);
            }
            catch (SQLiteException ex)
            {
                // Handle database insert errors here
                System.Diagnostics.Debug.WriteLine($"Error adding plugin: {ex.Message}");
            }
        }

        // Delete a plugin from the database
        public static async Task DeletePluginAsync(Plugin plugin)
        {
            try
            {
                await db.DeleteAsync(plugin);
            }
            catch (SQLiteException ex)
            {
                // Handle database delete errors here
                System.Diagnostics.Debug.WriteLine($"Error deleting plugin: {ex.Message}");
            }
        }

        // Update a plugin's data in the database
        public static async Task UpdatePluginAsync(Plugin plugin)
        {
            try
            {
                await db.UpdateAsync(plugin);
            }
            catch (SQLiteException ex)
            {
                // Handle database update errors here
                System.Diagnostics.Debug.WriteLine($"Error updating plugin: {ex.Message}");
            }
        }

        public static async Task<List<Game>> GetPinnedGamesAsync()
        {
            await InitializeDatabase(); // Ensure it's initialized
            return await db.Table<Game>().Where(g => g.IsPinned).ToListAsync();
        }

        public static async Task PinGameAsync(Game game)
        {
            game.IsPinned = true;
            await db.UpdateAsync(game);
        }

        public static async Task UnpinGameAsync(Game game)
        {
            game.IsPinned = false;
            await db.UpdateAsync(game);
        }

        public static async Task SaveGameSessionAsync(GameSession session)
        {
            var db = await GetDatabaseAsync();
            await db.InsertAsync(session);
        }

        public static async Task<List<GameSession>> GetGameSessionsForGameAsync(int gameId)
        {
            var db = await GetDatabaseAsync();
            return await db.Table<GameSession>()
                           .Where(s => s.GameId == gameId)
                           .OrderByDescending(s => s.StartTime)
                           .ToListAsync();
        }

        public static async Task<SQLiteAsyncConnection> GetDatabaseAsync()
        {
            var dbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Frost", "frost_games.db"
            );

            // Ensure directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(dbPath));

            var connection = new SQLiteAsyncConnection(dbPath);

            // Ensure tables exist
            await connection.CreateTableAsync<Game>();
            await connection.CreateTableAsync<GameSession>();  // Include this if GameSession table is used

            return connection;
        }
        public static async Task<List<GameSession>> GetAllGameSessionsAsync()
        {
            var db = await GetDatabaseAsync();
            return await db.Table<GameSession>()
                           .OrderByDescending(s => s.StartTime)
                           .ToListAsync();
        }

        #endregion
    }
}
