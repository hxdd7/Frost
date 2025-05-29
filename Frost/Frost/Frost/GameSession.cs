using SQLite;
using System;

namespace Frost.Models
{
    public class GameSession
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int GameId { get; set; } // Foreign key reference to Game

        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public int DurationSeconds { get; set; } // Calculated at end of session

        public string DateKey => StartTime.Date.ToString("yyyy-MM-dd"); // for grouping stats
    }
}
