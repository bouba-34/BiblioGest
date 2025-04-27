using BiblioGest.Models;
using System;
using System.Windows.Input;
using BiblioGest.Data;

namespace BiblioGest.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly BiblioGestDbContext _context;
        private ViewModelBase _currentViewModel;
        private bool _isLoggedIn;
        private User _currentUser;
        private string _statusMessage;

        public ViewModelBase CurrentViewModel
        {
            get => _currentViewModel;
            set => SetProperty(ref _currentViewModel, value);
        }

        public bool IsLoggedIn
        {
            get => _isLoggedIn;
            set => SetProperty(ref _isLoggedIn, value);
        }

        public User CurrentUser
        {
            get => _currentUser;
            set
            {
                if (SetProperty(ref _currentUser, value))
                {
                    OnPropertyChanged(nameof(IsAdmin));
                }
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public bool IsAdmin => CurrentUser?.IsAdmin ?? false;

        public ICommand NavigateToHomeCommand { get; private set; }
        public ICommand NavigateToBooksCommand { get; private set; }
        public ICommand NavigateToMembersCommand { get; private set; }
        public ICommand NavigateToLoansCommand { get; private set; }
        public ICommand NavigateToSearchCommand { get; private set; }
        public ICommand NavigateToUserAdminCommand { get; private set; }
        public ICommand LogoutCommand { get; private set; }

        public MainViewModel()
        {
            _context = new BiblioGestDbContext();
            
            // Initialize database if it doesn't exist
            _context.Database.EnsureCreated();

            // Initialize navigation commands
            NavigateToHomeCommand = new RelayCommand(NavigateToHome);
            NavigateToBooksCommand = new RelayCommand(NavigateToBooks);
            NavigateToMembersCommand = new RelayCommand(NavigateToMembers);
            NavigateToLoansCommand = new RelayCommand(NavigateToLoans);
            NavigateToSearchCommand = new RelayCommand(NavigateToSearch);
            NavigateToUserAdminCommand = new RelayCommand(NavigateToUserAdmin, CanNavigateToUserAdmin);
            LogoutCommand = new RelayCommand(Logout);

            // Start with login screen
            CurrentViewModel = new LoginViewModel(this);
            IsLoggedIn = false;
        }

        private void NavigateToHome(object parameter)
        {
            CurrentViewModel = new DashboardViewModel(_context);
        }

        private void NavigateToBooks(object parameter)
        {
            CurrentViewModel = new BookViewModel(_context);
        }

        private void NavigateToMembers(object parameter)
        {
            CurrentViewModel = new MemberViewModel(_context);
        }

        private void NavigateToLoans(object parameter)
        {
            CurrentViewModel = new LoanViewModel(_context);
        }

        private void NavigateToSearch(object parameter)
        {
            CurrentViewModel = new SearchViewModel(_context);
        }

        private bool CanNavigateToUserAdmin(object parameter)
        {
            return IsAdmin;
        }

        private void NavigateToUserAdmin(object parameter)
        {
            if (IsAdmin)
            {
                CurrentViewModel = new UserAdminViewModel(_context);
            }
        }

        private void Logout(object parameter)
        {
            CurrentViewModel = new LoginViewModel(this);
            IsLoggedIn = false;
            CurrentUser = null;
            StatusMessage = string.Empty;
        }

        public void Login(User user)
        {
            if (user == null)
                return;
                
            IsLoggedIn = true;
            CurrentUser = user;
            StatusMessage = $"Connecté en tant que {user.FullName ?? user.Username} | {DateTime.Now:dd/MM/yyyy HH:mm}";
            
            // Rediriger vers le tableau de bord
            NavigateToHome(null);
        }
    }
}