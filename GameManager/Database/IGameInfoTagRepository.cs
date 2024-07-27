using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameManager.Database
{
    public interface IGameInfoTagRepository
    {
        Task<bool> CheckGameHasTag(int tagId , int gameId);

        Task RemoveGameInfoTagAsync(int tagId, int gameId);
    }
}
