using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;

namespace GameManager.Components.Pages.components
{
    public partial class DialogMultiLineInputBox
    {
        public string Title { get; set; } = "Title";

        public string InputText { get; set; } = "";

        [CascadingParameter]
        private MudDialogInstance DialogInstance { get; set; } = null!;

        private Task OnOk(MouseEventArgs obj)
        {
            DialogInstance.Close(InputText);
            return Task.CompletedTask;
        }

        private Task OnCancel(MouseEventArgs obj)
        {
            DialogInstance.Cancel();
            return Task.CompletedTask;
        }
    }
}