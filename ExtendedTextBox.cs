using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;

namespace CustomTextBox
{
    public class ExtendedTextBox : TextBox
    {
        #region TextStringFormat
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

        private static void OnTextStringFormatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var extendedTextBox = (ExtendedTextBox)d;
            string oldFormatString = (string)e.OldValue;
            string newFormatString = (string)e.NewValue;
            if (!string.IsNullOrWhiteSpace(newFormatString))
            {
                extendedTextBox.ApplyBindingStringFormat();
            }
            else if (!string.IsNullOrWhiteSpace(oldFormatString))
            {
                extendedTextBox.ClearTextPropertyFormat();
            }
        }

        private bool isFormattingTextProperty;

        public ExtendedTextBox()
        {
            this.Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
          => FormatTextProperty();

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            if (this.isFormattingTextProperty)
            {
                return;
            }

            base.OnTextChanged(e);

            BindingBase textBinding = BindingOperations.GetBindingBase(this, TextBox.TextProperty);
            if (textBinding is null)
            {
                // No binding attached. Format numeric text directly.
                FormatTextProperty();
            }
        }

        private void FormatTextProperty()
        {
            if (string.IsNullOrWhiteSpace(this.TextStringFormat))
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(this.Text))
            {
                string formattedText;
                if (double.TryParse(this.Text, NumberStyles.None, CultureInfo.CurrentCulture.NumberFormat, out double number))
                {
                    formattedText = string.Format(CultureInfo.CurrentCulture, this.TextStringFormat, number);
                }
                else
                {
                    formattedText = string.Format(CultureInfo.CurrentCulture, this.TextStringFormat, this.Text);
                }

                this.isFormattingTextProperty = true;
                int currentCaretIndex = this.CaretIndex;
                SetCurrentValue(TextBox.TextProperty, formattedText);
                this.CaretIndex = currentCaretIndex;
                this.isFormattingTextProperty = false;
            }
        }

        // Binding attached. Apply formatting via data binding.
        private void ApplyBindingStringFormat()
        {
            BindingBase textBinding = BindingOperations.GetBindingBase(this, TextBox.TextProperty);
            if (textBinding is null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(textBinding.StringFormat))
            {
                textBinding = CloneObject(textBinding);
                textBinding.StringFormat = this.TextStringFormat;
                _ = SetBinding(TextBox.TextProperty, textBinding);
            }
        }

        private void ClearTextPropertyFormat()
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
        #endregion

        #region editing mode + select all value signs + save on enter
        private const char WRONG_NUMBER_DECIMAL_SEPARATOR = ',';

        public bool IsEditing
        {
            get => (bool)GetValue(IsEditingProperty);
            set => SetValue(IsEditingProperty, value);
        }

        public static readonly DependencyProperty IsEditingProperty = DependencyProperty.Register(
            "IsEditing",
            typeof(bool),
            typeof(ExtendedTextBox));

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            if (TextProperty != null && e.Key == Key.Enter)
            {
                ValidateDecimalValue();
                GetBindingExpression(TextProperty)?.UpdateSource();
                SelectAll();
            }

            SetIsEditing();
        }

        protected override void OnTextInput(TextCompositionEventArgs e)
        {
            base.OnTextInput(e);
            SetIsEditing();
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            ValidateDecimalValue();
            base.OnLostFocus(e);
            SetIsEditing();
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            SelectAll();
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
            Focus();
            SelectAll();
        }

        private void ValidateDecimalValue()
        {
            var currValue = Text.Replace(WRONG_NUMBER_DECIMAL_SEPARATOR, char.Parse(CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator));
            if (currValue != Text && double.TryParse(currValue, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out double result))
            {
                Text = result.ToString(CultureInfo.InvariantCulture);
            }
        }

        private void SetIsEditing()
        {
            var binding = GetBindingExpression(TextProperty);
            IsEditing = binding == null ? false : binding.IsDirty;
        }
        #endregion
    }
}
