using RainbowChallengeTracker.DBAccess.Models;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.CompilerServices;

namespace RainbowChallengeTracker.DBAccess.Repository
{
    internal static class GuildRepository
    {
        private static readonly SqlConnection DbConnection = new(Environment.GetEnvironmentVariable("CONNECTIONSTRING"));
        private static readonly SqlCommand DbCommand = new("", DbConnection);

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static GuildModel? GetGuild(ulong id)
        {
            GuildModel? entity = null;

            try
            {
                DbCommand.CommandText = "SELECT CATEGORY FROM Guild WHERE Id = @id;";
                DbCommand.Parameters.Clear();
                DbCommand.Parameters.AddWithValue("id", (long)id);
                DbConnection.Open();
                var ret = DbCommand.ExecuteScalar();
                entity = new()
                {
                    Id = id,
                    Category = ret is null ? null : ret is DBNull ? null : (ulong)(long)ret
                };
                DbConnection.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                if (DbConnection.State == ConnectionState.Open)
                    DbConnection.Close();
            }
            return entity;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void CreateGuild(GuildModel guild)
        {
            try
            {
                DbCommand.CommandText = "INSERT INTO Guild (ID, CATEGORY) VALUES (@id, @category);";
                DbCommand.Parameters.Clear();
                DbCommand.Parameters.AddWithValue("id", (long)guild.Id);
                DbCommand.Parameters.AddWithValue("category", guild.Category is null ? DBNull.Value : (long?)guild.Category);
                DbConnection.Open();
                DbCommand.ExecuteNonQuery();
                DbConnection.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                if (DbConnection.State == ConnectionState.Open)
                    DbConnection.Close();
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void UpdateGuild(GuildModel guild)
        {
            try
            {
                DbCommand.CommandText = "UPDATE Guild SET CATEGORY = @category WHERE Id = @id;";
                DbCommand.Parameters.Clear();
                DbCommand.Parameters.AddWithValue("id", (long)guild.Id);
                DbCommand.Parameters.AddWithValue("category", guild.Category is null ? DBNull.Value : (long?)guild.Category);
                DbConnection.Open();
                DbCommand.ExecuteNonQuery();
                DbConnection.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                if (DbConnection.State == ConnectionState.Open)
                    DbConnection.Close();
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void DeleteGuild(ulong id)
        {
            try
            {
                DbCommand.CommandText = "DELETE FROM Guild WHERE Id = @id;";
                DbCommand.Parameters.Clear();
                DbCommand.Parameters.AddWithValue("id", (long)id);
                DbConnection.Open();
                DbCommand.ExecuteNonQuery();
                DbConnection.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                if (DbConnection.State == ConnectionState.Open)
                    DbConnection.Close();
            }
        }
    }
}
