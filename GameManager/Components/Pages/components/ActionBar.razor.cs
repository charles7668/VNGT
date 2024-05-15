﻿using GameManager.Enums;
using Microsoft.AspNetCore.Components;

namespace GameManager.Components.Pages.components
{
    public partial class ActionBar
    {
        private string? SearchText { get; set; }

        [Parameter]
        public EventCallback<string> AddNewGameEvent { get; set; }

        [Parameter]
        public EventCallback<string?> SearchEvent { get; set; }

        [Parameter]
        public EventCallback OnDeleteEvent { get; set; }

        private Dictionary<SortOrder, string> SortOrderDict { get; set; } = new()
        {
            { SortOrder.NAME, "Name" },
            { SortOrder.UPLOAD_TIME, "Upload Time" }
        };

        private SortOrder SortBy { get; set; } = SortOrder.UPLOAD_TIME;

        [Parameter]
        public EventCallback<SortOrder> OnSortByChangeEvent { get; set; }

        protected override void OnInitialized()
        {
            SortOrderDict = new Dictionary<SortOrder, string>
            {
                { SortOrder.NAME, "Name" },
                { SortOrder.UPLOAD_TIME, "Upload Time" }
            };

            base.OnInitialized();
        }

        private async Task OnAddNewGame()
        {
            var customFileType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.WinUI, new[] { ".exe" } }
            });

            var options = new PickOptions
            {
                PickerTitle = "Please select game exe file",
                FileTypes = customFileType
            };

            FileResult? result = await FilePicker.PickAsync(options);
            if (result == null)
            {
                return;
            }

            if (AddNewGameEvent.HasDelegate)
            {
                await AddNewGameEvent.InvokeAsync(result.FullPath);
            }
        }

        private async Task OnDeleteClick()
        {
            if (OnDeleteEvent.HasDelegate)
                await OnDeleteEvent.InvokeAsync();
        }

        private async Task OnSearch()
        {
            if (SearchEvent.HasDelegate)
            {
                await SearchEvent.InvokeAsync(SearchText);
            }
        }

        private async Task OnSortByChange()
        {
            if (OnSortByChangeEvent.HasDelegate)
                await OnSortByChangeEvent.InvokeAsync(SortBy);
        }
    }
}