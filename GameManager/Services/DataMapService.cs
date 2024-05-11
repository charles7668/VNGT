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
            dest.Vendor = src.Vendor;
            dest.Description = src.Description;
            dest.ExePath = src.ExePath;
            dest.DateTime = src.DateTime;
        }

        public static void Map(GameInfo src, DialogGameInfoEdit.FormModel dest)
        {
            dest.Cover = src.CoverPath;
            dest.GameName = src.GameName;
            dest.Vendor = src.Vendor;
            dest.Description = src.Description;
            dest.ExePath = src.ExePath;
            dest.DateTime = src.DateTime;
        }
    }
}