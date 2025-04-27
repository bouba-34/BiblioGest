using BiblioGest.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using BiblioGest.Data;

namespace BiblioGest.Services
{
    public class AuthenticationService
    {
        private readonly BiblioGestDbContext _context;

        public AuthenticationService(BiblioGestDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Authentifie un utilisateur avec son nom d'utilisateur et mot de passe
        /// </summary>
        /// <param name="username">Nom d'utilisateur</param>
        /// <param name="password">Mot de passe</param>
        /// <returns>L'utilisateur authentifié, ou null si l'authentification échoue</returns>
        public async Task<User> AuthenticateAsync(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return null;

            // Rechercher l'utilisateur par son nom d'utilisateur
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower() && u.IsActive);

            // Vérifier si l'utilisateur existe et si le mot de passe est correct
            if (user != null && user.VerifyPassword(password))
            {
                // Mettre à jour la date de dernière connexion
                user.LastLoginDate = DateTime.Now;
                await _context.SaveChangesAsync();
                
                return user;
            }

            return null;
        }

        /// <summary>
        /// Crée un nouvel utilisateur
        /// </summary>
        /// <param name="username">Nom d'utilisateur</param>
        /// <param name="password">Mot de passe</param>
        /// <param name="fullName">Nom complet</param>
        /// <param name="email">Email</param>
        /// <param name="isAdmin">Indique si l'utilisateur est administrateur</param>
        /// <returns>L'utilisateur créé, ou null si la création échoue</returns>
        public async Task<User> CreateUserAsync(string username, string password, string fullName, string email, bool isAdmin = false)
        {
            // Vérifier si le nom d'utilisateur existe déjà
            bool usernameExists = await _context.Users.AnyAsync(u => u.Username.ToLower() == username.ToLower());
            if (usernameExists)
                return null;

            // Créer le nouvel utilisateur
            var user = new User
            {
                Username = username,
                FullName = fullName,
                Email = email,
                IsAdmin = isAdmin,
                CreatedDate = DateTime.Now,
                IsActive = true
            };

            // Définir le mot de passe
            user.SetPassword(password);

            // Enregistrer dans la base de données
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }

        /// <summary>
        /// Change le mot de passe d'un utilisateur
        /// </summary>
        /// <param name="userId">ID de l'utilisateur</param>
        /// <param name="currentPassword">Mot de passe actuel</param>
        /// <param name="newPassword">Nouveau mot de passe</param>
        /// <returns>True si le changement réussit, false sinon</returns>
        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || !user.VerifyPassword(currentPassword))
                return false;

            user.SetPassword(newPassword);
            await _context.SaveChangesAsync();
            
            return true;
        }

        /// <summary>
        /// Réinitialise le mot de passe d'un utilisateur (fonction administrative)
        /// </summary>
        /// <param name="userId">ID de l'utilisateur</param>
        /// <param name="newPassword">Nouveau mot de passe</param>
        /// <returns>True si la réinitialisation réussit, false sinon</returns>
        public async Task<bool> ResetPasswordAsync(int userId, string newPassword)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;

            user.SetPassword(newPassword);
            await _context.SaveChangesAsync();
            
            return true;
        }

        /// <summary>
        /// Désactive un compte utilisateur
        /// </summary>
        /// <param name="userId">ID de l'utilisateur</param>
        /// <returns>True si la désactivation réussit, false sinon</returns>
        public async Task<bool> DisableUserAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;

            user.IsActive = false;
            await _context.SaveChangesAsync();
            
            return true;
        }

        /// <summary>
        /// Initialise les utilisateurs par défaut si aucun n'existe
        /// </summary>
        public async Task InitializeDefaultUsersAsync()
        {
            // Vérifier si des utilisateurs existent déjà
            if (await _context.Users.AnyAsync())
                return;

            // Créer un utilisateur administrateur par défaut
            await CreateUserAsync("admin", "admin", "Administrateur", "admin@bibliogest.com", true);
            
            // Créer un utilisateur standard par défaut
            await CreateUserAsync("user", "user", "Utilisateur Standard", "user@bibliogest.com", false);
        }
    }
}