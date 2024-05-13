using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;

namespace CustomTextBox
{
    public class ExtendedTextBox : TextBox
    {
        public string TextStringFormat
        {
            get => (string)GetValue(TextStringFormatProperty);
            set => SetValue(TextStringFormatProperty, value);
        }

        public static readonly DependencyProperty TextStringFormatProperty = DependencyProperty.Register(
          "TextStringFormat",
          typeof(string),
          typeof(ExtendedTextBox),
          new PropertyMetadata(default(string), OnTextStringFormatChanged));

        // Monitor TextBox.Text property for changes to reapply the format string
        static ExtendedTextBox() => TextBox.TextProperty.OverrideMetadata(
          typeof(ExtendedTextBox),
          new FrameworkPropertyMetadata(OnTextChanged));

        public ExtendedTextBox()
          => this.Loaded += OnLoaded;

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
          => ((ExtendedTextBox)d).ApplyTextPropertyFormatString();

        private static void OnTextStringFormatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var extendedTextBox = (ExtendedTextBox)d;
            string oldFormatString = (string)e.OldValue;
            string newFormatString = (string)e.NewValue;
            if (!string.IsNullOrWhiteSpace(newFormatString))
            {
                extendedTextBox.ApplyTextPropertyFormatString();
            }
            else if (!string.IsNullOrWhiteSpace(oldFormatString))
            {
                extendedTextBox.ClearTextPropertyFormatString();
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
          => ApplyTextPropertyFormatString();

        private void ApplyTextPropertyFormatString()
        {
            if (string.IsNullOrWhiteSpace(this.TextStringFormat))
            {
                return;
            }

            BindingBase textBinding = BindingOperations.GetBindingBase(this, TextBox.TextProperty);
            if (textBinding is null)
            {
                // No binding attached. 
                // Apply format string directly on the Text property.

                string formattedText;
                if (double.TryParse(this.Text, NumberStyles.None, CultureInfo.CurrentCulture.NumberFormat, out double number))
                {
                    formattedText = string.Format(this.TextStringFormat, number);
                }
                else
                {
                    formattedText = string.Format(this.TextStringFormat, this.Text);
                }

                SetCurrentValue(TextBox.TextProperty, formattedText);
            }
            else
            {
                // Apply format string via Binding.

                textBinding = CloneObject(textBinding);
                textBinding.StringFormat = this.TextStringFormat;
                _ = SetBinding(TextBox.TextProperty, textBinding);
            }
        }

        private void ClearTextPropertyFormatString()
        {
            BindingBase textBinding = BindingOperations.GetBindingBase(this, TextBox.TextProperty);
            if (textBinding is null)
            {
                return;
            }

            textBinding = CloneObject(textBinding);
            if (!string.IsNullOrWhiteSpace(textBinding.StringFormat))
            {
                textBinding.StringFormat = null;
                _ = SetBinding(TextBox.TextProperty, textBinding);
            }
        }

        private TObject CloneObject<TObject>(TObject objectToClone)
        {
            string xamlObject = XamlWriter.Save(objectToClone);
            return (TObject)XamlReader.Parse(xamlObject);
        }
    }
}
