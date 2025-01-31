using Microsoft.AspNetCore.Mvc;

namespace Wallman.DataAccess.Wallpapers;

[Route("api/wallpapers")]
[ApiController]
public class WallpaperController : ControllerBase {
    private readonly string basePath;

    public WallpaperController(IWebHostEnvironment env) {
        basePath = Path.Combine(AppContext.BaseDirectory, "wallpapers");
    }

    [HttpGet("{platform}/{folder}/{filename}")]
    public IActionResult GetWallpaper(string platform, string folder, string filename, [FromQuery] bool download = false) {
        var filePath = Path.Combine(basePath, platform,  folder, filename);
        
        if (!System.IO.File.Exists(filePath)) {
            return NotFound();
        }

        var fileBytes = System.IO.File.ReadAllBytes(filePath);
        
        var fileExtension = Path.GetExtension(filename);
        var contentType = fileExtension switch {
            ".png" => "image/png",
            ".jpeg" or ".jpg" => "image/jpeg",
            ".gif" => "image/gif",
            _ => "application/octet-stream"
        };
        if (download) {
            return File(fileBytes, contentType, filename);
        }
        
        return File(fileBytes, contentType);
    }

}
