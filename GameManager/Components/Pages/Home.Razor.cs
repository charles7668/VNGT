using GameManager.Components.Pages.components;
using GameManager.Models;
using GameManager.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Diagnostics;

namespace GameManager.Components.Pages
{
    public partial class Home
    {
        private List<(GameInfo info, bool display)> ViewGameInfos { get; } = [];

        [Inject]
        private IDialogService? DialogService { get; set; }

        [Inject]
        private IConfigService? ConfigService { get; set; }

        private async Task AddNewGame(string exePath)
        {
            Debug.Assert(DialogService is not null);

            var inputModel = new DialogGameInfoEdit.FormModel
            {
                GameName = Path.GetFileName(Path.GetDirectoryName(exePath)) ?? "null",
                ExePath = exePath
            };
            var parameters = new DialogParameters<DialogGameInfoEdit>
            {
                { x => x.Model, inputModel }
            };
            IDialogReference? dialogReference = await DialogService.ShowAsync<DialogGameInfoEdit>("Add new game",
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
            var gameInfo = new GameInfo();
            Debug.Assert(ConfigService != null);
            if (resultModel.Cover != null)
            {
                try
                {
                    resultModel.Cover = await ConfigService.AddCoverImage(resultModel.Cover);
                }
                catch (Exception e)
                {
                    await DialogService.ShowMessageBox(
                        "Error",
                        $"{e.Message}"
                        , cancelText: "Cancel");
                }
            }

            DataMapService.Map(resultModel, gameInfo);
            ViewGameInfos.Add((gameInfo, true));
            StateHasChanged();
        }

        private void DeleteGameCard(GameInfo info)
        {
            for (int i = 0; i < ViewGameInfos.Count; i++)
            {
                if (ViewGameInfos[i].info != info)
                    continue;
                ViewGameInfos.RemoveAt(i);
                break;
            }

            StateHasChanged();
        }

        private void FilterInfo(string? pattern)
        {
            for (int i = 0; i < ViewGameInfos.Count; i++)
            {
                (GameInfo info, bool display) viewInfo = ViewGameInfos[i];
                viewInfo.display = string.IsNullOrEmpty(pattern) ||
                                   (viewInfo.info.GameName ?? "").Contains(pattern,
                                       StringComparison.CurrentCultureIgnoreCase);
                ViewGameInfos[i] = viewInfo;
            }

            StateHasChanged();
        }
    }
}