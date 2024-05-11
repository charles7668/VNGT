using GameManager.DB.Models;
using GameManager.GameInfoProvider;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GameManager.Components.Pages.components
{
    public partial class DialogFetchSelection
    {
        private int _page = 1;

        private int _selectedIndex;

        [Parameter]
        public string SearchName { get; set; } = null!;

        [Inject]
        private IProvider Provider { get; set; } = null!;

        [CascadingParameter]
        public MudDialogInstance MudDialog { get; set; } = null!;

        [Inject]
        private IDialogService DialogService { get; set; } = null!;

        [Parameter]
        public List<GameInfo>? DisplayInfos { get; set; } = new();

        [Parameter]
        public bool HasMore { get; set; }

        private bool PrevBtnDisabled => _page <= 1;

        private bool NextBtnDisabled => !HasMore;

        private async Task OnPageChange(int direction)
        {
            try
            {
                int newPage = _page + direction;
                (List<GameInfo>? infoList, bool hasMore) =
                    await Provider.FetchGameSearchListAsync(SearchName, 10, newPage);
                if (infoList == null)
                {
                    await DialogService.ShowMessageBox("Error", "parse result failed", "cancel");
                    return;
                }

                _page = newPage;
                DisplayInfos = infoList;
                HasMore = hasMore;
                StateHasChanged();
            }
            catch (Exception e)
            {
                await DialogService.ShowMessageBox("Error", e.Message);
            }
        }

        private async Task OnNextPage()
        {
            await OnPageChange(1);
        }

        private async Task OnPrevPage()
        {
            await OnPageChange(-1);
        }

        private Task OnOk()
        {
            if (DisplayInfos == null || DisplayInfos.Count == 0)
            {
                MudDialog.Close(null);
                return Task.CompletedTask;
            }

            MudDialog.Close(DisplayInfos[_selectedIndex].GameInfoId);
            return Task.CompletedTask;
        }

        private Task OnCancel()
        {
            MudDialog.Cancel();
            return Task.CompletedTask;
        }
    }
}