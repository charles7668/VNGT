﻿using GameManager.DB.Models;
using GameManager.Enums;

namespace GameManager.Database
{
    public interface IGameInfoRepository
    {
        Task<List<GameInfo>> GetGameInfos(SortOrder order = SortOrder.UPLOAD_TIME);

        Task<string?> GetCoverById(int id);

        Task AddAsync(GameInfo info);

        Task EditAsync(GameInfo info);

        Task DeleteByIdAsync(int id);
    }
}