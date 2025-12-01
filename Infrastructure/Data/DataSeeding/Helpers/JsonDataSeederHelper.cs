 
using System.Text.Json;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Ocsp;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Infrastructure.Data.DataSeeding.Helpers
{
    /// <summary>
    /// A professional utility class for reading and deserializing JSON seed files.
    /// This adheres to the DRY principle, centralizing file reading logic for all seeders.
    /// </summary>
    public static class JsonDataSeederHelper
    {
        /// <summary>
        /// Reads a JSON file from the defined path and deserializes its content into a list of DTOs.
        /// </summary>
        /// <typeparam name="T">The DTO type to deserialize the JSON data into.</typeparam>
        /// <param name="jsonFileName">The name of the JSON file (e.g., "FrequentFlyer.json").</param>
        /// <param name="logger">Logger instance for error reporting.</param>
        /// <returns>A list of deserialized DTO objects.</returns>
        /// <exception cref="FileNotFoundException">Thrown if the JSON file is not found.</exception>
        public static async Task<List<T>> ReadAndDeserializeJsonAsync<T>(
            string jsonFileName,
            ILogger? logger = null) where T : class
        {
            var basePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            // Define the primary expected path (e.g., Infrastructure/Data/DataSeeding/...)
            var filePath = Path.Combine(basePath ?? "", "Data", "DataSeeding", "DataSeedingFiles", jsonFileName);

            if (!File.Exists(filePath))
            {
                // Fallback path check for development/testing environments (e.g., project root)
                filePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "DataSeeding", "DataSeedingFiles", jsonFileName);
            }

            if (!File.Exists(filePath))
            {
                logger?.LogError("JSON file not found: {FileName}", jsonFileName);
                throw new FileNotFoundException($"JSON seed file was not found: {jsonFileName}", jsonFileName);
            }

            try
            {
                var jsonContent = await File.ReadAllTextAsync(filePath);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var data = JsonSerializer.Deserialize<List<T>>(jsonContent, options);

                if (data == null)
                {
                    logger?.LogWarning("JSON file {FileName} deserialized to null or empty list.", jsonFileName);
                    return new List<T>();
                }

                return data;
            }
            catch (JsonException ex)
            {
                logger?.LogError(ex, "Failed to deserialize JSON file {FileName}. Please check file structure.", jsonFileName);
                throw;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "An unexpected error occurred while reading JSON file {FileName}.", jsonFileName);
                throw;
            }
        }

        /// <summary>
        /// Resets the IDENTITY counter for a specified SQL Server table to ensure seeding starts from ID 1.
        /// This command runs ONLY if the table is currently empty.
        /// </summary>
        /// <param name="context">The database context instance (e.g., ApplicationDbContext).</param>
        /// <param name="tableName">The name of the table to reset (e.g., "user" or "frequent_flyer").</param>
        public static async Task ResetIdentityCounterAsync(DbContext context, string tableName)
        {


            //Check the database type to ensure it is SQL Server before using its T-SQL.
            if (context.Database.IsSqlServer())
            {
                // 1.Check if the table is empty.
                string checkEmptySql = $"SELECT COUNT(*) FROM [{tableName}]";
                int count = await context.Database.ExecuteSqlRawAsync(checkEmptySql);

                if (count == 0)
                {
                    // 2. If the table is empty, use the TRUNCATE TABLE. 
                    // The TRUNCATE TABLE is the professional solution that ensures the IDENTITY counter is reset to its initial value (1).
                    // Unlike DELETE, TRUNCATE always resets the counter.
                    string truncateSql = $"TRUNCATE TABLE [{tableName}]";

                    // Note: TRUNCATE cannot be used if foreign keys are active.
                    // In this case, DBCC CHECKIDENT(RESEED, 0) can be used, but we need to force it to work. 
                    // Since the ancillary_product table is a reference table, it should not have any FK pointing to it.
                    try
                    {
                        await context.Database.ExecuteSqlRawAsync(truncateSql);
                    }
                    catch (Exception)
                    {
                        // If TRUNCATE fails due to Foreign Keys (a rare occurrence for primary feed tables), we revert to the previous method with a modified value.
                        // We use -1 to ensure the current counter value becomes -1 (to correct any errors), then we use RESEED, 0.
                        // In your case, the surest solution is to set the first insert to 1. Since you're getting 0, the counter is currently -1.
                        // The counter must be 0 for the next insert to be 1.
                        string dbccFixSql =
                            $"IF NOT EXISTS(SELECT 1 FROM {tableName}) " +
                            $"DBCC CHECKIDENT('{tableName}', RESEED, 0);"; 
                        await context.Database.ExecuteSqlRawAsync(dbccFixSql);
                    }
                }
            }

            // Important note: When running the project for the first time, the table will be empty.
            // Use the previous method.If you deleted it from the database and want to repopulate it,
            // comment the previous method and use this to correct the counter after deletion.

            // Check the database type to ensure it is SQL Server before using DBCC.
            //if (context.Database.IsSqlServer())
            //{
            //    //  Correction: Enclose the table name in square brackets to avoid the reserved words issue.
            //    string bracketedTableName = $"[{tableName}]";

                //    //Use conditional T-SQL to ensure the command is executed only if the table is empty.
                //    string sqlCommand =
                //        $"IF NOT EXISTS(SELECT 1 FROM {bracketedTableName}) " +
                //        $"DBCC CHECKIDENT('{tableName}', RESEED, 0);";
                //    await context.Database.ExecuteSqlRawAsync(sqlCommand);
                //}
        }
    }
}