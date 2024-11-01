using GameManager.DB.Models;
using GameManager.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor.Utilities;

namespace GameManager.Components.Pages.components
{
    public partial class GameDetailScreenShots
    {
        [Parameter]
        [EditorRequired]
        public GameInfo GameInfo { get; set; } = null!;

        private ViewModel GameInfoVo { get; } = new();

        [Inject]
        private IImageService ImageService { get; set; } = null!;

        private bool IsLoading { get; set; } = true;

        private string GetImageContainerClass(ScreenShotViewModel model)
        {
            return CssBuilder.Default("image-container")
                .AddClass("selected", model.IsSelected)
                .Build();
        }

        private void OnImageClick(ScreenShotViewModel model)
        {
            GameInfoVo.ScreenShots.ForEach(x => x.IsSelected = false);

            model.IsSelected = true;

            InvokeAsync(StateHasChanged);
        }

        protected override Task OnInitializedAsync()
        {
            _ = Task.Run(() =>
            {
                GameInfoVo.ScreenShots = GameInfo.ScreenShots.Select(x => new ScreenShotViewModel(ImageService)
                {
                    Url = x
                }).ToList();
                IsLoading = false;
                _ = InvokeAsync(StateHasChanged);
            });
            return base.OnInitializedAsync();
        }

        private class ScreenShotViewModel(IImageService imageService)
        {
            public string Url { get; set; } = string.Empty;
            public string Display => imageService.UriResolve(Url);

            public bool IsSelected { get; set; }
        }

        private class ViewModel
        {
            public List<ScreenShotViewModel> ScreenShots { get; set; } = [];
        }
    }
}