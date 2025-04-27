using BiblioGest.Models;
using BiblioGest.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BiblioGest.Data;
using BiblioGest.Views;

namespace BiblioGest.ViewModels
{
    public class UserAdminViewModel : ViewModelBase
    {
        private readonly BiblioGestDbContext _context;
        private readonly AuthenticationService _authService;
        private ObservableCollection<User> _users;
        private User _selectedUser;
        private User _newUser;
        private string _searchText;
        private bool _isEditMode;
        private bool _isAddMode;
        private bool _isBusy;

        public ObservableCollection<User> Users
        {
            get => _users;
            set => SetProperty(ref _users, value);
        }

        public User SelectedUser
        {
            get => _selectedUser;
            set
            {
                SetProperty(ref _selectedUser, value);
                IsEditMode = value != null;
                
                if (value != null && IsEditMode)
                {
                    // Create a copy for editing to avoid changing the original until save
                    NewUser = new User
                    {
                        Id = value.Id,
                        Username = value.Username,
                        FullName = value.FullName,
                        Email = value.Email,
                        IsAdmin = value.IsAdmin,
                        IsActive = value.IsActive,
                        CreatedDate = value.CreatedDate,
                        LastLoginDate = value.LastLoginDate
                    };
                }
            }
        }

        public User NewUser
        {
            get => _newUser;
            set => SetProperty(ref _newUser, value);
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                SetProperty(ref _searchText, value);
                SearchUsers();
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

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public ICommand AddUserCommand { get; private set; }
        public ICommand SaveUserCommand { get; private set; }
        public ICommand DeleteUserCommand { get; private set; }
        public ICommand ResetPasswordCommand { get; private set; }
        public ICommand ToggleUserStatusCommand { get; private set; }
        public ICommand CancelEditCommand { get; private set; }

        public UserAdminViewModel(BiblioGestDbContext context)
        {
            _context = context;
            _authService = new AuthenticationService(context);
            
            // Initialize commands
            AddUserCommand = new RelayCommand(AddUser);
            SaveUserCommand = new RelayCommand(async param => await SaveUserAsync(param), CanSaveUser);
            DeleteUserCommand = new RelayCommand(async param => await DeleteUserAsync(param), CanManageUser);
            ResetPasswordCommand = new RelayCommand(async param => await ResetPasswordAsync(param), CanManageUser);
            ToggleUserStatusCommand = new RelayCommand(async param => await ToggleUserStatusAsync(param), CanManageUser);
            CancelEditCommand = new RelayCommand(CancelEdit);
            
            // Load data
            LoadUsers();
            
            // Initialize a new user
            NewUser = new User();
        }

        private async void LoadUsers()
        {
            try
            {
                var usersList = await _context.Users.ToListAsync();
                Users = new ObservableCollection<User>(usersList);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des utilisateurs: {ex.Message}", 
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchUsers()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                LoadUsers();
                return;
            }

            string searchLower = SearchText.ToLower();
            var filteredUsers = _context.Users
                .Where(u => 
                    u.Username.ToLower().Contains(searchLower) || 
                    (u.FullName != null && u.FullName.ToLower().Contains(searchLower)) || 
                    (u.Email != null && u.Email.ToLower().Contains(searchLower)))
                .ToList();
            
            Users = new ObservableCollection<User>(filteredUsers);
        }

        private void AddUser(object parameter)
        {
            NewUser = new User
            {
                CreatedDate = DateTime.Now,
                IsActive = true
            };
            IsAddMode = true;
            IsEditMode = false;
        }

        private bool CanSaveUser(object parameter)
        {
            if (IsBusy)
                return false;
                
            if (NewUser == null)
                return false;
                
            // Vérifier que le nom d'utilisateur est renseigné
            if (string.IsNullOrWhiteSpace(NewUser.Username))
                return false;
                
            // En mode ajout, le mot de passe est obligatoire
            if (IsAddMode)
            {
                var passwordBox = parameter as PasswordBox;
                if (passwordBox == null || string.IsNullOrWhiteSpace(passwordBox.Password))
                    return false;
            }
            
            return true;
        }

        private async Task SaveUserAsync(object parameter)
        {
            if (IsBusy || NewUser == null)
                return;
                
            var passwordBox = parameter as PasswordBox;
            if (passwordBox == null)
                return;
                
            string password = passwordBox.Password;
            
            try
            {
                IsBusy = true;
                
                if (IsAddMode)
                {
                    // Vérifier si le nom d'utilisateur existe déjà
                    bool usernameExists = await _context.Users
                        .AnyAsync(u => u.Username.ToLower() == NewUser.Username.ToLower());
                        
                    if (usernameExists)
                    {
                        MessageBox.Show("Ce nom d'utilisateur existe déjà.", "Erreur", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    
                    // Créer un nouvel utilisateur
                    await _authService.CreateUserAsync(
                        NewUser.Username,
                        password,
                        NewUser.FullName,
                        NewUser.Email,
                        NewUser.IsAdmin);
                }
                else if (IsEditMode)
                {
                    // Mettre à jour un utilisateur existant
                    var userToUpdate = await _context.Users.FindAsync(NewUser.Id);
                    if (userToUpdate != null)
                    {
                        userToUpdate.FullName = NewUser.FullName;
                        userToUpdate.Email = NewUser.Email;
                        userToUpdate.IsAdmin = NewUser.IsAdmin;
                        userToUpdate.IsActive = NewUser.IsActive;
                        
                        // Mettre à jour le mot de passe si fourni
                        if (!string.IsNullOrWhiteSpace(password))
                        {
                            userToUpdate.SetPassword(password);
                        }
                        
                        await _context.SaveChangesAsync();
                    }
                }
                
                // Rafraîchir la liste des utilisateurs
                LoadUsers();
                
                // Réinitialiser l'état
                IsAddMode = false;
                IsEditMode = false;
                NewUser = new User();
                
                MessageBox.Show("Utilisateur enregistré avec succès !", "Succès", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'enregistrement de l'utilisateur: {ex.Message}", 
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool CanManageUser(object parameter)
        {
            return !IsBusy && parameter is User user && user != null;
        }

        private async Task DeleteUserAsync(object parameter)
        {
            if (IsBusy || !(parameter is User user))
                return;
                
            var result = MessageBox.Show($"Êtes-vous sûr de vouloir supprimer l'utilisateur '{user.Username}' ?", 
                "Confirmer la suppression", MessageBoxButton.YesNo, MessageBoxImage.Question);
                
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    IsBusy = true;
                    
                    _context.Users.Remove(user);
                    await _context.SaveChangesAsync();
                    
                    // Rafraîchir la liste des utilisateurs
                    LoadUsers();
                    
                    MessageBox.Show("Utilisateur supprimé avec succès !", "Succès", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de la suppression de l'utilisateur: {ex.Message}", 
                        "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }

        private async Task ResetPasswordAsync(object parameter)
        {
            if (IsBusy || !(parameter is User user))
                return;
                
            // Demander le nouveau mot de passe
            var passwordWindow = new PasswordDialog("Entrez le nouveau mot de passe");
            if (passwordWindow.ShowDialog() == true)
            {
                string newPassword = passwordWindow.Password;
                
                if (string.IsNullOrWhiteSpace(newPassword))
                {
                    MessageBox.Show("Le mot de passe ne peut pas être vide.", "Erreur", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                
                try
                {
                    IsBusy = true;
                    
                    await _authService.ResetPasswordAsync(user.Id, newPassword);
                    
                    MessageBox.Show("Mot de passe réinitialisé avec succès !", "Succès", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de la réinitialisation du mot de passe: {ex.Message}", 
                        "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }

        private async Task ToggleUserStatusAsync(object parameter)
        {
            if (IsBusy || !(parameter is User user))
                return;
                
            try
            {
                IsBusy = true;
                
                // Inverser le statut de l'utilisateur
                var userToUpdate = await _context.Users.FindAsync(user.Id);
                if (userToUpdate != null)
                {
                    userToUpdate.IsActive = !userToUpdate.IsActive;
                    await _context.SaveChangesAsync();
                    
                    // Rafraîchir la liste des utilisateurs
                    LoadUsers();
                    
                    string status = userToUpdate.IsActive ? "activé" : "désactivé";
                    MessageBox.Show($"Utilisateur {status} avec succès !", "Succès", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la modification du statut de l'utilisateur: {ex.Message}", 
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void CancelEdit(object parameter)
        {
            IsAddMode = false;
            IsEditMode = false;
            NewUser = new User();
        }
    }
}