using Date_Tracker.Objects;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

namespace Date_Tracker.Scripts
{
    public static class DatabaseHandler
    {
        private static string LCS()
        {
            return ConfigurationManager.ConnectionStrings["default"].ConnectionString;
        }

        private static bool? DoesUIDAlreadyExist(int targetUid)
        {
            try
            {
                using (SqliteConnection cnn = new SqliteConnection(LCS()))
                {
                    if (cnn.State != System.Data.ConnectionState.Open) cnn.Open();

                    string sql = "SELECT COUNT(uid) FROM Dates WHERE uid=@1";
                    using var cmd = new SqliteCommand(sql, cnn);
                    cmd.Parameters.AddWithValue("@1", targetUid);

                    int returned = Convert.ToInt32(cmd.ExecuteScalar());
                    if (returned > 0) return true;
                    else return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not determine if UID already exists in database:\n{ex.Message}\n{ex.StackTrace}");
                return null;
            }
        }

        public static List<TrackedDate> GetSavedDates()
        {
            try
            {
                List<TrackedDate> fetched_dates = new List<TrackedDate>();
                using (SqliteConnection cnn = new SqliteConnection(LCS()))
                {
                    cnn.Open();
                    string sql = "SELECT * FROM Dates";
                    using var cmd = new SqliteCommand(sql, cnn);
                    using var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        string name = reader.GetString(0);
                        string dateAsString = reader.GetString(1);
                        int isPinned = reader.GetInt32(2);
                        int isFavourite = reader.GetInt32(3);
                        int deleteWhenReached = reader.GetInt32(4);
                        int mode = reader.GetInt32(5);
                        string timezone = reader.GetString(6);
                        int uid = reader.GetInt32(7);

                        TrackedDate newDate = new TrackedDate
                        {
                            Name = name,
                            Date = DateTime.Parse(dateAsString),
                            DateDisplay = dateAsString,
                            IsFavourite = (isFavourite == 1 ? true : false),
                            IsPinned = (isPinned == 1 ? true : false),
                            DeleteWhenReached = (deleteWhenReached == 1 ? true : false),
                            Mode = mode,
                            Timezone = timezone,
                            UID = uid
                        };
                        fetched_dates.Add(newDate);
                    }
                }
                return fetched_dates;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Something went wrong while attempting to fetch saved dates from database: {ex.Message}\n{ex.StackTrace}");
                return new List<TrackedDate>();
            }
        }

        public static bool AddTrackedDate(TrackedDate targetDate)
        {
            try
            {
                using (SqliteConnection cnn = new SqliteConnection(LCS()))
                {
                    cnn.Open();

                    string sql = "INSERT INTO Dates (name, date, is_pinned, is_favourite, delete_when_reached, mode, timezone, uid) VALUES (@1, @2, @3, @4, @5, @6, @7, @8)";
                    using var cmd = new SqliteCommand(sql, cnn);
                    
                    cmd.Parameters.AddWithValue("@1", targetDate.Name);
                    cmd.Parameters.AddWithValue("@2", targetDate.Date.ToString("dd/MM/yyyy"));
                    cmd.Parameters.AddWithValue("@3", Convert.ToInt32(targetDate.IsPinned));
                    cmd.Parameters.AddWithValue("@4", Convert.ToInt32(targetDate.IsFavourite));
                    cmd.Parameters.AddWithValue("@5", Convert.ToInt32(targetDate.DeleteWhenReached));
                    cmd.Parameters.AddWithValue("@6", targetDate.Mode);
                    cmd.Parameters.AddWithValue("@7", targetDate.Timezone);
                    int uid = targetDate.UID;
                    while (DoesUIDAlreadyExist(uid) == true)
                    {
                        uid = Utility.GenerateUID();
                    }
                    cmd.Parameters.AddWithValue("@8", uid);
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not save tracked date with UID: {targetDate.UID}:\n{ex.Message}\n{ex.StackTrace}");
                return false;
            }
        }

        public static bool DeleteTrackedDate(TrackedDate targetDate)
        {
            try
            {
                using (SqliteConnection cnn = new SqliteConnection(LCS()))
                {
                    cnn.Open();

                    string sql = "DELETE FROM Dates WHERE uid=@U";
                    using var cmd = new SqliteCommand(sql, cnn);
                    cmd.Parameters.AddWithValue("@U", targetDate.UID);

                    cmd.ExecuteNonQuery();
                    Debug.WriteLine($"Deleted with uid: {targetDate.UID}");
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not delete tracked date from database:\n{ex.Message}\n{ex.StackTrace}");
                return false;
            }
        }

        public static bool EditTrackedDate(TrackedDate targetDate, TrackedDate editedTargetDate)
        {
            try
            {
                using (SqliteConnection cnn = new SqliteConnection(LCS()))
                {
                    cnn.Open();

                    string sql = "UPDATE Dates SET name =@1, date=@2, is_pinned=@3, is_favourite=@4, delete_when_reached=@5, mode=@6, timezone=@7 WHERE uid=@UID";
                    using var cmd = new SqliteCommand(sql, cnn);
                    cmd.Parameters.AddWithValue("@1", editedTargetDate.Name);
                    cmd.Parameters.AddWithValue("@2", editedTargetDate.Date.ToString("dd/MM/yyyy"));
                    cmd.Parameters.AddWithValue("@3", Convert.ToInt32(editedTargetDate.IsPinned));
                    cmd.Parameters.AddWithValue("@4", Convert.ToInt32(editedTargetDate.IsFavourite));
                    cmd.Parameters.AddWithValue("@5", Convert.ToInt32(editedTargetDate.DeleteWhenReached));
                    cmd.Parameters.AddWithValue("@6", editedTargetDate.Mode);
                    cmd.Parameters.AddWithValue("@7", editedTargetDate.Timezone);
                    cmd.Parameters.AddWithValue("@UID", targetDate.UID);

                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to update tracked date with uid: {targetDate.UID}:\n{ex.Message}\n{ex.StackTrace}");
                return false;
            }
        }
    }
}
