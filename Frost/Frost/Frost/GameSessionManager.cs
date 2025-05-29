using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Frost.Models;
using SQLite;

namespace Frost.Helpers
{
    public class GameSessionManager
    {
        private static Dictionary<int, (Process Process, DateTime StartTime)> activeSessions = new();

        public static async Task StartSessionAsync(Game game)
        {
            try
            {
                var startTime = DateTime.Now;
                var process = Process.Start(new ProcessStartInfo
                {
                    FileName = game.ExePath,
                    UseShellExecute = true
                });

                if (process != null)
                {
                    activeSessions[game.Id] = (process, startTime);

                    // Monitor the process exit to log session
                    _ = Task.Run(async () =>
                    {
                        process.WaitForExit();
                        await StopSessionAsync(game.Id);
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Session Error] {ex.Message}");
            }
        }

        public static async Task StopSessionAsync(int gameId)
        {
            if (activeSessions.TryGetValue(gameId, out var session))
            {
                var endTime = DateTime.Now;
                var duration = (int)(endTime - session.StartTime).TotalSeconds;

                var record = new GameSession
                {
                    GameId = gameId,
                    StartTime = session.StartTime,
                    EndTime = endTime,
                    DurationSeconds = duration
                };

                await DatabaseHelper.SaveGameSessionAsync(record);
                activeSessions.Remove(gameId);
            }
        }

        public static async Task StopAllSessionsAsync()
        {
            var keys = activeSessions.Keys.ToList();
            foreach (var id in keys)
            {
                await StopSessionAsync(id);
            }
        }
    }
}
