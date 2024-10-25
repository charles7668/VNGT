using GameManager.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GameManager.Components.Pages.components
{
    public partial class DialogAddFromArchive
    {
        [CascadingParameter]
        private MudDialogInstance DialogInstance { get; set; } = null!;

        [Inject]
        private IConfigService ConfigService { get; set; } = null!;

        private IList<DB.Models.Library> Libraries { get; set; } = new List<DB.Models.Library>();

        [Inject]
        private IDialogService DialogService { get; set; } = null!;

        private Model Result { get; } = new();
        public bool IsInputValid { get; set; }
        public string[]? Errors { get; set; }
        public MudForm FormInstance { get; set; } = null!;

        protected override async Task OnInitializedAsync()
        {
            Libraries = await ConfigService.GetLibrariesAsync(CancellationToken.None);

            await base.OnInitializedAsync();
        }

        private void OnOk()
        {
            if (Result.TargetLibrary is null)
            {
                DialogService.ShowMessageBox(new MessageBoxOptions
                {
                    Message = "Please select a library",
                    Title = "Error",
                    YesText = "Ok"
                });
                return;
            }

            if (!IsInputValid && Errors != null && Errors.Any())
            {
                DialogService.ShowMessageBox(new MessageBoxOptions
                {
                    Message = string.Join('\n', Errors),
                    Title = "Error",
                    YesText = "Ok"
                });
                return;
            }

            DialogInstance.Close(Result);
        }

        private void OnCancel()
        {
            DialogInstance.Cancel();
        }

        private async Task<IEnumerable<string>> GameNameValidate(string? gameName)
        {
            if (Result.TargetLibrary is null)
            {
                return ["Please select a library"];
            }

            if (gameName is null)
            {
                return ["Please enter a game name"];
            }

            try
            {
                string targetPath = Path.Combine(Result.TargetLibrary, gameName);
                _ = new DirectoryInfo(targetPath);
                if (Directory.Exists(targetPath))
                {
                    return ["Game already exists"];
                }
            }
            catch (Exception e)
            {
                return [e.Message];
            }


            return [];
        }

        private void OnLibraryChanged()
        {
            FormInstance.Validate();
        }

        public class Model
        {
            public string? TargetLibrary { get; set; }

            public string? GameName { get; set; }

            public string? ArchivePassword { get; set; }
        }
    }
}