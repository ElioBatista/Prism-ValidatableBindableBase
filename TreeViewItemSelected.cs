using Microsoft.Practices.Unity;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Apollo.Infrastructure.Prism
{
    public class TreeViewItemSelected
    {
        private static readonly DependencyProperty SelectedCommandBehaviorProperty =
            DependencyProperty.RegisterAttached("SelectedCommandBehavior", typeof(TreeViewCommandBehavior), typeof(TreeViewItemSelected), null);

        public static readonly DependencyProperty CommandProperty = 
            DependencyProperty.RegisterAttached("Command", typeof(ICommand), typeof(TreeViewItemSelected), new PropertyMetadata(OnSetCommandCallback));
        public static ICommand GetCommand(TreeView menuItem)
        {
            return menuItem.GetValue(CommandProperty) as ICommand;
        }
        public static void SetCommand(TreeView menuItem, ICommand command)
        {
            menuItem.SetValue(CommandProperty, command);
        }

        private static void OnSetCommandCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            TreeView menuItem = dependencyObject as TreeView;
            if (menuItem != null)
            {
                TreeViewCommandBehavior behavior = GetOrCreateBehavior(menuItem);
                behavior.Command = e.NewValue as ICommand;
            }
        }

        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.RegisterAttached("CommandParameter", typeof(object), typeof(TreeViewItemSelected), new PropertyMetadata(OnSetCommandParameterCallback));
        public static object GetCommandParameter(TreeView menuItem)
        {
            return menuItem.GetValue(CommandParameterProperty);
        }
        public static void SetCommandParameter(TreeView menuItem, object parameter)
        {
            menuItem.SetValue(CommandParameterProperty, parameter);
        }

        private static void OnSetCommandParameterCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            TreeView menuItem = dependencyObject as TreeView;
            if (menuItem != null)
            {
                TreeViewCommandBehavior behavior = GetOrCreateBehavior(menuItem);
                behavior.CommandParameter = e.NewValue;
            }
        }

        private static TreeViewCommandBehavior GetOrCreateBehavior(TreeView menuItem)
        {
            TreeViewCommandBehavior behavior = menuItem.GetValue(SelectedCommandBehaviorProperty) as TreeViewCommandBehavior;
            if (behavior == null)
            {
                behavior = new TreeViewCommandBehavior(menuItem);
                menuItem.SetValue(SelectedCommandBehaviorProperty, behavior);
            }

            return behavior;
        }
    }
}
