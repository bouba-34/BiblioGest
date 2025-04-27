using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BiblioGest
{
    public static class PlaceholderTextExtension
    {
        public static readonly DependencyProperty PlaceholderTextProperty =
            DependencyProperty.RegisterAttached(
                "PlaceholderText",
                typeof(string),
                typeof(PlaceholderTextExtension),
                new PropertyMetadata(string.Empty, OnPlaceholderTextChanged));

        public static string GetPlaceholderText(DependencyObject obj)
        {
            return (string)obj.GetValue(PlaceholderTextProperty);
        }

        public static void SetPlaceholderText(DependencyObject obj, string value)
        {
            obj.SetValue(PlaceholderTextProperty, value);
        }

        private static void OnPlaceholderTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textBox)
            {
                // Remove existing handlers to avoid memory leaks
                textBox.GotFocus -= TextBox_GotFocus;
                textBox.LostFocus -= TextBox_LostFocus;
                textBox.TextChanged -= TextBox_TextChanged;

                if (!string.IsNullOrEmpty(e.NewValue as string))
                {
                    // Add event handlers
                    textBox.GotFocus += TextBox_GotFocus;
                    textBox.LostFocus += TextBox_LostFocus;
                    textBox.TextChanged += TextBox_TextChanged;

                    // Set initial state
                    UpdatePlaceholderState(textBox);
                }
            }
        }

        private static void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox)sender;
            if (textBox.Text == GetPlaceholderText(textBox))
            {
                textBox.Text = string.Empty;
                textBox.Foreground = Brushes.Black;
            }
        }

        private static void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdatePlaceholderState((TextBox)sender);
        }

        private static void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = (TextBox)sender;
            if (textBox.IsFocused && textBox.Text != GetPlaceholderText(textBox))
            {
                textBox.Foreground = Brushes.Black;
            }
        }

        private static void UpdatePlaceholderState(TextBox textBox)
        {
            if (string.IsNullOrEmpty(textBox.Text))
            {
                textBox.Text = GetPlaceholderText(textBox);
                textBox.Foreground = Brushes.Gray;
            }
        }
    }
}