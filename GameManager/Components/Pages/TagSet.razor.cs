using GameManager.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Web;

namespace GameManager.Components.Pages
{
    public partial class TagSet
    {
        [Inject]
        private IConfigService ConfigService { get; set; } = null!;

        [Inject]
        private NavigationManager NavigationManager { get; set; } = null!;

        private List<string> Tags { get; set; } = [];

        protected override async Task OnInitializedAsync()
        {
            Tags = (await ConfigService.GetTagsAsync()).ToList();
            await base.OnInitializedAsync();
        }

        private void OnChipClick(string tag)
        {
            string encode = HttpUtility.UrlEncode(tag);
            NavigationManager.NavigateTo($"/home/{encode}");
        }
    }
}