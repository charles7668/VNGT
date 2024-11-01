using GameManager.DB.Models;
using GameManager.Services;
using Microsoft.AspNetCore.Components;

namespace GameManager.Components.Pages.components
{
    public partial class GameDetailCharacter
    {
        [Parameter]
        [EditorRequired]
        public GameInfo GameInfo { get; set; } = null!;

        [Inject]
        private IImageService ImageService { get; set; } = null!;

        private ViewModel GameInfoVo { get; set; } = new();

        private bool IsLoading { get; set; } = true;
        private Task LoadingTask { get; set; } = Task.CompletedTask;

        protected override Task OnInitializedAsync()
        {
            IsLoading = true;
            _ = base.OnInitializedAsync();
            return Task.CompletedTask;
        }

        protected override Task OnAfterRenderAsync(bool firstRender)
        {
            if (IsLoading && LoadingTask.IsCompleted)
            {
                LoadingTask = Task.Run(() =>
                {
                    List<Character> characters = GameInfo.Characters;
                    GameInfoVo.Characters = characters.Select(x => new CharacterViewModel(ImageService)
                    {
                        Name = x.OriginalName ?? x.Name ?? "Unknown",
                        Description = x.Description ?? "",
                        ImageUrl = x.ImageUrl,
                        Age = x.Age,
                        Birthday = ConvertDisplayBirthday(x.Birthday),
                        BloodType = x.BloodType,
                        Sex = x.Sex
                    }).ToList();
                    InvokeAsync(StateHasChanged);
                    IsLoading = false;
                });
            }

            return base.OnAfterRenderAsync(firstRender);

            string? ConvertDisplayBirthday(string? birthday)
            {
                if (birthday == null)
                {
                    return null;
                }

                return !DateTime.TryParse(birthday, out DateTime date) ? null : date.ToString("MM-dd");
            }
        }

        private class CharacterViewModel(IImageService imageService)
        {
            public string Name { get; set; } = "";

            public string Description { get; set; } = "";

            public string? ImageUrl { get; set; }

            public string DisplayImage => imageService.UriResolve(ImageUrl);

            public string? Age { get; set; }

            public string? Sex { get; set; }

            public string? Birthday { get; set; }

            public string? BloodType { get; set; }
        }

        private class ViewModel
        {
            public List<CharacterViewModel> Characters { get; set; } = new();
        }
    }
}