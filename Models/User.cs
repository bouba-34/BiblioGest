using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;

namespace BiblioGest.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Username { get; set; }
        
        [Required]
        [StringLength(100)]
        public string PasswordHash { get; set; }
        
        [StringLength(100)]
        public string Salt { get; set; }
        
        [StringLength(100)]
        public string FullName { get; set; }
        
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }
        
        public bool IsAdmin { get; set; }
        
        public DateTime CreatedDate { get; set; }
        
        public DateTime? LastLoginDate { get; set; }
        
        public bool IsActive { get; set; }

        public User()
        {
            CreatedDate = DateTime.Now;
            IsActive = true;
        }

        /// <summary>
        /// Vérifie si le mot de passe fourni correspond au hash stocké
        /// </summary>
        /// <param name="password">Mot de passe en clair à vérifier</param>
        /// <returns>True si le mot de passe est correct, sinon false</returns>
        public bool VerifyPassword(string password)
        {
            // Générer le hash du mot de passe avec le sel stocké
            string computedHash = ComputeHash(password, Salt);
            
            // Comparer avec le hash stocké
            return computedHash == PasswordHash;
        }

        /// <summary>
        /// Définit un nouveau mot de passe pour l'utilisateur
        /// </summary>
        /// <param name="password">Nouveau mot de passe en clair</param>
        public void SetPassword(string password)
        {
            // Générer un nouveau sel aléatoire
            Salt = GenerateSalt();
            
            // Calculer le hash du mot de passe avec le sel
            PasswordHash = ComputeHash(password, Salt);
        }

        /// <summary>
        /// Génère un sel aléatoire pour le hachage du mot de passe
        /// </summary>
        /// <returns>Chaîne de sel aléatoire</returns>
        private string GenerateSalt()
        {
            byte[] saltBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }
            return Convert.ToBase64String(saltBytes);
        }

        /// <summary>
        /// Calcule le hash d'un mot de passe avec un sel donné
        /// </summary>
        /// <param name="password">Mot de passe en clair</param>
        /// <param name="salt">Sel à utiliser</param>
        /// <returns>Hash du mot de passe</returns>
        private string ComputeHash(string password, string salt)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(salt))
                return string.Empty;

            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] saltBytes = Convert.FromBase64String(salt);
            
            // Combiner le mot de passe et le sel
            byte[] combinedBytes = new byte[passwordBytes.Length + saltBytes.Length];
            Buffer.BlockCopy(passwordBytes, 0, combinedBytes, 0, passwordBytes.Length);
            Buffer.BlockCopy(saltBytes, 0, combinedBytes, passwordBytes.Length, saltBytes.Length);
            
            // Calculer le hash avec SHA256
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(combinedBytes);
                return Convert.ToBase64String(hashBytes);
            }
        }
    }
}