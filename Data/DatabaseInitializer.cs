using BiblioGest.Models;
using BiblioGest.Utilities;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Threading.Tasks;
using BiblioGest.Services;

namespace BiblioGest.Data
{
    public static class DatabaseInitializer
    {
        public static async Task InitializeAsync()
        {
            try
            {
                using (var context = new BiblioGestDbContext())
                {
                    // Make sure the database is created
                    await context.Database.EnsureCreatedAsync();
                    
                    // If we're using migrations, we can apply them here
                    // await context.Database.MigrateAsync();
                    
                    // Seed the database with sample data
                    var authService = new AuthenticationService(context);
                    await authService.InitializeDefaultUsersAsync();
                    
                    SampleDataGenerator.SeedSampleData(context);
                }
            }
            catch (Exception ex)
            {
                // In a real app, we would log this error
                Console.WriteLine($"Error initializing database: {ex.Message}");
                
                // Make sure the user sees the error
                System.Windows.MessageBox.Show(
                    $"Error initializing database: {ex.Message}\n\nThe application may not function correctly.",
                    "Database Initialization Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }

        public static string GetDatabasePath()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "BiblioGest",
                "BiblioGest.db");
        }
    }
}