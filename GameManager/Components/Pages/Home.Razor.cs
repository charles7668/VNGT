using GameManager.Components.Pages.components;
using GameManager.Database;
using GameManager.DB.Models;
using GameManager.Enums;
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
        public ISnackbar SnakeBar { get; set; } = null!;

        [Inject]
        private IUnitOfWork? UnitOfWork { get; set; }

        private List<ViewInfo> ViewGameInfos { get; } = [];

        [Inject]
        private IDialogService DialogService { get; set; } = null!;

        [Inject]
        private IConfigService ConfigService { get; set; } = null!;

        private bool IsSelectionMode { get; set; }

        protected override Task OnInitializedAsync()
        {
            Debug.Assert(UnitOfWork != null);
            _ = Task.Run(async () =>
            {
                Task loadTask = UnitOfWork.GameInfoRepository.GetGameInfoForEachAsync(info =>
                {
                    ViewGameInfos.Add(new ViewInfo
                    {
                        Info = info,
                        Display = true
                    });
                });

                await loadTask;
                _ = InvokeAsync(StateHasChanged);
            });

            _ = base.OnInitializedAsync();
            return Task.CompletedTask;
        }

        private async Task AddNewGame(string exePath)
        {
            var inputModel = new DialogGameInfoEdit.FormModel
            {
                GameName = Path.GetFileName(Path.GetDirectoryName(exePath)) ?? "null",
                ExePath = Path.GetDirectoryName(exePath)
            };
            var parameters = new DialogParameters<DialogGameInfoEdit>
            {
                { x => x.Model, inputModel }
            };
            IDialogReference? dialogReference = await DialogService.ShowAsync<DialogGameInfoEdit>("Add new game",
                parameters,
                new DialogOptions
                {
                    MaxWidth = MaxWidth.Large,
                    FullWidth = true,
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
                SnakeBar.Add(e.Message, Severity.Error);
                return;
            }

            ViewGameInfos.Add(new ViewInfo
            {
                Info = gameInfo,
                Display = true
            });
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
                if (ViewGameInfos[i].Info.Id != id)
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

        private void OnSearchInfo(string? pattern)
        {
            pattern = pattern?.Trim().ToLower();
            for (int i = 0; i < ViewGameInfos.Count; i++)
            {
                ViewInfo viewInfo = ViewGameInfos[i];
                string developer = viewInfo.Info.Developer ?? "UnKnown";
                viewInfo.Display = string.IsNullOrEmpty(pattern) ||
                                   (viewInfo.Info.GameName ?? "").ToLower().Contains(pattern,
                                       StringComparison.CurrentCultureIgnoreCase)
                                   || developer.ToLower().Contains(pattern,
                                       StringComparison.CurrentCulture)
                                   || (viewInfo.Info.ExePath ?? "").ToLower().Contains(pattern,
                                       StringComparison.CurrentCulture);
                ViewGameInfos[i] = viewInfo;
            }

            StateHasChanged();
        }

        private async Task OnDelete()
        {
            try
            {
                for (int i = 0; i < ViewGameInfos.Count; i++)
                {
                    if (!ViewGameInfos[i].IsSelected) continue;
                    await ConfigService.DeleteGameById(ViewGameInfos[i].Info.Id);
                    ViewGameInfos.RemoveAt(i);
                    i--;
                }
            }
            catch (Exception e)
            {
                await DialogService.ShowMessageBox("Error", e.Message, cancelText: "Cancel");
                return;
            }

            StateHasChanged();
        }

        private async Task OnSortByChange(SortOrder order)
        {
            switch (order)
            {
                case SortOrder.NAME:
                    ViewGameInfos.Sort((v1, v2) =>
                        string.Compare(v1.Info.GameName, v2.Info.GameName, StringComparison.Ordinal));
                    break;
                case SortOrder.UPLOAD_TIME:
                    ViewGameInfos.Sort((v1, v2) =>
                        DateTime.Compare((DateTime)v1.Info.UploadTime!, (DateTime)v2.Info.UploadTime!));
                    break;
                case SortOrder.DEVELOPER:
                    ViewGameInfos.Sort((v1, v2) =>
                        string.Compare(v1.Info.Developer, v2.Info.Developer, StringComparison.Ordinal));
                    break;
            }

            await InvokeAsync(StateHasChanged);
        }

        private void OnSelectionModeChange(bool value)
        {
            IsSelectionMode = value;
            foreach (ViewInfo viewInfo in ViewGameInfos)
            {
                viewInfo.IsSelected = false;
            }

            StateHasChanged();
        }

        private void OnSelectAllClick()
        {
            if (!IsSelectionMode)
                return;
            foreach (ViewInfo viewInfo in ViewGameInfos)
            {
                if (viewInfo.Display)
                    viewInfo.IsSelected = true;
            }

            StateHasChanged();
        }

        private void OnCardClick(ViewInfo viewInfo)
        {
            if (!IsSelectionMode)
                return;
            viewInfo.IsSelected = !viewInfo.IsSelected;
            StateHasChanged();
        }

        private Task OnRefreshClick()
        {
            Debug.Assert(UnitOfWork != null);
            ViewGameInfos.Clear();
            StateHasChanged();
            return Task.Run(async () =>
            {
                Task loadTask = UnitOfWork.GameInfoRepository.GetGameInfoForEachAsync(info =>
                {
                    ViewGameInfos.Add(new ViewInfo
                    {
                        Info = info,
                        Display = true
                    });
                });

                await loadTask;
                _ = InvokeAsync(StateHasChanged);
            });
        }

        private class ViewInfo
        {
            public GameInfo Info { get; set; }
            public bool Display { get; set; }
            public bool IsSelected { get; set; }
        }
    }
}