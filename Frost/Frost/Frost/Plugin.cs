using SQLite;

namespace Frost.Models
{
    public class Plugin
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Name { get; set; }
        public string Developer { get; set; }
        public string IconUrl { get; set; }
        public string ExePath { get; set; }

        [Ignore]
        public bool IsSelected { get; set; }
    }
}
