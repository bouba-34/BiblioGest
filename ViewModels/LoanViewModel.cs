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
    public class LoanViewModel : ViewModelBase
    {
        private readonly BiblioGestDbContext _context;
        private ObservableCollection<Loan> _loans;
        private ObservableCollection<Book> _availableBooks;
        private ObservableCollection<Member> _activeMembers;
        private Loan _selectedLoan;
        private Loan _newLoan;
        private string _searchText;
        private bool _isAddMode;
        private bool _showOnlyActive;
        private bool _showOnlyOverdue;

        public ObservableCollection<Loan> Loans
        {
            get => _loans;
            set => SetProperty(ref _loans, value);
        }

        public ObservableCollection<Book> AvailableBooks
        {
            get => _availableBooks;
            set => SetProperty(ref _availableBooks, value);
        }

        public ObservableCollection<Member> ActiveMembers
        {
            get => _activeMembers;
            set => SetProperty(ref _activeMembers, value);
        }

        public Loan SelectedLoan
        {
            get => _selectedLoan;
            set => SetProperty(ref _selectedLoan, value);
        }

        public Loan NewLoan
        {
            get => _newLoan;
            set => SetProperty(ref _newLoan, value);
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                SetProperty(ref _searchText, value);
                SearchLoans();
            }
        }

        public bool IsAddMode
        {
            get => _isAddMode;
            set => SetProperty(ref _isAddMode, value);
        }

        public bool ShowOnlyActive
        {
            get => _showOnlyActive;
            set
            {
                SetProperty(ref _showOnlyActive, value);
                FilterLoans();
            }
        }

        public bool ShowOnlyOverdue
        {
            get => _showOnlyOverdue;
            set
            {
                SetProperty(ref _showOnlyOverdue, value);
                FilterLoans();
            }
        }

        public ICommand AddLoanCommand { get; private set; }
        public ICommand CreateLoanCommand { get; private set; }
        public ICommand ReturnBookCommand { get; private set; }
        public ICommand CancelAddCommand { get; private set; }
        public ICommand ExtendLoanCommand { get; private set; }

        public LoanViewModel(BiblioGestDbContext context)
        {
            _context = context;
            
            // Initialize commands
            AddLoanCommand = new RelayCommand(AddLoan);
            CreateLoanCommand = new RelayCommand(CreateLoan, CanCreateLoan);
            ReturnBookCommand = new RelayCommand(ReturnBook, CanReturnBook);
            CancelAddCommand = new RelayCommand(CancelAdd);
            ExtendLoanCommand = new RelayCommand(ExtendLoan, CanExtendLoan);
            
            // Set default filter
            ShowOnlyActive = true;
            ShowOnlyOverdue = false;
            
            // Load data
            LoadLoans();
            LoadAvailableBooks();
            LoadActiveMembers();
            
            // Initialize a new loan
            NewLoan = new Loan();
        }

        private void LoadLoans()
        {
            var loansList = _context.Loans
                .Include(l => l.Book)
                .Include(l => l.Member)
                .ToList();
            
            Loans = new ObservableCollection<Loan>(loansList);
            
            FilterLoans();
        }

        private void LoadAvailableBooks()
        {
            // Get books with available copies
            var booksList = _context.Books
                .Where(b => b.CopiesAvailable > 0)
                .ToList();
            
            AvailableBooks = new ObservableCollection<Book>(booksList);
        }

        private void LoadActiveMembers()
        {
            // Get only active members
            var membersList = _context.Members
                .Where(m => m.IsActive)
                .ToList();
            
            ActiveMembers = new ObservableCollection<Member>(membersList);
        }

        private void FilterLoans()
        {
            var query = _context.Loans
                .Include(l => l.Book)
                .Include(l => l.Member)
                .AsQueryable();
            
            if (ShowOnlyActive)
            {
                query = query.Where(l => l.ReturnDate == null);
            }
            
            if (ShowOnlyOverdue)
            {
                query = query.Where(l => l.ReturnDate == null && l.DueDate < DateTime.Now);
            }
            
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                string searchLower = SearchText.ToLower();
                query = query.Where(l => 
                    l.Book.Title.ToLower().Contains(searchLower) ||
                    l.Book.ISBN.ToLower().Contains(searchLower) ||
                    l.Member.FirstName.ToLower().Contains(searchLower) ||
                    l.Member.LastName.ToLower().Contains(searchLower));
            }
            
            var filteredLoans = query.OrderByDescending(l => l.LoanDate).ToList();
            Loans = new ObservableCollection<Loan>(filteredLoans);
        }

        private void SearchLoans()
        {
            FilterLoans();
        }

        private void AddLoan(object parameter)
        {
            NewLoan = new Loan();
            IsAddMode = true;
        }

        private bool CanCreateLoan(object parameter)
        {
            return NewLoan != null && NewLoan.BookId > 0 && NewLoan.MemberId > 0;
        }

        private void CreateLoan(object parameter)
        {
            try
            {
                // Check if the book is available
                var book = _context.Books.Find(NewLoan.BookId);
                if (book.CopiesAvailable <= 0)
                {
                    MessageBox.Show("This book is not available for loan!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                
                // Check if the member is active
                var member = _context.Members.Find(NewLoan.MemberId);
                if (!member.IsActive)
                {
                    MessageBox.Show("This member is not active!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                
                // Create the loan
                NewLoan.LoanDate = DateTime.Now;
                NewLoan.DueDate = DateTime.Now.AddDays(14); // Default loan period: 14 days
                
                _context.Loans.Add(NewLoan);
                
                // Update book availability
                book.CopiesAvailable--;
                
                _context.SaveChanges();
                
                // Refresh data
                LoadLoans();
                LoadAvailableBooks();
                
                // Reset state
                IsAddMode = false;
                NewLoan = new Loan();
                
                MessageBox.Show("Loan created successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating loan: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanReturnBook(object parameter)
        {
            return SelectedLoan != null && SelectedLoan.ReturnDate == null;
        }

        private void ReturnBook(object parameter)
        {
            if (SelectedLoan == null || SelectedLoan.ReturnDate != null) return;

            try
            {
                // Update the loan
                var loan = _context.Loans.Find(SelectedLoan.Id);
                loan.ReturnDate = DateTime.Now;
                
                // Update book availability
                var book = _context.Books.Find(loan.BookId);
                book.CopiesAvailable++;
                
                _context.SaveChanges();
                
                // Refresh data
                LoadLoans();
                LoadAvailableBooks();
                
                MessageBox.Show("Book returned successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error returning book: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelAdd(object parameter)
        {
            IsAddMode = false;
            NewLoan = new Loan();
        }

        private bool CanExtendLoan(object parameter)
        {
            return SelectedLoan != null && SelectedLoan.ReturnDate == null;
        }

        private void ExtendLoan(object parameter)
        {
            if (SelectedLoan == null || SelectedLoan.ReturnDate != null) return;

            try
            {
                // Extend the loan by 7 days
                var loan = _context.Loans.Find(SelectedLoan.Id);
                loan.DueDate = loan.DueDate.AddDays(7);
                
                _context.SaveChanges();
                
                // Refresh data
                LoadLoans();
                
                MessageBox.Show("Loan extended successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error extending loan: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}