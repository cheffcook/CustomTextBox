using System.Windows.Controls;
using System.Windows.Input;

namespace CustomTextBox
{
    public class ExtendedTextBox : TextBox
    {
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (TextProperty != null && e.Key == Key.Enter) 
            {
                GetBindingExpression(TextProperty)?.UpdateSource();
            }
        }
    }
}
