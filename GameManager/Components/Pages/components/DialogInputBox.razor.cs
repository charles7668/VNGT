using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GameManager.Components.Pages.components
{
    public partial class DialogInputBox
    {
        [Parameter]
        public List<InputModel> Inputs { get; set; } = new();

        private bool IsValid { get; set; } = true;

        private MudForm Form { get; set; } = null!;

        [CascadingParameter]
        private IMudDialogInstance DialogInstance { get; set; } = null!;

        private async Task OnOk()
        {
            await Form.Validate();
            if (!IsValid)
            {
                return;
            }

            DialogInstance.Close();
        }

        private Task OnCancel()
        {
            DialogInstance.Cancel();
            return Task.CompletedTask;
        }

        public class InputModel
        {
            public string? Label { get; set; }

            public Func<string?, string?> Validate { get; set; } = s => null;

            public string? Value { get; set; }
        }
    }
}