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
            dest.GameChineseName = src.GameChineseName;
            dest.GameEnglishName = src.GameEnglishName;
            dest.Developer = src.Vendor;
            dest.Description = src.Description;
            dest.ReleaseDate = src.DateTime;
            dest.ExePath = src.ExePath;
            dest.SaveFilePath = src.SaveFilePath;
            dest.ExeFile = src.ExeFile;
            dest.LaunchOption ??= new LaunchOption();
            dest.LaunchOption.RunAsAdmin = src.RunAsAdmin;
            dest.LaunchOption.LaunchWithLocaleEmulator = src.LeConfig;
            dest.LaunchOption.RunWithVNGTTranslator = src.RunWithVNGTTranslator;
            dest.LaunchOption.IsVNGTTranslatorNeedAdmin = src.IsVNGTTranslatorNeedAdmin;
            dest.LaunchOption.RunWithSandboxie = src.RunWithSandboxie;
            dest.LaunchOption.SandboxieBoxName = src.SandboxieBoxName;
            dest.Staffs = src.Staffs;
            dest.Characters = src.Characters;
            dest.ReleaseInfos = src.ReleaseInfos;
            dest.RelatedSites = src.RelatedSites;
            dest.ScreenShots = src.ScreenShots;
            dest.Tags = src.Tags.Select(x => new Tag()
            {
                Name = x
            }).ToList();
            dest.EnableSync = src.EnableSync;
        }

        public static void Map(GameInfo src, DialogGameInfoEdit.FormModel dest)
        {
            dest.Cover = src.CoverPath;
            dest.GameName = src.GameName;
            dest.GameChineseName = src.GameChineseName;
            dest.GameEnglishName = src.GameEnglishName;
            dest.Vendor = src.Developer;
            dest.Description = src.Description;
            dest.ExePath = src.ExePath;
            dest.SaveFilePath = src.SaveFilePath;
            dest.ExeFile = string.IsNullOrEmpty(src.ExeFile) ? "Not Set" : src.ExeFile;
            dest.DateTime = src.ReleaseDate;
            dest.RunAsAdmin = src.LaunchOption?.RunAsAdmin ?? false;
            dest.LeConfig = src.LaunchOption?.LaunchWithLocaleEmulator ?? "None";
            dest.RunWithVNGTTranslator = src.LaunchOption?.RunWithVNGTTranslator ?? false;
            dest.IsVNGTTranslatorNeedAdmin = src.LaunchOption?.IsVNGTTranslatorNeedAdmin ?? false;
            dest.RunWithSandboxie = src.LaunchOption?.RunWithSandboxie ?? false;
            dest.SandboxieBoxName = src.LaunchOption?.SandboxieBoxName ?? "DefaultBox";
            dest.Staffs = src.Staffs;
            dest.Characters = src.Characters;
            dest.ReleaseInfos = src.ReleaseInfos;
            dest.RelatedSites = src.RelatedSites;
            dest.ScreenShots = src.ScreenShots;
            dest.Tags = src.Tags.Select(x => x.Name).ToList();
            dest.EnableSync = src.EnableSync;
        }
    }
}