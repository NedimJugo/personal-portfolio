using System;
using System.Threading.Tasks;
using Npgsql;

class Program
{
    static async Task Main()
    {
        string connStr = "Host=ep-proud-math-ageh2enc-pooler.c-2.eu-central-1.aws.neon.tech;Port=5432;Database=neondb;Username=neondb_owner;Password=REDACTED_DB_PASSWORD;SSL Mode=Require;Trust Server Certificate=true";

        await using var conn = new NpgsqlConnection(connStr);
        await conn.OpenAsync();
        Console.WriteLine("Connected to PostgreSQL Database!");

        // FRONT = ChatGPT Image Oct 1, 2025, 12_13_18 AM.png
        // BACK = 134308049.jpeg
        string swappedHeroJson = "{\"badgeText\":\"⚡ Available for Hire\",\"title\":\"Hey! I'm <span class=\\\"highlight-text\\\">Nedim Jugo</span>\",\"subtitle\":\"Full-Stack Developer & Digital Craftsman\",\"description\":\"I turn coffee into code and ideas into reality. Specializing in building modern web applications that users actually enjoy using!\",\"primaryButtonText\":\"View My Work\",\"secondaryButtonText\":\"Get In Touch\",\"characterImageUrl\":\"https://ecochallengeblob.blob.core.windows.net/ecochallenge/ChatGPT%20Image%20Oct%201,%202025,%2012_13_18%20AM.png\",\"speechBubbleText\":\"Let's build something amazing!\",\"flipPhotoUrl\":\"https://ecochallengeblob.blob.core.windows.net/ecochallenge/134308049.jpeg\",\"flipPhotoCaption\":\"Discover more about me!\"}";

        string sql = @"
            UPDATE ""SiteContents"" 
            SET ""Content"" = @json 
            WHERE ""Section"" = 'hero';";

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("json", swappedHeroJson);
        int rows = await cmd.ExecuteNonQueryAsync();
        Console.WriteLine($"Swapped hero images in PostgreSQL SiteContents! ({rows} row(s) updated)");
    }
}
