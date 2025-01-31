using MySql.Data.MySqlClient;

namespace Wallman.DataAccess.Wallpapers;

public class WallpapersDBContext {
    public static async Task<List<WallpaperModel>> GetWallappers() {
        var finalList = new List<WallpaperModel>();
        var connection = DBController.GetConnection();
        
        try {
            await DBController.OpenConnectionAsync();

            var query = "SELECT * FROM wallpapers";
            await using var cmd = new MySqlCommand(query, connection);
            await using var rdr = await cmd.ExecuteReaderAsync();
            while (await rdr.ReadAsync()) {
                finalList.Add(new WallpaperModel() {
                    Id = rdr.GetInt32(rdr.GetOrdinal("id")),
                    Name = rdr.GetString(rdr.GetOrdinal("name")),
                    Resolution = rdr.GetString(rdr.GetOrdinal("resolution")),
                    Location = rdr.GetString(rdr.GetOrdinal("location")),
                    Platform = rdr.GetString(rdr.GetOrdinal("platform")),
                    Approved = rdr.GetBoolean(rdr.GetOrdinal("approved")),
                    Uploader = rdr.GetString(rdr.GetOrdinal("uploader"))
                });
            }
        }
        catch (Exception e) {
            Console.WriteLine(e);
            throw;
        }
        finally {
            await DBController.CloseConnectionAsync();
        }

        return finalList;
    }

    public static async Task ApproveWallpaper(int id) {
        var connection = DBController.GetConnection();

        try {
            await DBController.OpenConnectionAsync();

            var query = "UPDATE wallpapers SET approved = @approved WHERE id = @id";
            await using var cmd = new MySqlCommand(query, connection);
            
            cmd.Parameters.AddWithValue("@approved", true);
            cmd.Parameters.AddWithValue("@id", id);

            var rowsAffected = await cmd.ExecuteNonQueryAsync();
            if (rowsAffected == 0) {
                throw new Exception("No wallpaper found with the given ID.");
            }
        }
        catch (Exception e) {
            Console.WriteLine(e);
            throw;
        }
        finally {
            await DBController.CloseConnectionAsync();
        }
    }

    public static async Task RejectWallpaper(int id, string location) {
        var connection = DBController.GetConnection();

        try {
            await DBController.OpenConnectionAsync();

            var query = "DELETE FROM wallpapers WHERE id = @id";
            await using var cmd = new MySqlCommand(query, connection);

            cmd.Parameters.AddWithValue("@id", id);
            
            var fullFilePath = Path.Combine(AppContext.BaseDirectory, location);
            var uploadsFolder = Path.GetDirectoryName(fullFilePath);
            
            if (File.Exists(fullFilePath)) {
                File.Delete(fullFilePath);
                File.Delete(Path.Combine(uploadsFolder, $"prev_wall{Path.GetExtension(fullFilePath)}"));
            }
            
            var rowsAffected = await cmd.ExecuteNonQueryAsync();
            if (rowsAffected == 0) {
                throw new Exception("No wallpaper found with the given ID.");
            }
            
            if (Directory.Exists(uploadsFolder) && Directory.GetFiles(uploadsFolder).Length == 0) {
                Directory.Delete(uploadsFolder);
            }
        }
        catch (Exception e) {
            Console.WriteLine(e);
            throw;
        }
        finally {
            await DBController.CloseConnectionAsync();
        }
    }
    
    public static async Task AddWallpaper(string name, string resolution, string location, string platform, string uploader) {
        var connection = DBController.GetConnection();

        try {
            await DBController.OpenConnectionAsync();

            var query = "INSERT INTO wallpapers (name, resolution, location, platform, approved, uploader) " +
                        "VALUES (@name, @resolution, @location, @platform, @approved, @uploader)";
            await using var cmd = new MySqlCommand(query, connection);

            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@resolution", resolution);
            cmd.Parameters.AddWithValue("@location", location);
            cmd.Parameters.AddWithValue("@platform", platform);
            cmd.Parameters.AddWithValue("@approved", false);
            cmd.Parameters.AddWithValue("@uploader", uploader);

            var rowsAffected = await cmd.ExecuteNonQueryAsync();
            if (rowsAffected == 0) {
                throw new Exception("Ошибка при добавлении обоев.");
            }
        }
        catch (Exception e) {
            Console.WriteLine(e);
            throw;
        }
        finally {
            await DBController.CloseConnectionAsync();
        }
    }

}