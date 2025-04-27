using BiblioGest.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace BiblioGest.Views
{
    /// <summary>
    /// Interaction logic for LoginView.xaml
    /// </summary>
    public partial class LoginView : UserControl
    {
        public LoginView()
        {
            InitializeComponent();
            this.Loaded += LoginView_Loaded;
        }

        private void LoginView_Loaded(object sender, RoutedEventArgs e)
        {
            // Handle password securely
            var viewModel = this.DataContext as LoginViewModel;
            if (viewModel != null)
            {
                // Clear password box when view is loaded
                PasswordBox.Password = string.Empty;
                
                // Bind password box directly
                PasswordBox.PasswordChanged += (s, args) => {
                    viewModel.Password = PasswordBox.Password;
                };
                
                // Set focus to the username if it's empty
                if (string.IsNullOrEmpty(viewModel.Username))
                {
                    Dispatcher.BeginInvoke(new Action(() => {
                        var textBox = FindVisualChild<TextBox>(this);
                        if (textBox != null)
                        {
                            textBox.Focus();
                        }
                    }));
                }
                else
                {
                    // Otherwise focus on password
                    Dispatcher.BeginInvoke(new Action(() => {
                        PasswordBox.Focus();
                    }));
                }
            }
        }

        // Helper method to find a child control
        private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child != null && child is T)
                    return (T)child;
                
                var result = FindVisualChild<T>(child);
                if (result != null)
                    return result;
            }
            return null;
        }
    }

    // Converter to show/hide elements based on string emptiness
    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return string.IsNullOrEmpty(value as string) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}