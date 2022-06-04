using RainbowChallengeTracker.DBAccess.Models;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.CompilerServices;

namespace RainbowChallengeTracker.DBAccess.Repository
{
    internal static class ChallengeRepository
    {
        private static readonly SqlConnection DbConnection = new(Environment.GetEnvironmentVariable("CONNECTIONSTRING"));
        private static readonly SqlCommand DbCommand = new("", DbConnection);

        public static readonly List<ChallengeModel> Challenges = new();

        static ChallengeRepository()
        {
            ReloadCache();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void ReloadCache()
        {
            try
            {
                Challenges.Clear();
                DbCommand.CommandText = "SELECT ID, TEXT, AMOUNT FROM Challenge";
                DbCommand.Parameters.Clear();
                DbConnection.Open();
                using var reader = DbCommand.ExecuteReader();
                while (reader.Read())
                {
                    Challenges.Add(new()
                    {
                        ID = reader.GetInt32("ID"),
                        Text = reader.GetString("TEXT"),
                        Amount = reader.GetInt32("AMOUNT")
                    });
                }
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
        public static void CreateChallenge(ChallengeModel challenge)
        {
            try
            {
                DbCommand.CommandText = "INSERT INTO Challenge (TEXT, AMOUNT) VALUES (@text, @amount);";
                DbCommand.Parameters.Clear();
                DbCommand.Parameters.AddWithValue("text", challenge.Text);
                DbCommand.Parameters.AddWithValue("amount", challenge.Amount);
                DbConnection.Open();
                DbCommand.ExecuteNonQuery();
                DbConnection.Close();

                Challenges.Add(challenge);
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
