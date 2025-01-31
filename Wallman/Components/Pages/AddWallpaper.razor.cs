using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Wallman.DataAccess.Wallpapers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using Image = SixLabors.ImageSharp.Image;
using BlazorCaptcha;
using Microsoft.JSInterop;

namespace Wallman.Components.Pages;

public partial class AddWallpaper : ComponentBase {
    [Inject] private IJSRuntime JS { get; set; }
    
    private string captchaInput = "";
    private string Captcha = "";
    private int CaptchaLetters = 5;
    
    private string nickname;
    private string name;
    private string resolution;
    private Platforms selectedPlatform = Platforms.Desktop;
    private IBrowserFile selectedFile;
    private string _statusMessage;
    private bool _isError;

    protected override Task OnInitializedAsync() {
        Captcha = Tools.GetCaptchaWord(CaptchaLetters);
        return base.OnInitializedAsync();
    }

    private void LoadFiles(InputFileChangeEventArgs e) {
        const long maxFileSize = 20 * 1024 * 1024;
        if (e.File.Size > maxFileSize) {
            _statusMessage = "Ошибка: Файл слишком большой. Максимальный размер — 20 МБ.";
            _isError = true;
            return;
        }

        if (e.File != null) {
            selectedFile = e.File;
        }
    }

    private async Task SendWallpaper() {
        if (captchaInput != Captcha || string.IsNullOrEmpty(captchaInput)) {
            _statusMessage = "Ошибка: Символы введены не верно.";
            _isError = true;
            return;
        }
        
        if (selectedFile == null) {
            _statusMessage = "Ошибка: Файл не выбран.";
            _isError = true;
            return;
        }

        if (string.IsNullOrWhiteSpace(name)) {
            _statusMessage = "Ошибка: Введите название обоев.";
            _isError = true;
            return;
        }

        await DownloadWallpaperToServer();
        await JS.InvokeVoidAsync("window.location.replace", "/upload");
    }

    private async Task DownloadWallpaperToServer() {
        string uploadsFolder = string.Empty;
        string filePath = string.Empty;
        try {
            var sanitizedName = Path.GetInvalidFileNameChars()
                .Aggregate(name, (current, c) => current.Replace(c, '_'))
                .Replace(" ", "_")
                .ToLower();

            uploadsFolder = Path.Combine(AppContext.BaseDirectory, "wallpapers",
                selectedPlatform.ToString(), sanitizedName);
            if (Directory.Exists(uploadsFolder)) {
                _statusMessage = $"Обои с таким названием уже существуют!";
                _isError = true;
                return;
            }

            Directory.CreateDirectory(uploadsFolder);

            filePath = Path.Combine(uploadsFolder, "wall" + Path.GetExtension(selectedFile.Name));
            _statusMessage = "Файл успешно загружен и отправлен на проверку!";
            _isError = false;
            await using FileStream fs = new(filePath, FileMode.Create);
            await selectedFile.OpenReadStream(20 * 1024 * 1024).CopyToAsync(fs);

            try {
                using (var image = Image.Load(filePath)) {
                    CreateLowesResolutionImage(image, uploadsFolder, Path.GetExtension(filePath));
                }
            }
            catch (Exception ex) {
                Console.WriteLine($"Ошибка при обработке изображения: {ex.Message}");
            }

            var wallpaper = new WallpaperModel() {
                Name = name,
                Platform = selectedPlatform.ToString(),
                Location = filePath.Substring(filePath.IndexOf("wallpapers")),
                Resolution = resolution,
                Approved = false,
                Uploader = "@" + nickname
            };
            await WallpapersDBContext.AddWallpaper(wallpaper.Name, wallpaper.Resolution, wallpaper.Location,
                wallpaper.Platform, wallpaper.Uploader);
        }
        catch (Exception ex) {
            if (File.Exists(filePath)) {
                File.Delete(filePath);
            }

            if (!string.IsNullOrEmpty(uploadsFolder) && Directory.Exists(uploadsFolder) &&
                !Directory.EnumerateFileSystemEntries(uploadsFolder).Any()) {
                Directory.Delete(uploadsFolder);
            }

            _statusMessage = $"Ошибка при загрузке файла: {ex.Message}";
            _isError = true;
        }
    }

    public void CreateLowesResolutionImage(Image inputImage, string uploadsFolder, string extension) {
        try {
            
            int width = 800;
            int height = 600;

            if (selectedPlatform == Platforms.Mobile) {
                width = 600;
                height = 1400;
            }

            inputImage.Mutate(x => x.Resize(width, height));

            string resizedFilePath = Path.Combine(uploadsFolder, "prev_wall" + extension);
            inputImage.Save(resizedFilePath, new JpegEncoder());
        }
        catch (Exception ex) {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}