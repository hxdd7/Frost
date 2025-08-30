using SQLite;

namespace Frost.Models
{
    public class CategoryGame
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public int CategoryId { get; set; }

        [Indexed]
        public int GameId { get; set; }
    }
}
