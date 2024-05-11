using GameManager.DB.Models;
using GameManager.Services;
using Helper;
using Helper.Image;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Diagnostics;

namespace GameManager.Components.Pages.components
{
    public partial class Card
    {
        [Inject]
        private IDialogService? DialogService { get; set; }

        [Inject]
        private IConfigService? ConfigService { get; set; }

        [Parameter]
        public GameInfo? GameInfo { get; set; }

        [Parameter]
        public string? Width { get; set; }

        [Parameter]
        public string? Height { get; set; }

        [Parameter]
        public EventCallback<GameInfo> OnDeleteEventCallback { get; set; }

        private string ImageSrc
        {
            get
            {
                if (GameInfo == null || string.IsNullOrEmpty(GameInfo.CoverPath))
                    return "";
                return GameInfo.CoverPath.IsHttpLink()
                    ? GameInfo.CoverPath
                    : ImageHelper.GetDisplayUrl(ConfigService?.GetCoverFullPath(GameInfo.CoverPath).Result!);
            }
        }

        private async Task OnEdit()
        {
            Debug.Assert(ConfigService != null);
            if (GameInfo == null || DialogService == null)
                return;
            var inputModel = new DialogGameInfoEdit.FormModel();
            DataMapService.Map(GameInfo, inputModel);
            if (GameInfo.CoverPath != null && GameInfo.CoverPath.IsHttpLink())
                inputModel.Cover = GameInfo.CoverPath;
            else
                inputModel.Cover = ConfigService.GetCoverFullPath(GameInfo.CoverPath).Result;

            var parameters = new DialogParameters<DialogGameInfoEdit>
            {
                { x => x.Model, inputModel }
            };
            IDialogReference? dialogReference = await DialogService.ShowAsync<DialogGameInfoEdit>("Edit Game Info",
                parameters,
                new DialogOptions
                {
                    BackdropClick = false
                });
            DialogResult? dialogResult = await dialogReference.Result;
            if (dialogResult.Canceled)
                return;
            if (dialogResult.Data is not DialogGameInfoEdit.FormModel resultModel)
                return;
            try
            {
                if ((resultModel.Cover == null && GameInfo.CoverPath != null)
                    || (resultModel.Cover.IsHttpLink() && !GameInfo.CoverPath.IsHttpLink()))
                {
                    await ConfigService.DeleteCoverImage(GameInfo.CoverPath);
                }
                else if (resultModel.Cover != null && GameInfo.CoverPath.IsHttpLink() &&
                         !resultModel.Cover.IsHttpLink())
                {
                    resultModel.Cover = await ConfigService.AddCoverImage(resultModel.Cover);
                }
                else if (resultModel.Cover != null && !resultModel.Cover.IsHttpLink())
                {
                    await ConfigService.ReplaceCoverImage(resultModel.Cover, GameInfo.CoverPath);
                }
            }
            catch (Exception e)
            {
                await DialogService.ShowMessageBox("Error", $"{e.Message}", cancelText: "Cancel");
            }

            DataMapService.Map(resultModel, GameInfo);
            StateHasChanged();
        }

        private void OnOpenInExplorer()
        {
            Debug.Assert(GameInfo != null);
            string? path = Path.GetDirectoryName(GameInfo.ExePath);
            if (path == null)
                return;
            try
            {
                // using "explorer.exe" and send path
                Process.Start("explorer.exe", path);
            }
            catch (Exception ex)
            {
                DialogService?.ShowMessageBox("Error", ex.Message, cancelText: "Cancel");
            }
        }

        private async Task OnDelete()
        {
            if (OnDeleteEventCallback.HasDelegate)
                await OnDeleteEventCallback.InvokeAsync(GameInfo);
        }
    }
}