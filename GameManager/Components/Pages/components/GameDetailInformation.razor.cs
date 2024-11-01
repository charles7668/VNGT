using GameManager.DB.Enums;
using GameManager.DB.Models;
using GameManager.Services;
using Microsoft.AspNetCore.Components;

namespace GameManager.Components.Pages.components
{
    public partial class GameDetailInformation
    {
        [Parameter]
        [EditorRequired]
        public GameInfo GameInfo { get; set; } = null!;

        private ViewModel GameInfoViewModel { get; set; } = new();

        [Inject]
        private IStaffService StaffService { get; set; } = null!;

        [Inject]
        private IConfigService ConfigService { get; set; } = null!;

        protected override Task OnInitializedAsync()
        {
            Task.Run(() =>
            {
                GameInfoViewModel.Introduction = GameInfo.Description ?? "";
                GameInfoViewModel.OriginalGameName = GameInfo.GameName ?? "";
                GameInfoViewModel.ChineseGameName = GameInfo.GameChineseName ?? "";
                GameInfoViewModel.EnglishGameName = GameInfo.GameEnglishName ?? "";
                GameInfoViewModel.ReleaseDate = GameInfo.DateTime?.ToString("yyyy-MM-dd") ??
                                                DateTime.MinValue.ToString("yyyy-MM-dd");
                GameInfoViewModel.Developer = GameInfo.Developer ?? "Unknown";
                GameInfoViewModel.Staffs = GameInfo.Staffs
                    .Where(x => StaffService.GetStaffRoleEnumByName(x.StaffRole.RoleName).Result != StaffRoleEnum.STAFF)
                    .Select(
                        x => new StaffViewModel
                        {
                            Role = GetRoleName(x.StaffRole.RoleName),
                            Name = x.Name
                        }).OrderBy(x => x.Role).ToList();

                GameInfoViewModel.RelatedSites = GameInfo.RelatedSites.Select(x => new RelatedSiteViewModel
                {
                    Name = x.Name,
                    Url = x.Url
                }).ToList();

                GameInfoViewModel.ReleaseInfoViewModels = GameInfo.ReleaseInfos.Select(x => new ReleaseInfoViewModel
                {
                    ReleaseName = x.ReleaseName,
                    ReleaseLanguage = x.ReleaseLanguage,
                    ReleaseDate = x.ReleaseDate == DateTime.MinValue ? "Unknown" : x.ReleaseDate.ToString("yyyy-MM-dd"),
                    Platforms = x.Platforms.Select(GetPlatformString).ToList(),
                    AgeRating = "R" + x.AgeRating + "+",
                    ExternalLinks = x.ExternalLinks.Select(y => new ExternalLinkViewModel
                    {
                        Url = y.Url,
                        Label = y.Label
                    }).ToList()
                }).ToList();
                GameInfoViewModel.Tags = GameInfo.Tags.Select(x => x.Name).ToList();
            });
            _ = base.OnInitializedAsync();
            return Task.CompletedTask;
        }

        private string GetPlatformString(PlatformEnum platformEnum)
        {
            return platformEnum switch
            {
                PlatformEnum.WINDOWS => "Windows",
                PlatformEnum.MACOS => "Mac",
                PlatformEnum.LINUX => "Linux",
                PlatformEnum.ANDROID => "Android",
                PlatformEnum.IOS => "iOS",
                PlatformEnum.NDS => "NDS",
                PlatformEnum.VNDS => "VNDS",
                PlatformEnum.PSP => "PSP",
                _ => "Unknown"
            };
        }

        private string GetRoleName(string roleNameInDb)
        {
            StaffRoleEnum roleEnum = StaffService.GetStaffRoleEnumByName(roleNameInDb).Result;
            return roleEnum switch
            {
                StaffRoleEnum.SCENARIO => "Scenario",
                StaffRoleEnum.SONG => "Song",
                StaffRoleEnum.MUSIC => "Music",
                StaffRoleEnum.ARTIST => "Artist",
                StaffRoleEnum.CHARACTER_DESIGN => "Character Design",
                StaffRoleEnum.DIRECTOR => "Director",
                StaffRoleEnum.STAFF => "Staff",
                _ => "Unknown"
            };
        }

        private class ViewModel
        {
            public string Introduction { get; set; } = string.Empty;

            public string OriginalGameName { get; set; } = string.Empty;

            public string EnglishGameName { get; set; } = string.Empty;

            public string ChineseGameName { get; set; } = string.Empty;

            public string ReleaseDate { get; set; } = DateTime.MinValue.ToString("yyyy-MM-dd");

            public string Developer { get; set; } = string.Empty;

            public List<StaffViewModel> Staffs { get; set; } = [];

            public List<ReleaseInfoViewModel> ReleaseInfoViewModels { get; set; } = [];

            public List<RelatedSiteViewModel> RelatedSites { get; set; } = [];

            public List<string> Tags { get; set; } = [];
        }

        private class CharacterViewModel
        {
            public string? Name { get; set; }

            public string? OriginalName { get; set; }

            public List<string> Alias { get; set; } = [];

            public string? Description { get; set; }

            public string? ImageUrl { get; set; }

            public string? Age { get; set; }

            public string? Sex { get; set; }

            public string? Birthday { get; set; }

            public string? BloodType { get; set; }
        }

        private class ExternalLinkViewModel
        {
            public string? Url { get; set; }

            public string? Label { get; set; }
        }

        private class ReleaseInfoViewModel
        {
            public string ReleaseName { get; set; } = string.Empty;

            public string ReleaseLanguage { get; set; } = string.Empty;

            public string ReleaseDate { get; set; } = "Unknown";

            public List<string> Platforms { get; set; } = [];

            public string AgeRating { get; set; } = "R0+";

            public List<ExternalLinkViewModel> ExternalLinks { get; set; } = [];
        }

        private class RelatedSiteViewModel
        {
            public string? Name { get; set; } = string.Empty;
            public string? Url { get; set; } = string.Empty;
        }

        private class StaffViewModel
        {
            public string Role { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
        }
    }
}