using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

using Dragablz.Core;
using Dragablz.Dockablz;

namespace Dragablz
{
    public partial class TabablzControl
    {
        private static void RegisterClassCommandBindings ( )
        {
            CommandManager.RegisterClassCommandBinding ( typeof ( FrameworkElement ), new CommandBinding ( CloseItemCommand,           ExecuteCloseItem,           CanExecuteCloseItem           ) );
            CommandManager.RegisterClassCommandBinding ( typeof ( FrameworkElement ), new CommandBinding ( CloseOtherItemsCommand,     ExecuteCloseOtherItems,     CanExecuteCloseOtherItems     ) );
            CommandManager.RegisterClassCommandBinding ( typeof ( FrameworkElement ), new CommandBinding ( ClosePrecedingItemsCommand, ExecuteClosePrecedingItems, CanExecuteClosePrecedingItems ) );
            CommandManager.RegisterClassCommandBinding ( typeof ( FrameworkElement ), new CommandBinding ( CloseFollowingItemsCommand, ExecuteCloseFollowingItems, CanExecuteCloseFollowingItems ) );
            CommandManager.RegisterClassCommandBinding ( typeof ( FrameworkElement ), new CommandBinding ( MoveItemToNewWindowCommand, ExecuteMoveItemToNewWindow, CanExecuteMoveItemToNewWindow ) );
        }

        /// <summary>
        /// Routed command which can be used to add a new tab.  See <see cref="NewItemFactory" />.
        /// </summary>
        public static readonly RoutedCommand AddItemCommand = new RoutedUICommand ( "New tab", "AddItem", typeof ( TabablzControl ) );

        /// <summary>
        /// Routed command which can be used to close a tab.
        /// </summary>
        public static readonly RoutedCommand CloseItemCommand = new RoutedUICommand ( "Close tab", "CloseItem", typeof ( TabablzControl ) );

        /// <summary>
        /// Routed command which can be used to close all tabs except a tab.
        /// </summary>
        public static readonly RoutedCommand CloseOtherItemsCommand = new RoutedUICommand ( "Close other tabs", "CloseOtherItems", typeof ( TabablzControl ) );

        /// <summary>
        /// Routed command which can be used to close all tabs preceding a tab.
        /// </summary>
        public static readonly RoutedCommand ClosePrecedingItemsCommand = new RoutedUICommand ( "Close preceding tabs", "ClosePrecedingItems", typeof ( TabablzControl ) );

        /// <summary>
        /// Routed command which can be used to close all tabs following a tab.
        /// </summary>
        public static readonly RoutedCommand CloseFollowingItemsCommand = new RoutedUICommand ( "Close following tabs", "CloseFollowingItems", typeof ( TabablzControl ) );

        /// <summary>
        /// Routed command which can be used to close all tabs following a tab.
        /// </summary>
        public static readonly RoutedCommand MoveItemToNewWindowCommand = new RoutedUICommand ( "Move tab to new window", "MoveItemToNewWindow", typeof ( TabablzControl ) );

        public static void CloseItem           ( DragablzItem dragablzItem ) => CloseItemCommand          .Execute ( dragablzItem, dragablzItem );
        public static void CloseOtherItems     ( DragablzItem dragablzItem ) => CloseOtherItemsCommand    .Execute ( dragablzItem, dragablzItem );
        public static void ClosePrecedingItems ( DragablzItem dragablzItem ) => ClosePrecedingItemsCommand.Execute ( dragablzItem, dragablzItem );
        public static void CloseFollowingItems ( DragablzItem dragablzItem ) => CloseFollowingItemsCommand.Execute ( dragablzItem, dragablzItem );
        public static void MoveItemToNewWindow ( DragablzItem dragablzItem ) => MoveItemToNewWindowCommand.Execute ( dragablzItem, dragablzItem );

        private static void CanExecuteCloseItem ( object sender, CanExecuteRoutedEventArgs e )
        {
            e.CanExecute = FindOwner ( e.Parameter, e.OriginalSource ).DragablzItem != null;
        }

        private static void ExecuteCloseItem ( object sender, ExecutedRoutedEventArgs e )
        {
            var owner = EnsureOwner ( e.Parameter, e.OriginalSource );

            CloseItem ( owner.DragablzItem, owner.TabablzControl );
        }

        private static void CanExecuteCloseOtherItems ( object sender, CanExecuteRoutedEventArgs e )
        {
            e.CanExecute = FindOwner ( e.Parameter, e.OriginalSource ).TabablzControl?.Items.Count > 1;
        }

        private static void ExecuteCloseOtherItems ( object sender, ExecutedRoutedEventArgs e )
        {
            var owner = EnsureOwner ( e.Parameter, e.OriginalSource );

            owner.TabablzControl.ExecuteCloseOtherItems ( owner.DragablzItem );
        }

        private void ExecuteCloseOtherItems ( DragablzItem dragablzItem )
        {
            foreach ( var header in GetOrderedHeaders ( ).Reverse ( ).Where ( header => header != dragablzItem ) )
                CloseItem ( header, this );
        }

        private static void CanExecuteClosePrecedingItems ( object sender, CanExecuteRoutedEventArgs e )
        {
            var owner = FindOwner ( e.Parameter, e.OriginalSource );

            e.CanExecute = owner.TabablzControl?.GetOrderedHeaders ( ).FirstOrDefault ( )?.Equals ( owner.DragablzItem ) == false;
        }

        private static void ExecuteClosePrecedingItems ( object sender, ExecutedRoutedEventArgs e )
        {
            var owner = EnsureOwner ( e.Parameter, e.OriginalSource );

            owner.TabablzControl.ExecuteClosePrecedingItems ( owner.DragablzItem );
        }

        private void ExecuteClosePrecedingItems ( DragablzItem dragablzItem )
        {
            foreach ( var header in GetOrderedHeaders ( ).Reverse ( ).SkipWhile ( header => header != dragablzItem ).Skip ( 1 ) )
                CloseItem ( header, this );
        }

        private static void CanExecuteCloseFollowingItems ( object sender, CanExecuteRoutedEventArgs e )
        {
            var owner = FindOwner ( e.Parameter, e.OriginalSource );

            e.CanExecute = owner.TabablzControl?.GetOrderedHeaders ( ).LastOrDefault ( )?.Equals ( owner.DragablzItem ) == false;
        }

        private static void ExecuteCloseFollowingItems ( object sender, ExecutedRoutedEventArgs e )
        {
            var owner = EnsureOwner ( e.Parameter, e.OriginalSource );

            owner.TabablzControl.ExecuteCloseFollowingItems ( owner.DragablzItem );
        }

        private void ExecuteCloseFollowingItems ( DragablzItem dragablzItem )
        {
            foreach ( var header in GetOrderedHeaders ( ).Reverse ( ).TakeWhile ( header => header != dragablzItem ) )
                CloseItem ( header, this );
        }

        private static void CanExecuteMoveItemToNewWindow ( object sender, CanExecuteRoutedEventArgs e )
        {
            var owner = FindOwner ( e.Parameter, e.OriginalSource );

            e.CanExecute = owner.TabablzControl?.CanExecuteMoveItemToNewWindow ( owner.DragablzItem ) == true;
        }

        private bool CanExecuteMoveItemToNewWindow ( DragablzItem dragablzItem )
        {
            return dragablzItem != null && ! IsFixedItem ( dragablzItem ) && ( Layout.IsContainedWithinBranch ( _dragablzItemsControl ) || Items.Count > 1 );
        }

        private static void ExecuteMoveItemToNewWindow ( object sender, ExecutedRoutedEventArgs e )
        {
            var owner = EnsureOwner ( e.Parameter, e.OriginalSource );

            owner.TabablzControl.ExecuteMoveItemToNewWindow ( owner.DragablzItem );
        }

        private void ExecuteMoveItemToNewWindow ( DragablzItem dragablzItem )
        {
            if ( ! CanExecuteMoveItemToNewWindow ( dragablzItem ) )
                return;

            var interTabController = InterTabController;

            var newHost = interTabController.InterTabClient.GetNewHost ( interTabController.InterTabClient,
                                                                         interTabController.Partition,
                                                                         this );

            newHost.Container.Show ( );

            var minSize = EmptyHeaderSizingHint == EmptyHeaderSizingHint.PreviousTab ? new Size ( _dragablzItemsControl.ActualWidth,
                                                                                                  _dragablzItemsControl.ActualHeight ) :
                                                                                       new Size ( );

            var item = dragablzItem.DataContext;

            RemoveFromSource ( item );

            _itemsHolder.Children.Remove ( FindChildContentPresenter ( item ) );

            if ( Items.Count == 0 )
            {
                _dragablzItemsControl.MinHeight = minSize.Height;
                _dragablzItemsControl.MinWidth  = minSize.Width;
                Layout.ConsolidateBranch ( this );
            }

            newHost.TabablzControl.AddToSource ( item );
            newHost.TabablzControl.SelectedItem = item;
        }

        private static ( DragablzItem DragablzItem, TabablzControl TabablzControl ) EnsureOwner ( object eventParameter, object eventOriginalSource )
        {
            var owner = FindOwner ( eventParameter, eventOriginalSource );
            if ( owner.DragablzItem == null )
                throw new ApplicationException ( "Unable to ascertain DragablzItem to target." );

            return owner;
        }

        private static ( DragablzItem DragablzItem, TabablzControl TabablzControl ) FindOwner ( object eventParameter, object eventOriginalSource )
        {
            if ( ! ( eventParameter is DragablzItem dragablzItem ) )
            {
                var dependencyObject = eventOriginalSource as DependencyObject;

                dragablzItem = dependencyObject.VisualTreeAncestory ( ).OfType < DragablzItem > ( ).FirstOrDefault ( );
                if ( dragablzItem == null )
                {
                    var popup = dependencyObject.LogicalTreeAncestory ( ).OfType < Popup > ( ).LastOrDefault ( );
                    if ( popup?.PlacementTarget != null )
                        dragablzItem = popup.PlacementTarget.VisualTreeAncestory ( ).OfType < DragablzItem > ( ).FirstOrDefault ( );
                }
            }

            if ( dragablzItem == null )
                return default;

            var tabablzControl = LoadedInstances.FirstOrDefault ( tabControl => tabControl.IsMyItem ( dragablzItem ) );

            return tabablzControl != null ? ( dragablzItem, tabablzControl ) : default;
        }
    }
}