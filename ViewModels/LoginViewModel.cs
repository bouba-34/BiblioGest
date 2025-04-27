using BiblioGest.Models;
using BiblioGest.Services;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BiblioGest.Data;

namespace BiblioGest.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly MainViewModel _mainViewModel;
        private readonly AuthenticationService _authService;
        private string _username;
        private string _password;
        private bool _rememberMe;
        private string _errorMessage;
        private bool _isLoggingIn;

        public string Username
        {
            get => _username;
            set
            {
                SetProperty(ref _username, value);
                // Clear error message when user tries again
                if (!string.IsNullOrEmpty(ErrorMessage))
                    ErrorMessage = string.Empty;
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                SetProperty(ref _password, value);
                // Clear error message when user tries again
                if (!string.IsNullOrEmpty(ErrorMessage))
                    ErrorMessage = string.Empty;
            }
        }

        public bool RememberMe
        {
            get => _rememberMe;
            set => SetProperty(ref _rememberMe, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool IsLoggingIn
        {
            get => _isLoggingIn;
            set => SetProperty(ref _isLoggingIn, value);
        }

        public ICommand LoginCommand { get; private set; }

        public LoginViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            _authService = new AuthenticationService(new BiblioGestDbContext());
            
            // Initialize the login command
            LoginCommand = new RelayCommand(async param => await ExecuteLoginAsync(param), CanExecuteLogin);
            
            // Load remembered username if available
            if (Properties.Settings.Default.RememberUsername)
            {
                Username = Properties.Settings.Default.LastUsername;
                RememberMe = true;
            }

            // Initialiser les utilisateurs par défaut si nécessaire
            Task.Run(async () => await _authService.InitializeDefaultUsersAsync());
        }

        private bool CanExecuteLogin(object parameter)
        {
            return !string.IsNullOrWhiteSpace(Username) && 
                   !string.IsNullOrWhiteSpace(Password) && 
                   !IsLoggingIn;
        }

        private async Task ExecuteLoginAsync(object parameter)
        {
            try
            {
                IsLoggingIn = true;
                
                // Authentifier l'utilisateur
                var user = await _authService.AuthenticateAsync(Username, Password);
                
                if (user != null)
                {
                    // Sauvegarder les paramètres si "Se souvenir de moi" est coché
                    Properties.Settings.Default.RememberUsername = RememberMe;
                    if (RememberMe)
                        Properties.Settings.Default.LastUsername = Username;
                    else
                        Properties.Settings.Default.LastUsername = string.Empty;
                    
                    Properties.Settings.Default.Save();
                    
                    // Connexion réussie
                    _mainViewModel.Login(user);
                }
                else
                {
                    ErrorMessage = "Nom d'utilisateur ou mot de passe incorrect. Veuillez réessayer.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erreur lors de la connexion: {ex.Message}";
            }
            finally
            {
                IsLoggingIn = false;
            }
        }
    }
}