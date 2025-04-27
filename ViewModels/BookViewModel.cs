using BiblioGest.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using BiblioGest.Data;

namespace BiblioGest.ViewModels
{
    public class BookViewModel : ViewModelBase
    {
        private readonly BiblioGestDbContext _context;
        private ObservableCollection<Book> _books;
        private ObservableCollection<Category> _categories;
        private Book _selectedBook;
        private Book _newBook;
        private string _searchText;
        private bool _isEditMode;
        private bool _isAddMode;

        public ObservableCollection<Book> Books
        {
            get => _books;
            set => SetProperty(ref _books, value);
        }

        public ObservableCollection<Category> Categories
        {
            get => _categories;
            set => SetProperty(ref _categories, value);
        }

        public Book SelectedBook
        {
            get => _selectedBook;
            set
            {
                SetProperty(ref _selectedBook, value);
                IsEditMode = value != null;
                
                if (value != null && IsEditMode)
                {
                    // Create a copy for editing to avoid changing the original until save
                    NewBook = new Book
                    {
                        Id = value.Id,
                        ISBN = value.ISBN,
                        Title = value.Title,
                        Author = value.Author,
                        Publisher = value.Publisher,
                        PublicationYear = value.PublicationYear,
                        CategoryId = value.CategoryId,
                        CopiesAvailable = value.CopiesAvailable
                    };
                }
            }
        }

        public Book NewBook
        {
            get => _newBook;
            set => SetProperty(ref _newBook, value);
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                SetProperty(ref _searchText, value);
                SearchBooks();
            }
        }

        public bool IsEditMode
        {
            get => _isEditMode;
            set => SetProperty(ref _isEditMode, value);
        }

        public bool IsAddMode
        {
            get => _isAddMode;
            set => SetProperty(ref _isAddMode, value);
        }

        public ICommand AddBookCommand { get; private set; }
        public ICommand SaveBookCommand { get; private set; }
        public ICommand DeleteBookCommand { get; private set; }
        public ICommand CancelEditCommand { get; private set; }

        public BookViewModel(BiblioGestDbContext context)
        {
            _context = context;
            
            // Initialize commands
            AddBookCommand = new RelayCommand(AddBook);
            SaveBookCommand = new RelayCommand(SaveBook, CanSaveBook);
            DeleteBookCommand = new RelayCommand(DeleteBook, CanDeleteBook);
            CancelEditCommand = new RelayCommand(CancelEdit);
            
            // Load data
            LoadBooks();
            LoadCategories();
            
            // Initialize a new book
            NewBook = new Book { PublicationYear = DateTime.Now.Year, CopiesAvailable = 1 };
        }

        private void LoadBooks()
        {
            var booksList = _context.Books
                .Include(b => b.Category)
                .ToList();
            
            Books = new ObservableCollection<Book>(booksList);
        }

        private void LoadCategories()
        {
            var categoriesList = _context.Categories.ToList();
            Categories = new ObservableCollection<Category>(categoriesList);
        }

        private void SearchBooks()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                LoadBooks();
                return;
            }

            string searchLower = SearchText.ToLower();
            var filteredBooks = _context.Books
                .Include(b => b.Category)
                .Where(b => 
                    b.Title.ToLower().Contains(searchLower) || 
                    b.Author.ToLower().Contains(searchLower) || 
                    b.ISBN.ToLower().Contains(searchLower) ||
                    b.Publisher.ToLower().Contains(searchLower))
                .ToList();
            
            Books = new ObservableCollection<Book>(filteredBooks);
        }

        private void AddBook(object parameter)
        {
            NewBook = new Book { PublicationYear = DateTime.Now.Year, CopiesAvailable = 1 };
            IsAddMode = true;
            IsEditMode = false;
        }

        private bool CanSaveBook(object parameter)
        {
            return NewBook != null && 
                   !string.IsNullOrWhiteSpace(NewBook.Title) && 
                   !string.IsNullOrWhiteSpace(NewBook.Author) && 
                   !string.IsNullOrWhiteSpace(NewBook.ISBN) &&
                   NewBook.CategoryId > 0;
        }

        private void SaveBook(object parameter)
        {
            try
            {
                if (IsAddMode)
                {
                    _context.Books.Add(NewBook);
                }
                else if (IsEditMode)
                {
                    var bookToUpdate = _context.Books.Find(NewBook.Id);
                    if (bookToUpdate != null)
                    {
                        bookToUpdate.ISBN = NewBook.ISBN;
                        bookToUpdate.Title = NewBook.Title;
                        bookToUpdate.Author = NewBook.Author;
                        bookToUpdate.Publisher = NewBook.Publisher;
                        bookToUpdate.PublicationYear = NewBook.PublicationYear;
                        bookToUpdate.CategoryId = NewBook.CategoryId;
                        bookToUpdate.CopiesAvailable = NewBook.CopiesAvailable;
                    }
                }

                _context.SaveChanges();
                
                // Refresh book list
                LoadBooks();
                
                // Reset state
                IsAddMode = false;
                IsEditMode = false;
                NewBook = new Book { PublicationYear = DateTime.Now.Year, CopiesAvailable = 1 };
                
                MessageBox.Show("Book saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving book: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanDeleteBook(object parameter)
        {
            return SelectedBook != null;
        }

        private void DeleteBook(object parameter)
        {
            if (SelectedBook == null) return;

            var result = MessageBox.Show($"Are you sure you want to delete '{SelectedBook.Title}'?", 
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // Check if the book has loans
                    var hasLoans = _context.Loans.Any(l => l.BookId == SelectedBook.Id);
                    if (hasLoans)
                    {
                        MessageBox.Show("Cannot delete this book because it has associated loans.", 
                            "Delete Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    _context.Books.Remove(SelectedBook);
                    _context.SaveChanges();
                    
                    // Refresh book list
                    LoadBooks();
                    
                    IsEditMode = false;
                    
                    MessageBox.Show("Book deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting book: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CancelEdit(object parameter)
        {
            IsAddMode = false;
            IsEditMode = false;
            NewBook = new Book { PublicationYear = DateTime.Now.Year, CopiesAvailable = 1 };
        }
    }
}