using SQLite;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Frost.Models
{
    public static class CategoryRepository
    {
        public static async Task AddGameToCategoryAsync(SQLiteAsyncConnection db, int categoryId, int gameId)
        {
            var exists = await db.Table<CategoryGame>()
                                 .Where(x => x.CategoryId == categoryId && x.GameId == gameId)
                                 .FirstOrDefaultAsync();

            if (exists == null)
            {
                await db.InsertAsync(new CategoryGame
                {
                    CategoryId = categoryId,
                    GameId = gameId
                });
            }
        }

        public static async Task RemoveGameFromCategoryAsync(SQLiteAsyncConnection db, int categoryId, int gameId)
        {
            var item = await db.Table<CategoryGame>()
                               .Where(x => x.CategoryId == categoryId && x.GameId == gameId)
                               .FirstOrDefaultAsync();
            if (item != null)
                await db.DeleteAsync(item);
        }

        public static async Task<List<Game>> GetGamesForCategoryAsync(SQLiteAsyncConnection db, int categoryId)
        {
            var categoryGames = await db.Table<CategoryGame>()
                                        .Where(x => x.CategoryId == categoryId)
                                        .ToListAsync();

            var gameIds = categoryGames.Select(x => x.GameId).ToList();

            return await db.Table<Game>()
                           .Where(g => gameIds.Contains(g.Id))
                           .ToListAsync();
        }

        public static async Task<Category> GetCategoryWithGamesAsync(SQLiteAsyncConnection db, int categoryId)
        {
            var category = await db.FindAsync<Category>(categoryId);
            if (category != null)
                category.Games = await GetGamesForCategoryAsync(db, category.Id);

            return category;
        }
    }
}
