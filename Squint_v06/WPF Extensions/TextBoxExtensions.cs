using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SquintScript.Extensions
{
    public class TextBoxExtensions
    {
        public static readonly DependencyProperty UpdateSourceOnKeyProperty = DependencyProperty.RegisterAttached("UpdateSourceOnKey", typeof(Key), typeof(TextBox), new FrameworkPropertyMetadata(Key.None));

        public static void SetUpdateSourceOnKey(UIElement element, Key value)
        {
            element.PreviewKeyUp += TextBoxKeyUp;
            element.SetValue(UpdateSourceOnKeyProperty, value);
        }

        static void TextBoxKeyUp(object sender, KeyEventArgs e)
        {

            var textBox = sender as TextBox;
            if (textBox == null) return;

            var propertyValue = (Key)textBox.GetValue(UpdateSourceOnKeyProperty);
            if (e.Key != propertyValue) return;

            var bindingExpression = textBox.GetBindingExpression(TextBox.TextProperty);
            if (bindingExpression != null)
            {
                try
                {
                    bindingExpression.UpdateSource();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("{0} {1} {2}", ex.Message, ex.InnerException, ex.StackTrace));
                }
            }
        }

        public static Key GetUpdateSourceOnKey(UIElement element)
        {
            return (Key)element.GetValue(UpdateSourceOnKeyProperty);
        }

    }
}
