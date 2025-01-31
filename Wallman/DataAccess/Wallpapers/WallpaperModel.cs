namespace Wallman.DataAccess.Wallpapers;

public enum Platforms {
    Desktop,
    Mobile
}

public class WallpaperModel {
    public int Id { get; set; }
    public string Name { get; set; }
    public string Resolution { get; set; }
    public string Location { get; set; }
    public string Platform { get; set; }
    public bool Approved { get; set; }
    public string Uploader { get; set; }
}