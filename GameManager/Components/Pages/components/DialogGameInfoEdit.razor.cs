using GameManager.DB.Models;
using GameManager.GameInfoProvider;
using GameManager.Services;
using Helper.Image;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Diagnostics;
using System.Globalization;

namespace GameManager.Components.Pages.components
{
    public partial class DialogGameInfoEdit
    {
        private const string? DIALOG_WIDTH = "500px";

        private string? _bound = "not set";

        [Parameter]
        public FormModel Model { get; set; } = new();

        [Inject]
        private IDialogService DialogService { get; set; } = null!;

        [Inject]
        private IProvider Provider { get; set; } = null!;

        private string CoverPath => ImageHelper.GetDisplayUrl(Model.Cover);

        [CascadingParameter]
        public MudDialogInstance? MudDialog { get; set; }

        private void OnCancel()
        {
            MudDialog?.Cancel();
        }

        private void OnSave()
        {
            MudDialog?.Close(DialogResult.Ok(Model));
        }

        private void DatePickerTextChanged(string? value)
        {
            if (value == null || value.Length < 6)
            {
                Model.DateTime = null;
            }
            else
            {
                string[] formats = ["yyyy/MM/dd"];
                if (DateTime.TryParseExact(value, formats, CultureInfo.InvariantCulture, DateTimeStyles.None,
                        out DateTime validDate))
                {
                    Model.DateTime = validDate;
                }
                else
                {
                    Model.DateTime = null;
                }
            }

            _bound = Model.DateTime.HasValue ? Model.DateTime.Value.ToString("yyyy/MM") : "not set";
        }

        private async Task UploadByUrl()
        {
            Debug.Assert(DialogService != null);
            IDialogReference? dialogReference = await DialogService.ShowAsync<DialogImageChange>("Change Cover",
                new DialogOptions
                {
                    BackdropClick = false
                });
            DialogResult? dialogResult = await dialogReference.Result;
            if (dialogResult.Canceled)
                return;
            string? cover = dialogResult.Data as string;
            Model.Cover = cover;
        }

        private async Task OnInfoFetch()
        {
            if (string.IsNullOrEmpty(Model.GameName))
                return;
            try
            {
                (List<GameInfo>? infoList, bool hasMore) =
                    await Provider.FetchGameSearchListAsync(Model.GameName, 10, 1);
                if (infoList == null || infoList.Count == 0)
                    return;
                string gameId = infoList[0].GameInfoId ?? "";
                if (infoList.Count > 1)
                {
                    var parameters = new DialogParameters<DialogFetchSelection>
                    {
                        { x => x.DisplayInfos, infoList },
                        { x => x.HasMore, hasMore },
                        { x => x.SearchName, Model.GameName }
                    };
                    IDialogReference? dialogReference = await DialogService.ShowAsync<DialogFetchSelection>("",
                        parameters,
                        options: new DialogOptions
                        {
                            BackdropClick = false
                        });
                    DialogResult? dialogResult = await dialogReference.Result;
                    if (dialogResult.Canceled)
                        return;
                    gameId = dialogResult.Data as string ?? "";
                }

                GameInfo? info = await Provider.FetchGameDetailByIdAsync(gameId);
                if (info == null)
                    return;
                info.ExePath = Model.ExePath;
                DataMapService.Map(info, Model);
                StateHasChanged();
            }
            catch (Exception e)
            {
                await DialogService.ShowMessageBox("Error", e.Message, "Cancel");
            }
        }

        public class FormModel
        {
            [Label("Game name")]
            public string? GameName { get; set; }

            [Label("Developer")]
            public string? Vendor { get; set; }

            [Label("Executable path")]
            public string? ExePath { get; set; }

            [Label("Release Date")]
            public DateTime? DateTime { get; set; }

            public string? Cover { get; set; }

            [Label("Description")]
            public string? Description { get; set; }
        }
    }
}