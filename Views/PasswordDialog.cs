using System.Windows;
using System.Windows.Controls;

namespace BiblioGest.Views
{
    /// <summary>
    /// Boîte de dialogue pour saisir un mot de passe
    /// </summary>
    public class PasswordDialog : Window
    {
        private PasswordBox passwordBox;
        
        public string Password { get; private set; }
        
        public PasswordDialog(string message)
        {
            // Configurer la fenêtre
            Title = "Mot de passe";
            Width = 350;
            Height = 200;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.NoResize;
            WindowStyle = WindowStyle.ToolWindow;
            
            // Créer la mise en page
            var grid = new Grid { Margin = new Thickness(15) };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            
            // Message
            var messageTextBlock = new TextBlock
            {
                Text = message,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 10)
            };
            Grid.SetRow(messageTextBlock, 0);
            
            // Étiquette du mot de passe
            var passwordLabel = new TextBlock
            {
                Text = "Mot de passe:",
                Margin = new Thickness(0, 10, 0, 5)
            };
            Grid.SetRow(passwordLabel, 1);
            
            // Champ de mot de passe
            passwordBox = new PasswordBox
            {
                Margin = new Thickness(0, 0, 0, 10)
            };
            Grid.SetRow(passwordBox, 2);
            
            // Panneau de boutons
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            Grid.SetRow(buttonPanel, 3);
            
            // Bouton OK
            var okButton = new Button
            {
                Content = "OK",
                IsDefault = true,
                MinWidth = 80,
                Margin = new Thickness(0, 0, 10, 0),
                Padding = new Thickness(10, 5, 10, 5)
            };
            okButton.Click += (sender, e) => 
            {
                Password = passwordBox.Password;
                DialogResult = true;
                Close();
            };
            
            // Bouton Annuler
            var cancelButton = new Button
            {
                Content = "Annuler",
                IsCancel = true,
                MinWidth = 80,
                Padding = new Thickness(10, 5, 10, 5)
            };
            cancelButton.Click += (sender, e) => 
            {
                DialogResult = false;
                Close();
            };
            
            // Ajouter les boutons au panneau
            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);
            
            // Ajouter les éléments à la grille
            grid.Children.Add(messageTextBlock);
            grid.Children.Add(passwordLabel);
            grid.Children.Add(passwordBox);
            grid.Children.Add(buttonPanel);
            
            // Définir le contenu de la fenêtre
            Content = grid;
            
            // Donner le focus au champ de mot de passe
            Loaded += (sender, e) => passwordBox.Focus();
        }
    }
}