using GameManager.Models;
using GameManager.Services;
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

        private string ImageSrc
        {
            get
            {
                if (GameInfo == null || string.IsNullOrEmpty(GameInfo.CoverPath))
                    return "";
                return "data:image/png;base64, " +
                       Convert.ToBase64String(
                           File.ReadAllBytes(ConfigService?.GetCoverFullPath(GameInfo.CoverPath).Result!));
            }
        }

        private async Task OnEdit()
        {
            Debug.Assert(ConfigService != null);
            if (GameInfo == null || DialogService == null)
                return;
            var inputModel = new DialogGameInfoEdit.FormModel();
            DataMapService.Map(GameInfo, inputModel);
            inputModel.Cover = await ConfigService.GetCoverFullPath(GameInfo.CoverPath);
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
            if (resultModel.Cover != null)
            {
                try
                {
                    if (GameInfo.CoverPath != null)
                        await ConfigService.ReplaceCoverImage(resultModel.Cover, GameInfo.CoverPath);
                    else
                        resultModel.Cover = await ConfigService.AddCoverImage(resultModel.Cover);
                }
                catch (Exception e)
                {
                    await DialogService.ShowMessageBox("Error", $"{e.Message}", cancelText: "Cancel");
                }
            }
            else if (GameInfo.CoverPath != null)
            {
                await ConfigService.DeleteCoverImage(GameInfo.CoverPath);
            }

            DataMapService.Map(resultModel, GameInfo);
            StateHasChanged();
        }
    }
}