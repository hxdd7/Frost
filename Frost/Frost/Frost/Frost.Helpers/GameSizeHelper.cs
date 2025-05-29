using Frost.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Frost.Helpers
{
    public static class GameSizeHelper
    {
        /// <summary>
        /// Updates install size info for games with missing size values.
        /// </summary>
        public static async Task UpdateMissingGameSizesAsync()
        {
            var games = await DatabaseHelper.GetAllGamesAsync();

            foreach (var game in games)
            {
                if (game.InstallSizeBytes == 0 && !string.IsNullOrWhiteSpace(game.ExePath))
                {
                    try
                    {
                        string folderPath = Path.GetDirectoryName(game.ExePath);
                        if (Directory.Exists(folderPath))
                        {
                            long size = await Task.Run(() => GetDirectorySize(folderPath));
                            game.InstallSizeBytes = size;
                            game.InstallSizeFormatted = FormatSize(size);

                            await DatabaseHelper.UpdateGameAsync(game);
                            Debug.WriteLine($"Updated size for {game.Name}: {game.InstallSizeFormatted}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error calculating size for {game.Name}: {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Recursively calculates the total size of a folder.
        /// </summary>
        public static long GetDirectorySize(string folderPath)
        {
            try
            {
                return new DirectoryInfo(folderPath)
                    .EnumerateFiles("*", SearchOption.AllDirectories)
                    .Sum(file => file.Length);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Directory size error: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Converts bytes into a human-readable format.
        /// </summary>
        public static string FormatSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}
