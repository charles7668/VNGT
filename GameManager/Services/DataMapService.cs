using GameManager.Components.Pages.components;
using GameManager.DB.Models;

namespace GameManager.Services
{
    public static class DataMapService
    {
        public static void Map(DialogGameInfoEdit.FormModel src, GameInfo dest)
        {
            dest.CoverPath = src.Cover;
            dest.GameName = src.GameName;
            dest.Developer = src.Vendor;
            dest.Description = src.Description;
            dest.ExePath = src.ExePath;
            dest.DateTime = src.DateTime;
            dest.LaunchOption ??= new LaunchOption();
            dest.LaunchOption.RunAsAdmin = src.RunAsAdmin;
        }

        public static void Map(GameInfo src, DialogGameInfoEdit.FormModel dest)
        {
            dest.Cover = src.CoverPath;
            dest.GameName = src.GameName;
            dest.Vendor = src.Developer;
            dest.Description = src.Description;
            dest.ExePath = src.ExePath;
            dest.DateTime = src.DateTime;
            dest.RunAsAdmin = src.LaunchOption?.RunAsAdmin ?? false;
        }
    }
}