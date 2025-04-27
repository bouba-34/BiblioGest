using BiblioGest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using BiblioGest.Data;

namespace BiblioGest.Utilities
{
    public static class SampleDataGenerator
    {
        public static void SeedSampleData(BiblioGestDbContext context)
        {
            // Check if we already have data
            if (context.Books.Any() || context.Members.Any() || context.Loans.Any())
            {
                return; // Database already has data
            }

            // Add some sample books
            var books = new List<Book>
            {
                new Book { ISBN = "9780061120084", Title = "To Kill a Mockingbird", Author = "Harper Lee", Publisher = "HarperCollins", PublicationYear = 1960, CategoryId = 1, CopiesAvailable = 3 },
                new Book { ISBN = "9780451524935", Title = "1984", Author = "George Orwell", Publisher = "Signet Classics", PublicationYear = 1949, CategoryId = 1, CopiesAvailable = 2 },
                new Book { ISBN = "9780141439518", Title = "Pride and Prejudice", Author = "Jane Austen", Publisher = "Penguin Classics", PublicationYear = 1813, CategoryId = 1, CopiesAvailable = 1 },
                new Book { ISBN = "9780743273565", Title = "The Great Gatsby", Author = "F. Scott Fitzgerald", Publisher = "Scribner", PublicationYear = 1925, CategoryId = 1, CopiesAvailable = 2 },
                new Book { ISBN = "9780060935467", Title = "To the Lighthouse", Author = "Virginia Woolf", Publisher = "Mariner Books", PublicationYear = 1927, CategoryId = 1, CopiesAvailable = 1 },
                
                new Book { ISBN = "9780393609394", Title = "Sapiens: A Brief History of Humankind", Author = "Yuval Noah Harari", Publisher = "Harper", PublicationYear = 2015, CategoryId = 2, CopiesAvailable = 2 },
                new Book { ISBN = "9781451648539", Title = "Steve Jobs", Author = "Walter Isaacson", Publisher = "Simon & Schuster", PublicationYear = 2011, CategoryId = 5, CopiesAvailable = 1 },
                new Book { ISBN = "9780544633384", Title = "Being Mortal: Medicine and What Matters in the End", Author = "Atul Gawande", Publisher = "Metropolitan Books", PublicationYear = 2014, CategoryId = 2, CopiesAvailable = 1 },
                
                new Book { ISBN = "9780547928227", Title = "The Hobbit", Author = "J.R.R. Tolkien", Publisher = "Houghton Mifflin", PublicationYear = 1937, CategoryId = 1, CopiesAvailable = 3 },
                new Book { ISBN = "9780553386790", Title = "A Brief History of Time", Author = "Stephen Hawking", Publisher = "Bantam", PublicationYear = 1988, CategoryId = 3, CopiesAvailable = 2 },
                new Book { ISBN = "9780307476463", Title = "Cosmos", Author = "Carl Sagan", Publisher = "Ballantine Books", PublicationYear = 1980, CategoryId = 3, CopiesAvailable = 1 },
                
                new Book { ISBN = "9780375831003", Title = "The Book Thief", Author = "Markus Zusak", Publisher = "Alfred A. Knopf", PublicationYear = 2005, CategoryId = 1, CopiesAvailable = 2 },
                new Book { ISBN = "9780307277671", Title = "The Road", Author = "Cormac McCarthy", Publisher = "Vintage", PublicationYear = 2006, CategoryId = 1, CopiesAvailable = 1 },
                
                new Book { ISBN = "9780316769174", Title = "The Catcher in the Rye", Author = "J.D. Salinger", Publisher = "Little, Brown and Company", PublicationYear = 1951, CategoryId = 1, CopiesAvailable = 2 },
                new Book { ISBN = "9780143105427", Title = "The Grapes of Wrath", Author = "John Steinbeck", Publisher = "Penguin Classics", PublicationYear = 1939, CategoryId = 1, CopiesAvailable = 1 }
            };
            
            context.Books.AddRange(books);
            context.SaveChanges();

            // Add some sample members
            var members = new List<Member>
            {
                new Member { FirstName = "John", LastName = "Smith", Email = "john.smith@example.com", Phone = "555-123-4567", Address = "123 Main St, Anytown", RegistrationDate = DateTime.Now.AddYears(-2), IsActive = true },
                new Member { FirstName = "Jane", LastName = "Doe", Email = "jane.doe@example.com", Phone = "555-234-5678", Address = "456 Oak Ave, Somewhere", RegistrationDate = DateTime.Now.AddYears(-1), IsActive = true },
                new Member { FirstName = "Robert", LastName = "Johnson", Email = "robert.j@example.com", Phone = "555-345-6789", Address = "789 Pine Ln, Elsewhere", RegistrationDate = DateTime.Now.AddMonths(-10), IsActive = true },
                new Member { FirstName = "Sarah", LastName = "Williams", Email = "sarah.w@example.com", Phone = "555-456-7890", Address = "101 Elm St, Nowhere", RegistrationDate = DateTime.Now.AddMonths(-8), IsActive = true },
                new Member { FirstName = "Michael", LastName = "Brown", Email = "michael.b@example.com", Phone = "555-567-8901", Address = "202 Maple Dr, Anywhere", RegistrationDate = DateTime.Now.AddMonths(-6), IsActive = true },
                new Member { FirstName = "Emily", LastName = "Jones", Email = "emily.j@example.com", Phone = "555-678-9012", Address = "303 Cedar Blvd, Someplace", RegistrationDate = DateTime.Now.AddMonths(-4), IsActive = true },
                new Member { FirstName = "David", LastName = "Miller", Email = "david.m@example.com", Phone = "555-789-0123", Address = "404 Birch Ct, Othertown", RegistrationDate = DateTime.Now.AddMonths(-3), IsActive = false },
                new Member { FirstName = "Jessica", LastName = "Davis", Email = "jessica.d@example.com", Phone = "555-890-1234", Address = "505 Walnut Pl, Newtown", RegistrationDate = DateTime.Now.AddMonths(-2), IsActive = true },
                new Member { FirstName = "Thomas", LastName = "Wilson", Email = "thomas.w@example.com", Phone = "555-901-2345", Address = "606 Spruce Way, Oldtown", RegistrationDate = DateTime.Now.AddMonths(-1), IsActive = true },
                new Member { FirstName = "Jennifer", LastName = "Taylor", Email = "jennifer.t@example.com", Phone = "555-012-3456", Address = "707 Fir Path, Hometown", RegistrationDate = DateTime.Now.AddDays(-15), IsActive = true }
            };
            
            context.Members.AddRange(members);
            context.SaveChanges();

            // Add some sample loans (some active, some returned, some overdue)
            var loans = new List<Loan>
            {
                // Active loans
                new Loan { BookId = 1, MemberId = 1, LoanDate = DateTime.Now.AddDays(-10), DueDate = DateTime.Now.AddDays(4) },
                new Loan { BookId = 3, MemberId = 2, LoanDate = DateTime.Now.AddDays(-7), DueDate = DateTime.Now.AddDays(7) },
                new Loan { BookId = 5, MemberId = 3, LoanDate = DateTime.Now.AddDays(-5), DueDate = DateTime.Now.AddDays(9) },
                new Loan { BookId = 7, MemberId = 4, LoanDate = DateTime.Now.AddDays(-3), DueDate = DateTime.Now.AddDays(11) },
                
                // Overdue loans
                new Loan { BookId = 9, MemberId = 5, LoanDate = DateTime.Now.AddDays(-20), DueDate = DateTime.Now.AddDays(-6) },
                new Loan { BookId = 11, MemberId = 6, LoanDate = DateTime.Now.AddDays(-18), DueDate = DateTime.Now.AddDays(-4) },
                
                // Returned loans
                new Loan { BookId = 2, MemberId = 7, LoanDate = DateTime.Now.AddDays(-30), DueDate = DateTime.Now.AddDays(-16), ReturnDate = DateTime.Now.AddDays(-17) },
                new Loan { BookId = 4, MemberId = 8, LoanDate = DateTime.Now.AddDays(-25), DueDate = DateTime.Now.AddDays(-11), ReturnDate = DateTime.Now.AddDays(-12) },
                new Loan { BookId = 6, MemberId = 9, LoanDate = DateTime.Now.AddDays(-22), DueDate = DateTime.Now.AddDays(-8), ReturnDate = DateTime.Now.AddDays(-10) },
                new Loan { BookId = 8, MemberId = 10, LoanDate = DateTime.Now.AddDays(-15), DueDate = DateTime.Now.AddDays(-1), ReturnDate = DateTime.Now.AddDays(-2) },
                
                // Historic loans to show book popularity
                new Loan { BookId = 1, MemberId = 5, LoanDate = DateTime.Now.AddDays(-60), DueDate = DateTime.Now.AddDays(-46), ReturnDate = DateTime.Now.AddDays(-48) },
                new Loan { BookId = 1, MemberId = 8, LoanDate = DateTime.Now.AddDays(-45), DueDate = DateTime.Now.AddDays(-31), ReturnDate = DateTime.Now.AddDays(-33) },
                new Loan { BookId = 9, MemberId = 2, LoanDate = DateTime.Now.AddDays(-40), DueDate = DateTime.Now.AddDays(-26), ReturnDate = DateTime.Now.AddDays(-28) },
                new Loan { BookId = 9, MemberId = 6, LoanDate = DateTime.Now.AddDays(-35), DueDate = DateTime.Now.AddDays(-21), ReturnDate = DateTime.Now.AddDays(-22) }
            };
            
            context.Loans.AddRange(loans);
            context.SaveChanges();
        }
    }
}