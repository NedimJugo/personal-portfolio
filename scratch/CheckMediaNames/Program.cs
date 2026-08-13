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

        string sql = @"SELECT ""Id"", ""FileName"", ""FileUrl"", LENGTH(""FileData"") FROM ""Media"" WHERE ""FileName"" LIKE '%character%' OR ""FileName"" LIKE '%flip%' OR ""FileName"" LIKE '%logo%';";

        await using var cmd = new NpgsqlCommand(sql, conn);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            Console.WriteLine($"Media: Id={reader[0]}, FileName='{reader[1]}', FileUrl='{reader[2]}', FileDataBytes={reader[3]}");
        }
    }
}
