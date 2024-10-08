﻿namespace GameManager.Database
{
    public interface IUnitOfWork
    {
        IGameInfoRepository GameInfoRepository { get; }

        ILibraryRepository LibraryRepository { get; }

        IAppSettingRepository AppSettingRepository { get; }

        ITagRepository TagRepository { get; }

        IGameInfoTagRepository GameInfoTagRepository { get; }

        ILaunchOptionRepository LaunchOptionRepository { get; }

        Task<int> SaveChangesAsync();
    }
}