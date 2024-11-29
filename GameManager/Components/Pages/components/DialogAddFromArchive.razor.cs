using GameManager.DTOs;
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

        private IList<LibraryDTO> Libraries { get; set; } = new List<LibraryDTO>();

        [Inject]
        private IDialogService DialogService { get; set; } = null!;

        [Parameter]
        public string SelectedFile { get; set; } = string.Empty;

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

        private Task<IEnumerable<string>> GameNameValidate(string? gameName)
        {
            if (Result.TargetLibrary is null)
            {
                return Task.FromResult<IEnumerable<string>>(["Please select a library"]);
            }

            if (gameName is null)
            {
                return Task.FromResult<IEnumerable<string>>(["Please enter a game name"]);
            }

            try
            {
                string targetPath = Path.Combine(Result.TargetLibrary, gameName);
                _ = new DirectoryInfo(targetPath);
                if (Directory.Exists(targetPath))
                {
                    return Task.FromResult<IEnumerable<string>>(["Game already exists"]);
                }
            }
            catch (Exception e)
            {
                return Task.FromResult<IEnumerable<string>>([e.Message]);
            }


            return Task.FromResult<IEnumerable<string>>([]);
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