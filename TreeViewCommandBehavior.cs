using Apollo.SM.Core;
using Prism.Interactivity;
using System.Windows;
using System.Windows.Controls;

namespace Apollo.Infrastructure.Prism
{
    public class TreeViewCommandBehavior : CommandBehaviorBase<TreeView>
    {
        public TreeViewCommandBehavior(TreeView tree)
            : base(tree)
        {           
            //tree.SelectedItemChanged += TreeView_SelectedItemChanged;
            tree.MouseLeftButtonUp += Tree_MouseLeftButtonUp;
        }

        private void Tree_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var tree = sender as TreeView;
            if (null == tree || tree.SelectedItem == null)
                return;
            var menuItem = tree.SelectedItem as ModuleMenu;
            if (!menuItem.CanNavigate)
                return;

            if (menuItem != null)
                CommandParameter = menuItem;
            else
                CommandParameter = null;

            ExecuteCommand(menuItem);
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            //if (!System.Threading.Thread.CurrentPrincipal.Identity.IsAuthenticated)
            //    return;
            var tree = sender as TreeView;
            
            if ( null == tree || tree.SelectedItem == null)
                return;

            //var param = e.NewActiveTreeNode.Data as INavigationItem;
            var param = e.NewValue as ModuleMenu;
            if (!param.CanNavigate)
                return;

            if (param != null)
                CommandParameter = param;
            //CommandParameter = param.ViewName;
            //CommandParameter = param.NavigationPath;
            else
                CommandParameter = null;

            ExecuteCommand(param);
  
        }

        //void TreeView_SelectedItemChanged(object sender, ActiveNodeChangedEventArgs e)
        //{
        //    //if (!System.Threading.Thread.CurrentPrincipal.Identity.IsAuthenticated)
        //    //    return;

        //    if (e.NewActiveTreeNode == null)
        //        return;

        //    //var param = e.NewActiveTreeNode.Data as INavigationItem;
        //    var param = e.NewActiveTreeNode.Data as ModuleMenu;
        //    if (param != null)
        //        CommandParameter = param.ViewName;
        //        //CommandParameter = param.NavigationPath;
        //    else
        //        CommandParameter = String.Empty;

        //    ExecuteCommand(CommandParameter);
        //}
    }
}
