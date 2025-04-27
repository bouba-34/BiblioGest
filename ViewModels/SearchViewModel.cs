using BiblioGest.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using BiblioGest.Data;

namespace BiblioGest.ViewModels
{
    public class SearchViewModel : ViewModelBase
    {
        private readonly BiblioGestDbContext _context;
        private string _searchText;
        private ObservableCollection<Book> _bookResults;
        private ObservableCollection<Member> _memberResults;
        private ObservableCollection<Category> _categories;
        private int? _selectedCategoryId;
        private int _startYear;
        private int _endYear;
        private bool _searchBooks;
        private bool _searchMembers;
        private Book _selectedBook;
        private Member _selectedMember;

        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        public ObservableCollection<Book> BookResults
        {
            get => _bookResults;
            set => SetProperty(ref _bookResults, value);
        }

        public ObservableCollection<Member> MemberResults
        {
            get => _memberResults;
            set => SetProperty(ref _memberResults, value);
        }

        public ObservableCollection<Category> Categories
        {
            get => _categories;
            set => SetProperty(ref _categories, value);
        }

        public int? SelectedCategoryId
        {
            get => _selectedCategoryId;
            set => SetProperty(ref _selectedCategoryId, value);
        }

        public int StartYear
        {
            get => _startYear;
            set => SetProperty(ref _startYear, value);
        }

        public int EndYear
        {
            get => _endYear;
            set => SetProperty(ref _endYear, value);
        }

        public bool SearchBooks
        {
            get => _searchBooks;
            set => SetProperty(ref _searchBooks, value);
        }

        public bool SearchMembers
        {
            get => _searchMembers;
            set => SetProperty(ref _searchMembers, value);
        }

        public Book SelectedBook
        {
            get => _selectedBook;
            set => SetProperty(ref _selectedBook, value);
        }

        public Member SelectedMember
        {
            get => _selectedMember;
            set => SetProperty(ref _selectedMember, value);
        }

        public ICommand SearchCommand { get; private set; }
        public ICommand ClearCommand { get; private set; }
        public ICommand ExportResultsCommand { get; private set; }

        public SearchViewModel(BiblioGestDbContext context)
        {
            _context = context;
            
            // Initialize commands
            SearchCommand = new RelayCommand(Search, CanSearch);
            ClearCommand = new RelayCommand(Clear);
            ExportResultsCommand = new RelayCommand(ExportResults, CanExportResults);
            
            // Initialize search parameters
            SearchBooks = true;
            SearchMembers = false;
            StartYear = 1900;
            EndYear = DateTime.Now.Year;
            
            // Load categories
            LoadCategories();
            
            // Initialize results collections
            BookResults = new ObservableCollection<Book>();
            MemberResults = new ObservableCollection<Member>();
        }

        private void LoadCategories()
        {
            var categoriesList = _context.Categories.ToList();
            // Add "All Categories" option at the beginning
            var allCategories = new Category { Id = 0, Name = "All Categories" };
            categoriesList.Insert(0, allCategories);
            Categories = new ObservableCollection<Category>(categoriesList);
        }

        private bool CanSearch(object parameter)
        {
            return !string.IsNullOrWhiteSpace(SearchText) || SelectedCategoryId.HasValue;
        }

        private void Search(object parameter)
        {
            if (SearchBooks)
            {
                SearchBooksMethod();
            }
            
            if (SearchMembers)
            {
                SearchMembersMethod();
            }
        }

        private void SearchBooksMethod()
        {
            var query = _context.Books
                .Include(b => b.Category)
                .AsQueryable();
            
            // Filter by search text
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                string searchLower = SearchText.ToLower();
                query = query.Where(b => 
                    b.Title.ToLower().Contains(searchLower) || 
                    b.Author.ToLower().Contains(searchLower) || 
                    b.ISBN.ToLower().Contains(searchLower) ||
                    b.Publisher.ToLower().Contains(searchLower));
            }
            
            // Filter by category
            if (SelectedCategoryId.HasValue && SelectedCategoryId.Value > 0)
            {
                query = query.Where(b => b.CategoryId == SelectedCategoryId.Value);
            }
            
            // Filter by publication year range
            query = query.Where(b => b.PublicationYear >= StartYear && b.PublicationYear <= EndYear);
            
            var results = query.ToList();
            BookResults = new ObservableCollection<Book>(results);
        }

        private void SearchMembersMethod()
        {
            var query = _context.Members.AsQueryable();
            
            // Filter by search text
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                string searchLower = SearchText.ToLower();
                query = query.Where(m => 
                    m.FirstName.ToLower().Contains(searchLower) || 
                    m.LastName.ToLower().Contains(searchLower) || 
                    m.Email.ToLower().Contains(searchLower) ||
                    m.Phone.Contains(SearchText));
            }
            
            var results = query.ToList();
            MemberResults = new ObservableCollection<Member>(results);
        }

        private void Clear(object parameter)
        {
            SearchText = string.Empty;
            SelectedCategoryId = null;
            StartYear = 1900;
            EndYear = DateTime.Now.Year;
            BookResults.Clear();
            MemberResults.Clear();
        }

        private bool CanExportResults(object parameter)
        {
            return (SearchBooks && BookResults.Count > 0) || (SearchMembers && MemberResults.Count > 0);
        }

        private void ExportResults(object parameter)
        {
            try
            {
                // Create export directory if it doesn't exist
                string exportDir = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "BiblioGest");
                System.IO.Directory.CreateDirectory(exportDir);
                
                string fileName = $"SearchResults_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                string filePath = System.IO.Path.Combine(exportDir, fileName);
                
                using (var writer = new System.IO.StreamWriter(filePath))
                {
                    if (SearchBooks && BookResults.Count > 0)
                    {
                        writer.WriteLine("Book Results");
                        writer.WriteLine("ISBN,Title,Author,Publisher,Year,Category,Copies Available");
                        
                        foreach (var book in BookResults)
                        {
                            writer.WriteLine($"\"{book.ISBN}\",\"{book.Title}\",\"{book.Author}\",\"{book.Publisher}\",{book.PublicationYear},\"{book.Category?.Name}\",{book.CopiesAvailable}");
                        }
                        
                        writer.WriteLine();
                    }
                    
                    if (SearchMembers && MemberResults.Count > 0)
                    {
                        writer.WriteLine("Member Results");
                        writer.WriteLine("ID,Name,Email,Phone,Registration Date,Status");
                        
                        foreach (var member in MemberResults)
                        {
                            string status = member.IsActive ? "Active" : "Inactive";
                            writer.WriteLine($"{member.Id},\"{member.FullName}\",\"{member.Email}\",\"{member.Phone}\",{member.RegistrationDate:yyyy-MM-dd},{status}");
                        }
                    }
                }
                
                System.Windows.MessageBox.Show($"Results exported to {filePath}", "Export Successful", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error exporting results: {ex.Message}", "Export Failed", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
    }
}