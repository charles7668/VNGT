using GameManager.Components.Pages.components;
using GameManager.Database;
using GameManager.DB.Models;
using GameManager.Services;
using Helper;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Diagnostics;

namespace GameManager.Components.Pages
{
    public partial class Home
    {
        [Inject]
        private IUnitOfWork? UnitOfWork { get; set; }

        private List<(GameInfo info, bool display)> ViewGameInfos { get; } = [];

        [Inject]
        private IDialogService DialogService { get; set; } = null!;

        [Inject]
        private IConfigService ConfigService { get; set; } = null!;

        protected override async Task OnInitializedAsync()
        {
            Debug.Assert(UnitOfWork != null);
            List<GameInfo> infos = await UnitOfWork.GameInfoRepository.GetGameInfos();
            foreach (GameInfo info in infos)
            {
                ViewGameInfos.Add((info, true));
            }

            await base.OnInitializedAsync();
        }

        private async Task AddNewGame(string exePath)
        {
            var inputModel = new DialogGameInfoEdit.FormModel
            {
                GameName = Path.GetFileName(Path.GetDirectoryName(exePath)) ?? "null",
                ExePath = Path.GetDirectoryName(exePath),
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
                    if (!resultModel.Cover.IsHttpLink())
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
            try
            {
                await ConfigService.AddGameInfo(gameInfo);
            }
            catch (Exception e)
            {
                await DialogService.ShowMessageBox(
                    "Error",
                    $"{e.Message}"
                    , cancelText: "Cancel");
                return;
            }

            ViewGameInfos.Add((gameInfo, true));
            StateHasChanged();
        }

        private async Task DeleteGameCard(int id)
        {
            DialogParameters<DialogConfirm> parameters = new()
            {
                { x => x.Content, "Are you sure you want to delete?" }
            };
            IDialogReference? dialogReference = await DialogService.ShowAsync<DialogConfirm>("Warning", parameters,
                new DialogOptions
                {
                    BackdropClick = false
                });
            DialogResult? dialogResult = await dialogReference.Result;
            if (dialogResult.Canceled)
                return;

            bool hasChane = false;
            for (int i = 0; i < ViewGameInfos.Count; i++)
            {
                if (ViewGameInfos[i].info.Id != id)
                    continue;
                hasChane = true;
                try
                {
                    await ConfigService.DeleteGameById(id);
                }
                catch (Exception e)
                {
                    await DialogService.ShowMessageBox("Error", $"{e.Message}", cancelText: "Cancel");
                    return;
                }

                ViewGameInfos.RemoveAt(i);
                break;
            }

            if (hasChane)
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