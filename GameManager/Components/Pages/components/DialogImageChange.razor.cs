using GameManager.Services;
using Helper;
using Helper.Image;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GameManager.Components.Pages.components
{
    public partial class DialogImageChange
    {
        private string _displayImageLink = string.Empty;

        private string? InputUrlError { get; set; }

        [CascadingParameter]
        public MudDialogInstance? MudDialog { get; set; }

        [Inject]
        private IImageService ImageService { get; set; } = null!;

        private string InputUrl { get; set; } = string.Empty;

        private string DisplayUrl
        {
            get
            {
                if (_displayImageLink.IsHttpLink() || _displayImageLink.StartsWith("cors://"))
                    return ImageService.UriResolve(_displayImageLink);
                return ImageHelper.GetDisplayUrl(_displayImageLink);
            }
            set => _displayImageLink = value;
        }

        private void OnApplyUrlClick()
        {
            if (!InputUrl.IsValidUrl())
            {
                InputUrlError = "Invalid URL";
                return;
            }

            InputUrlError = null;

            DisplayUrl = InputUrl;
            StateHasChanged();
        }

        private async Task OnUploadClick()
        {
            var options = new PickOptions
            {
                PickerTitle = "Please select cover image file",
                FileTypes = FilePickerFileType.Images
            };

            FileResult? result = await FilePicker.PickAsync(options);
            if (result == null)
            {
                return;
            }

            DisplayUrl = result.FullPath;
            StateHasChanged();
        }

        private void OnOk()
        {
            MudDialog?.Close(_displayImageLink);
        }

        private void OnCancel()
        {
            MudDialog?.Cancel();
        }
    }
}