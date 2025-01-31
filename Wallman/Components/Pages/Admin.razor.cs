using Microsoft.AspNetCore.Components;
using Wallman.DataAccess.Wallpapers;

namespace Wallman.Components.Pages;

public partial class Admin : ComponentBase {
    private List<WallpaperModel> wallpapers = new();
    
    private bool logined = false;
    private async Task HandleLogin(bool isLoggedIn)
    {
        logined = isLoggedIn;
    }
    
    protected override async Task OnInitializedAsync() {
        await ReloadWallpapers();
    }
    
    private async Task Approve(int id) {
        await WallpapersDBContext.ApproveWallpaper(id);
        await ReloadWallpapers();
    }
    private async Task Reject(WallpaperModel wallpaperModel) {
        await WallpapersDBContext.RejectWallpaper(wallpaperModel.Id, wallpaperModel.Location);
        await ReloadWallpapers();
    }
    
    private async Task ReloadWallpapers() {
        wallpapers = (await WallpapersDBContext.GetWallappers()).Where(x => x.Approved == false).ToList();
        StateHasChanged(); 
    }
}