using SQLite;
using System.Collections.Generic;
using System.Linq;

namespace Frost.Models
{
    public class Category
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        // REQUIRED
        [Indexed, NotNull]
        public string Name { get; set; }

        // OPTIONAL
        public string Description { get; set; }
        public string BackgroundImgUrl { get; set; }
        public string Company { get; set; }

        // Ignored property – populated in code after querying
        [Ignore]
        public List<Game> Games { get; set; } = new();

        [Ignore]
        public bool IsSelected { get; set; } // <-- Add this
    }
}
