using GameManager.DB.Models;
using JetBrains.Annotations;

namespace GameManager.DTOs
{
    public class LaunchOptionDTO : IConvertable<LaunchOption>
    {
        [UsedImplicitly]
        public int Id { get; set; }

        [UsedImplicitly]
        public bool RunAsAdmin { get; set; }

        [UsedImplicitly]
        public bool RunWithVNGTTranslator { get; set; }

        [UsedImplicitly]
        public bool RunWithSandboxie { get; set; }

        [UsedImplicitly]
        public string SandboxieBoxName { get; set; } = "DefaultBox";

        [UsedImplicitly]
        public bool IsVNGTTranslatorNeedAdmin { get; set; }

        [UsedImplicitly]
        public string? LaunchWithLocaleEmulator { get; set; }

        public LaunchOption Convert()
        {
            return new LaunchOption
            {
                Id = Id,
                RunAsAdmin = RunAsAdmin,
                RunWithVNGTTranslator = RunWithVNGTTranslator,
                RunWithSandboxie = RunWithSandboxie,
                SandboxieBoxName = SandboxieBoxName,
                IsVNGTTranslatorNeedAdmin = IsVNGTTranslatorNeedAdmin,
                LaunchWithLocaleEmulator = LaunchWithLocaleEmulator
            };
        }

        public static LaunchOptionDTO Create(LaunchOption launchOption)
        {
            return new LaunchOptionDTO
            {
                Id = launchOption.Id,
                RunAsAdmin = launchOption.RunAsAdmin,
                RunWithVNGTTranslator = launchOption.RunWithVNGTTranslator,
                RunWithSandboxie = launchOption.RunWithSandboxie,
                SandboxieBoxName = launchOption.SandboxieBoxName,
                IsVNGTTranslatorNeedAdmin = launchOption.IsVNGTTranslatorNeedAdmin,
                LaunchWithLocaleEmulator = launchOption.LaunchWithLocaleEmulator
            };
        }
    }
}