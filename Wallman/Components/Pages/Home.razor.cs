using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Wallman.DataAccess.Wallpapers;
namespace Wallman.Components.Pages;

public partial class Home : ComponentBase {
    [Inject] 
    private IJSRuntime JSRuntime { get; set; }

    private string searchQuery = "";
    private List<WallpaperModel> wallpapers = new();
    private Platforms platform = Platforms.Desktop;
    
    protected override async Task OnInitializedAsync() {
        wallpapers = await WallpapersDBContext.GetWallappers();
    }

    private async Task DownloadWallpaper(string wallpaperName, string wallpaperPlatform, string ext) {
        var url = $"api/wallpapers/{wallpaperPlatform}/{wallpaperName}/wall{ext}";
        await JSRuntime.InvokeVoidAsync("downloadWallpaper", url);
    }

    
    private string GetButtonClass(Platforms platformC) {
        return platform == platformC ? "button primary" : "button secondary";
    }
    private void ChangePlatform(string platformC) {
        platform = Enum.Parse<Platforms>(platformC);
    }
}