using GameManager.Services;
using Microsoft.AspNetCore.Components;
using System.Web;

namespace GameManager.Components.Pages
{
    public partial class TagSet
    {
        private List<string> _originalTags = [];

        [Inject]
        private IConfigService ConfigService { get; set; } = null!;

        [Inject]
        private NavigationManager NavigationManager { get; set; } = null!;

        private List<string> Tags { get; set; } = [];

        protected override async Task OnInitializedAsync()
        {
            _originalTags = (await ConfigService.GetTagsAsync()).ToList();
            Tags = _originalTags;
            await base.OnInitializedAsync();
        }

        private void OnChipClick(string tag)
        {
            string encode = HttpUtility.UrlEncode(tag);
            NavigationManager.NavigateTo($"/home/{encode}");
        }

        private Task OnFilterValueChanged(string input)
        {
            Tags = string.IsNullOrWhiteSpace(input)
                ? _originalTags
                : _originalTags.Where(x => x.Contains(input, StringComparison.OrdinalIgnoreCase)).ToList();
            return Task.CompletedTask;
        }
    }
}