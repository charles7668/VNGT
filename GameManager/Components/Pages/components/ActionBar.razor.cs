using GameManager.Enums;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace GameManager.Components.Pages.components
{
    public partial class ActionBar
    {
        private string? SearchText { get; set; }

        [Parameter]
        public EventCallback<string> AddNewGameEvent { get; set; }

        [Parameter]
        public EventCallback<SearchParameter> SearchEvent { get; set; }

        [Parameter]
        public EventCallback OnDeleteEvent { get; set; }

        [Parameter]
        public EventCallback OnRefreshEvent { get; set; }

        private Dictionary<SortOrder, string> SortOrderDict { get; set; } = null!;

        private SortOrder SortBy { get; set; } = SortOrder.UPLOAD_TIME;

        [Parameter]
        public EventCallback<SortOrder> OnSortByChangeEvent { get; set; }

        private SearchFilter SearchFilterModel { get; set; } = new();

        protected override void OnInitialized()
        {
            SortOrderDict = new Dictionary<SortOrder, string>
            {
                { SortOrder.NAME, "Name" },
                { SortOrder.UPLOAD_TIME, "Upload Time" },
                { SortOrder.DEVELOPER, "Developer" }
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

        private async Task OnKeyDown(KeyboardEventArgs key)
        {
            if (key.Key == "Enter")
                await OnSearch();
        }

        private async Task OnSearch()
        {
            if (SearchEvent.HasDelegate)
            {
                await SearchEvent.InvokeAsync(new SearchParameter(SearchText, SearchFilterModel));
            }
        }

        private Task OnRefresh()
        {
            if (OnRefreshEvent.HasDelegate)
                return OnRefreshEvent.InvokeAsync();
            return Task.CompletedTask;
        }

        private async Task OnSortByChange()
        {
            if (OnSortByChangeEvent.HasDelegate)
                await OnSortByChangeEvent.InvokeAsync(SortBy);
        }

        public class SearchFilter
        {
            public bool SearchName { get; set; } = true;

            public bool SearchDeveloper { get; set; } = true;

            public bool SearchExePath { get; set; } = true;
        }

        public class SearchParameter(string? searchText, SearchFilter filter)
        {
            public string? SearchText { get; set; } = searchText;

            public SearchFilter SearchFilter { get; set; } = filter;
        }
    }
}