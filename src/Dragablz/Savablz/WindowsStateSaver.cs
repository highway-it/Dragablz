using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

using Dragablz.Dockablz;

namespace Dragablz.Savablz
{
    /// <summary>
    /// Saves/restore the state of the windows
    /// </summary>
    public static class WindowsStateSaver
    {
        /// <summary>
        /// Gets the state of all windows in the application.
        /// </summary>
        /// <typeparam name="TTabModel">The type of tab model</typeparam>
        /// <typeparam name="TTabViewModel">The type of tab view model, currently displayed in the app.</typeparam>
        /// <param name="tabContentModelConverter">The converter that transforms tab view models to models</param>
        /// <returns>The state of all windows</returns>
        public static IEnumerable < LayoutWindowState < TTabModel, object? > > GetWindowsState < TTabModel, TTabViewModel > ( Func < TTabViewModel, TTabModel > tabContentModelConverter )
        {
            return Layout.GetLoadedInstances ( ).Select ( layout => GetLayoutState ( layout, tabContentModelConverter, window => (object?) null ) );
        }

        /// <summary>
        /// Gets the state of all windows in the application.
        /// </summary>
        /// <typeparam name="TTabModel">The type of tab model</typeparam>
        /// <typeparam name="TTabViewModel">The type of tab view model, currently displayed in the app.</typeparam>
        /// <typeparam name="TWindowSettings">The type of custom window settings.</typeparam>
        /// <param name="tabContentModelConverter">The converter that transforms tab view models to models</param>
        /// <param name="windowSettingsConverter">The function that serializes custom window settings</param>
        /// <returns>The state of all windows</returns>
        public static IEnumerable < LayoutWindowState < TTabModel, TWindowSettings > > GetWindowsState < TTabModel, TTabViewModel, TWindowSettings > ( Func < TTabViewModel, TTabModel > tabContentModelConverter, Func < Window, TWindowSettings > windowSettingsConverter )
        {
            return Layout.GetLoadedInstances ( ).Select ( layout => GetLayoutState ( layout, tabContentModelConverter, windowSettingsConverter ) );
        }

        /// <summary>
        /// Gets the state of a single window.
        /// </summary>
        /// <typeparam name="TTabModel">The type of tab model</typeparam>
        /// <typeparam name="TTabViewModel">The type of tab view model, currently displayed in the app.</typeparam>
        /// <typeparam name="TWindowSettings">The type of custom window settings.</typeparam>
        /// <param name="layout">The layout to be inspected</param>
        /// <param name="tabContentModelConverter">The converter that transforms tab view models to models</param>
        /// <param name="windowSettingsConverter">The function that serializes custom window settings</param>
        /// <returns>The state of the specified window</returns>
        private static LayoutWindowState < TTabModel, TWindowSettings > GetLayoutState < TTabModel, TTabViewModel, TWindowSettings > ( Layout layout, Func < TTabViewModel, TTabModel > tabContentModelConverter, Func < Window, TWindowSettings > windowSettingsConverter )
        {
            var window = Window.GetWindow ( layout ) ?? throw new InvalidOperationException ( "The layout is not bound to any window" );
            var root   = (BranchItemState < TTabModel >) null;

            layout.Query ( ).Visit (
                branchAccessor => root = new BranchItemState < TTabModel > ( GetBranchState ( branchAccessor, tabContentModelConverter ), null ),
                tabablzControl => root = new BranchItemState < TTabModel > ( null, GetTabSetState ( tabablzControl, tabContentModelConverter ) )
            );

            return new LayoutWindowState < TTabModel, TWindowSettings > ( window.Left, window.Top, window.Width, window.Height, window.WindowState, root, windowSettingsConverter ( window ) );
        }

        /// <summary>
        /// Gets the state of a branch.
        /// </summary>
        /// <typeparam name="TTabModel">The type of tab model</typeparam>
        /// <typeparam name="TTabViewModel">The type of tab view model, currently displayed in the app.</typeparam>
        /// <param name="branchVisitor">The branch to be inspected</param>
        /// <param name="tabContentModelConverter">The converter that transforms tab view models to models</param>
        /// <returns>The read state of the branch</returns>
        private static BranchState < TTabModel > GetBranchState < TTabModel, TTabViewModel > ( BranchAccessor branchVisitor, Func < TTabViewModel, TTabModel > tabContentModelConverter )
        {
            var firstState  = (BranchItemState < TTabModel >) null;
            var secondState = (BranchItemState < TTabModel >) null;

            if ( branchVisitor.FirstItemBranchAccessor != null )
                firstState = new BranchItemState < TTabModel > ( GetBranchState ( branchVisitor.FirstItemBranchAccessor, tabContentModelConverter ), null );
            else
                firstState = new BranchItemState < TTabModel > ( null, GetTabSetState ( branchVisitor.FirstItemTabablzControl, tabContentModelConverter ) );

            if ( branchVisitor.SecondItemBranchAccessor != null )
                secondState = new BranchItemState < TTabModel > ( GetBranchState ( branchVisitor.SecondItemBranchAccessor, tabContentModelConverter ), null );
            else
                secondState = new BranchItemState < TTabModel > ( null, GetTabSetState ( branchVisitor.SecondItemTabablzControl, tabContentModelConverter ) );

            return new BranchState < TTabModel > ( firstState,
                                                   secondState,
                                                   branchVisitor.Branch.Orientation,
                                                   branchVisitor.Branch.GetFirstProportion ( ) );
        }

        /// <summary>
        /// Gets the state of a TabablzControl so that it can be serialized.
        /// </summary>
        /// <typeparam name="TTabModel">The type of tab model</typeparam>
        /// <typeparam name="TTabViewModel">The type of tab view model, currently displayed in the app.</typeparam>
        /// <param name="tabablzControl">The control to be </param>
        /// <param name="tabContentModelConverter">The converter that transforms tab view models to models</param>
        /// <returns>The state of the tab set</returns>
        public static TabSetState < TTabModel > GetTabSetState < TTabModel, TTabViewModel > ( TabablzControl tabablzControl, Func < TTabViewModel, TTabModel > tabContentModelConverter )
        {
            var tabItems      = tabablzControl.Items.Cast < TTabViewModel > ( ).Select ( tabContentModelConverter ).ToArray ( );
            var selectedIndex = (int?) tabablzControl.SelectedIndex;
            if ( selectedIndex == -1 )
                selectedIndex = null;

            return new TabSetState < TTabModel > ( tabItems, selectedIndex );
        }

        /// <summary>
        /// Restores the state of all windows.
        /// </summary>
        /// <typeparam name="TTabModel">The type of tab model</typeparam>
        /// <typeparam name="TTabViewModel">The type of tab view model to be displayed in the app.</typeparam>
        /// <param name="windowInitialTabablzControl">The initial tabablz control that will be used for restore</param>
        /// <param name="layoutWindowsState">The state of the windows</param>
        /// <param name="viewModelFactory">The function that creates the view model based on a model</param>
        public static void RestoreWindowsState < TTabModel, TTabViewModel > ( TabablzControl windowInitialTabablzControl, LayoutWindowState < TTabModel, object? > [ ] layoutWindowsState, Func < TTabModel, TTabViewModel > viewModelFactory )
        {
            RestoreWindowsState ( windowInitialTabablzControl, layoutWindowsState, viewModelFactory, (window, settings) => { } );
        }

        /// <summary>
        /// Restores the state of all windows.
        /// </summary>
        /// <typeparam name="TTabModel">The type of tab model</typeparam>
        /// <typeparam name="TTabViewModel">The type of tab view model to be displayed in the app.</typeparam>
        /// <typeparam name="TWindowSettings">The type of custom window settings.</typeparam>
        /// <param name="windowInitialTabablzControl">The initial tabablz control that will be used for restore</param>
        /// <param name="layoutWindowsState">The state of the windows</param>
        /// <param name="viewModelFactory">The function that creates the view model based on a model</param>
        /// <param name="applyWindowSettings">The function that applies custom window settings</param>
        public static void RestoreWindowsState < TTabModel, TTabViewModel, TWindowSettings > ( TabablzControl windowInitialTabablzControl, LayoutWindowState < TTabModel, TWindowSettings > [ ] layoutWindowsState, Func < TTabModel, TTabViewModel > viewModelFactory, Action < Window, TWindowSettings > applyWindowSettings )
        {
            if ( ! layoutWindowsState.Any ( ) )
                return;

            var mainWindowState = layoutWindowsState [ 0 ];
            var mainWindow      = Window.GetWindow ( windowInitialTabablzControl );
            if ( mainWindow == null )
                throw new InvalidOperationException ( "The TabablzControl is not bound to any window" );

            mainWindow.Width       = mainWindowState.Width;
            mainWindow.Height      = mainWindowState.Height;
            mainWindow.Left        = mainWindowState.X;
            mainWindow.Top         = mainWindowState.Y;
            mainWindow.WindowState = mainWindowState.WindowState;

            using ( new WindowRestoringState ( mainWindow ) )
            {
                applyWindowSettings ( mainWindow, mainWindowState.Settings );

                RestoreBranchItemState ( windowInitialTabablzControl, mainWindowState.Child, viewModelFactory );
            }

            foreach ( var windowState in layoutWindowsState.Skip ( 1 ) )
            {
                var interTabController = windowInitialTabablzControl.InterTabController;
                var newHost            = interTabController.InterTabClient.GetNewHost ( interTabController.InterTabClient,
                                                                                        interTabController.Partition,
                                                                                        windowInitialTabablzControl );

                newHost.Container.Width       = windowState.Width;
                newHost.Container.Height      = windowState.Height;
                newHost.Container.Left        = windowState.X;
                newHost.Container.Top         = windowState.Y;
                newHost.Container.WindowState = windowState.WindowState;

                using ( new WindowRestoringState ( newHost.Container ) )
                {
                    applyWindowSettings ( newHost.Container, windowState.Settings );

                    RestoreBranchItemState ( newHost.TabablzControl, windowState.Child, viewModelFactory );
                }
            }
        }

        /// <summary>
        /// Restores the state of the tab set.
        /// </summary>
        /// <typeparam name="TTabModel">The type of tab model</typeparam>
        /// <typeparam name="TTabViewModel">The type of tab view model to be displayed in the app.</typeparam>
        /// <param name="tabablzControl">The control in which to restore the items</param>
        /// <param name="tabSetState">The state of the tab set to be restored</param>
        /// <param name="viewModelFactory">The function that creates the view model based on a model</param>
        public static void RestoreTabSetState < TTabModel, TTabViewModel > ( TabablzControl tabablzControl, TabSetState < TTabModel > tabSetState, Func < TTabModel, TTabViewModel > viewModelFactory )
        {
            var restoreSelectedTabItem = (Action) ( ( ) => tabablzControl.SelectedIndex = tabSetState.SelectedTabItemIndex ?? -1 );

            foreach ( var tabModel in tabSetState.TabItems )
            {
                var tabViewModel = viewModelFactory ( tabModel );
                if ( tabViewModel != null )
                    tabablzControl.AddToSource ( tabViewModel );
            }

            tabablzControl.Dispatcher.BeginInvoke ( restoreSelectedTabItem, DispatcherPriority.Loaded );
        }

        /// <summary>
        /// Restores the state of the branch.
        /// </summary>
        /// <typeparam name="TTabModel">The type of tab model</typeparam>
        /// <typeparam name="TTabViewModel">The type of tab view model to be displayed in the app.</typeparam>
        /// <param name="tabablzControl">The control in which to restore the items</param>
        /// <param name="branchState">The state of the branch to be restored</param>
        /// <param name="viewModelFactory">The function that creates the view model based on a model</param>
        private static void RestoreBranchState < TTabModel, TTabViewModel > ( TabablzControl tabablzControl, BranchState < TTabModel > branchState, Func < TTabModel, TTabViewModel > viewModelFactory )
        {
            var layout  = Layout.Find ( tabablzControl ).RootLayout;
            var newHost = layout.InterLayoutClient.GetNewHost ( tabablzControl.InterTabController.Partition, tabablzControl );
            var branch  = Layout.Branch ( tabablzControl, newHost.TabablzControl, branchState.Orientation, false, branchState.Ratio );

            RestoreBranchItemState ( tabablzControl,        branchState.FirstChild,  viewModelFactory );
            RestoreBranchItemState ( branch.TabablzControl, branchState.SecondChild, viewModelFactory );
        }

        /// <summary>
        /// Restores the state of a branch item
        /// </summary>
        /// <typeparam name="TTabModel">The type of tab model</typeparam>
        /// <typeparam name="TTabViewModel">The type of tab view model to be displayed in the app.</typeparam>
        /// <param name="tabablzControl">The control in which to restore the items</param>
        /// <param name="branchItemState">The state of the branch item to be restored</param>
        /// <param name="viewModelFactory">The function that creates the view model based on a model</param>
        private static void RestoreBranchItemState < TTabModel, TTabViewModel > ( TabablzControl tabablzControl, BranchItemState < TTabModel > branchItemState, Func < TTabModel, TTabViewModel > viewModelFactory )
        {
            if ( branchItemState.TabSet != null )
                RestoreTabSetState ( tabablzControl, branchItemState.TabSet, viewModelFactory );
            else if ( branchItemState.Branch != null )
                RestoreBranchState ( tabablzControl, branchItemState.Branch, viewModelFactory );
        }

        private class WindowRestoringState : IDisposable
        {
            private readonly Window window;

            public WindowRestoringState ( Window window )
            {
                this.window = window;

                window.Opacity = 0;
                window.Show ( );
            }

            public void Dispose ( )
            {
                window.Opacity = 1;
            }
        }
    }
}