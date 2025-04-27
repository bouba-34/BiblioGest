using BiblioGest.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using BiblioGest.Data;
using Microsoft.EntityFrameworkCore;

namespace BiblioGest.ViewModels
{
    public class DashboardViewModel : ViewModelBase
    {
        private readonly BiblioGestDbContext _context;
        
        private int _totalBooks;
        private int _totalMembers;
        private int _activeLoans;
        private int _overdueLoans;
        private ObservableCollection<Loan> _recentLoans;
        private ObservableCollection<Book> _popularBooks;

        public int TotalBooks
        {
            get => _totalBooks;
            set => SetProperty(ref _totalBooks, value);
        }

        public int TotalMembers
        {
            get => _totalMembers;
            set => SetProperty(ref _totalMembers, value);
        }

        public int ActiveLoans
        {
            get => _activeLoans;
            set => SetProperty(ref _activeLoans, value);
        }

        public int OverdueLoans
        {
            get => _overdueLoans;
            set => SetProperty(ref _overdueLoans, value);
        }

        public ObservableCollection<Loan> RecentLoans
        {
            get => _recentLoans;
            set => SetProperty(ref _recentLoans, value);
        }

        public ObservableCollection<Book> PopularBooks
        {
            get => _popularBooks;
            set => SetProperty(ref _popularBooks, value);
        }

        public DashboardViewModel(BiblioGestDbContext context)
        {
            _context = context;
            LoadDashboardData();
        }

        private void LoadDashboardData()
        {
            TotalBooks = _context.Books.Count();
            TotalMembers = _context.Members.Count();
            
            // Get active loans (not returned)
            var loans = _context.Loans
                .Include(l => l.Book)
                .Include(l => l.Member)
                .Where(l => l.ReturnDate == null)
                .ToList();
            
            ActiveLoans = loans.Count;
            OverdueLoans = loans.Count(l => l.IsOverdue);
            
            // Get recent loans
            RecentLoans = new ObservableCollection<Loan>(
                _context.Loans
                .Include(l => l.Book)
                .Include(l => l.Member)
                .OrderByDescending(l => l.LoanDate)
                .Take(10)
                .ToList()
            );
            
            // Get most popular books (based on number of loans)
            PopularBooks = new ObservableCollection<Book>(
                _context.Books
                .Include(b => b.Loans)
                .OrderByDescending(b => b.Loans.Count)
                .Take(10)
                .ToList()
            );
        }
    }
}