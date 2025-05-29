using Microsoft.UI.Xaml;
using SQLite;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;

namespace Frost.Models
{
    public class Game
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Name { get; set; }
        public string Company { get; set; }
        public string BackgroundImgUrl { get; set; }
        public string CoverImgUrl { get; set; }
        public string IconUrl { get; set; }
        public string ExePath { get; set; }

        public bool IsPinned { get; set; }

        public string Tags { get; set; }

        public long InstallSizeBytes { get; set; }
        public string InstallSizeFormatted { get; set; }

        [Ignore]
        public List<string> TagList
        {
            get => string.IsNullOrWhiteSpace(Tags)
                ? new List<string>()
                : Tags.Split(',').Select(t => t.Trim()).ToList();

            set => Tags = string.Join(",", value);
        }

        public Visibility CoverImageVisibility { get; set; }

        [Ignore]
        public bool IsSelected { get; set; }

        [Ignore]
        public string InstallDrive
        {
            get
            {
                try
                {
                    return string.IsNullOrWhiteSpace(ExePath)
                        ? "Unknown"
                        : Path.GetPathRoot(ExePath)?.TrimEnd('\\') ?? "Unknown";
                }
                catch
                {
                    return "Unknown";
                }
            }
        }
    }
}
