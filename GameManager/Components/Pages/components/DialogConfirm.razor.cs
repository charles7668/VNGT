using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GameManager.Components.Pages.components
{
    public partial class DialogConfirm
    {
        [Parameter]
        public string? Content { get; set; }

        [CascadingParameter]
        private MudDialogInstance DialogInstance { get; set; } = null!;

        public Task OnOk()
        {
            DialogInstance.Close();
            return Task.CompletedTask;
        }

        public Task OnCancel()
        {
            DialogInstance.Cancel();
            return Task.CompletedTask;
        }
    }
}