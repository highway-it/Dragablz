using Dragablz.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Dragablz.Dockablz
{
    public delegate void ClosingFloatingItemCallback ( ItemActionCallbackArgs < Layout > args );

    [TemplatePart ( Name = TopDropZonePartName, Type = typeof ( DropZone ) )]
    [TemplatePart ( Name = RightDropZonePartName, Type = typeof ( DropZone ) )]
    [TemplatePart ( Name = BottomDropZonePartName, Type = typeof ( DropZone ) )]
    [TemplatePart ( Name = LeftDropZonePartName, Type = typeof ( DropZone ) )]
    [TemplatePart ( Name = FloatingDropZonePartName, Type = typeof ( DropZone ) )]
    [TemplatePart ( Name = FloatingContentPresenterPartName, Type = typeof ( ContentPresenter ) )]
    public class Layout : ContentControl
    {
        private static readonly HashSet < Layout > LoadedLayouts = new HashSet < Layout > ( );
        private const string TopDropZonePartName = "PART_TopDropZone";
        private const string RightDropZonePartName = "PART_RightDropZone";
        private const string BottomDropZonePartName = "PART_BottomDropZone";
        private const string LeftDropZonePartName = "PART_LeftDropZone";
        private const string FloatingDropZonePartName = "PART_FloatDropZone";
        private const string FloatingContentPresenterPartName = "PART_FloatContentPresenter";

        private readonly IDictionary < DropZoneLocation, DropZone > _dropZones = new Dictionary < DropZoneLocation, DropZone > ( );
        private static Tuple < Layout, DropZone > _currentlyOfferedDropZone;

        public static readonly RoutedCommand UnfloatItemCommand = new RoutedCommand ( );
        public static readonly RoutedCommand MaximiseFloatingItem = new RoutedCommand ( );
        public static readonly RoutedCommand RestoreFloatingItem = new RoutedCommand ( );
        public static readonly RoutedCommand CloseFloatingItem = new RoutedCommand ( );
        public static readonly RoutedCommand TileFloatingItemsCommand = new RoutedCommand ( );
        public static readonly RoutedCommand TileFloatingItemsVerticallyCommand = new RoutedCommand ( );
        public static readonly RoutedCommand TileFloatingItemsHorizontallyCommand = new RoutedCommand ( );

        private readonly DragablzItemsControl _floatingItems;
        private static bool _isDragOpWireUpPending;
        private FloatTransfer _floatTransfer;

        static Layout ( )
        {
            DefaultStyleKeyProperty.OverrideMetadata ( typeof ( Layout ), new FrameworkPropertyMetadata ( typeof ( Layout ) ) );

            EventManager.RegisterClassHandler ( typeof ( DragablzItem ), DragablzItem.DragStartedEvent, new DragablzDragStartedEventHandler ( ItemDragStarted ) );
            EventManager.RegisterClassHandler ( typeof ( DragablzItem ), DragablzItem.PreviewDragDeltaEvent, new DragablzDragDeltaEventHandler ( PreviewItemDragDelta ), true );
            EventManager.RegisterClassHandler ( typeof ( DragablzItem ), DragablzItem.DragCompletedEvent, new DragablzDragCompletedEventHandler ( ItemDragCompleted ) );
        }

        public Layout ( )
        {
            Loaded += ( sender, args ) =>
            {
                LoadedLayouts.Add ( this );
                MarkItemLocations ( this );
            };
            Unloaded += ( sender, args ) => LoadedLayouts.Remove ( this );

            CommandBindings.Add ( new CommandBinding ( UnfloatItemCommand, UnfloatExecuted, CanExecuteUnfloat ) );
            CommandBindings.Add ( new CommandBinding ( MaximiseFloatingItem, MaximiseFloatingItemExecuted, CanExecuteMaximiseFloatingItem ) );
            CommandBindings.Add ( new CommandBinding ( CloseFloatingItem, CloseFloatingItemExecuted, CanExecuteCloseFloatingItem ) );
            CommandBindings.Add ( new CommandBinding ( RestoreFloatingItem, RestoreFloatingItemExecuted, CanExecuteRestoreFloatingItem ) );
            CommandBindings.Add ( new CommandBinding ( TileFloatingItemsCommand, TileFloatingItemsExecuted ) );
            CommandBindings.Add ( new CommandBinding ( TileFloatingItemsCommand, TileFloatingItemsExecuted ) );
            CommandBindings.Add ( new CommandBinding ( TileFloatingItemsVerticallyCommand, TileFloatingItemsVerticallyExecuted ) );
            CommandBindings.Add ( new CommandBinding ( TileFloatingItemsHorizontallyCommand, TileFloatingItemsHorizontallyExecuted ) );

            //TODO bad bad behaviour.  Pick up this from the template.
            _floatingItems = new DragablzItemsControl
            {
                ContainerCustomisations = new ContainerCustomisations (
                    GetFloatingContainerForItemOverride,
                    PrepareFloatingContainerForItemOverride,
                    ClearingFloatingContainerForItemOverride )
            };

            var floatingItemsSourceBinding = new Binding("FloatingItemsSource") { Source = this };
            _floatingItems.SetBinding ( ItemsControl.ItemsSourceProperty, floatingItemsSourceBinding );
            var floatingItemsControlStyleBinding = new Binding("FloatingItemsControlStyle") { Source = this };
            _floatingItems.SetBinding ( StyleProperty, floatingItemsControlStyleBinding );
            var floatingItemTemplateBinding = new Binding("FloatingItemTemplate") { Source = this };
            _floatingItems.SetBinding ( ItemsControl.ItemTemplateProperty, floatingItemTemplateBinding );
            var floatingItemTemplateSelectorBinding = new Binding("FloatingItemTemplateSelector") { Source = this };
            _floatingItems.SetBinding ( ItemsControl.ItemTemplateSelectorProperty, floatingItemTemplateSelectorBinding );
            var floatingItemContainerStyeBinding = new Binding("FloatingItemContainerStyle") { Source = this };
            _floatingItems.SetBinding ( ItemsControl.ItemContainerStyleProperty, floatingItemContainerStyeBinding );
            var floatingItemContainerStyleSelectorBinding = new Binding("FloatingItemContainerStyleSelector") { Source = this };
            _floatingItems.SetBinding ( ItemsControl.ItemContainerStyleSelectorProperty, floatingItemContainerStyleSelectorBinding );
        }

        /// <summary>
        /// Helper method to get all the currently loaded layouts.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable < Layout > GetLoadedInstances ( )
        {
            return LoadedLayouts.ToList ( );
        }

        /// <summary>
        /// Finds the location of a tab control withing a layout.
        /// </summary>
        /// <param name="tabablzControl"></param>
        /// <returns></returns>
        public static LocationReport Find ( TabablzControl tabablzControl )
        {
            if ( tabablzControl == null ) throw new ArgumentNullException ( nameof ( tabablzControl ) );

            return Finder.Find ( tabablzControl );
        }

        /// <summary>
        /// Creates a split in a layout, at the location of a specified <see cref="TabablzControl" />.
        /// </summary>
        /// <para></para>
        /// <param name="tabablzControl">Tab control to be split.</param>
        /// <param name="orientation">Direction of split.</param>
        /// <param name="makeSecond">Set to <c>true</c> to make the current tab control push into the right hand or bottom of the split.</param>
        /// <remarks>The tab control to be split must be hosted in a layout control.</remarks>
        public static BranchResult Branch ( TabablzControl tabablzControl, Orientation orientation, bool makeSecond )
        {
            return Branch ( tabablzControl, orientation, makeSecond, .5 );
        }

        /// <summary>
        /// Creates a split in a layout, at the location of a specified <see cref="TabablzControl" />.
        /// </summary>
        /// <para></para>
        /// <param name="tabablzControl">Tab control to be split.</param>
        /// <param name="orientation">Direction of split.</param>
        /// <param name="makeSecond">Set to <c>true</c> to make the current tab control push into the right hand or bottom of the split.</param>
        /// <param name="firstItemProportion">Sets the proportion of the first tab control, with 0.5 being 50% of available space.</param>
        /// <remarks>The tab control to be split must be hosted in a layout control.  <see cref="BranchTemplate" /> should be set (typically via XAML).</remarks>
        public static BranchResult Branch ( TabablzControl tabablzControl, Orientation orientation, bool makeSecond, double firstItemProportion )
        {
            return Branch ( tabablzControl, null, orientation, makeSecond, firstItemProportion );
        }

        /// <summary>
        /// Creates a split in a layout, at the location of a specified <see cref="TabablzControl" />.
        /// </summary>
        /// <para></para>
        /// <param name="tabablzControl">Tab control to be split.</param>
        /// <param name="newSiblingTabablzControl">New sibling tab control (otherwise <see cref="BranchTemplate" /> will be used).</param>
        /// <param name="orientation">Direction of split.</param>
        /// <param name="makeCurrentSecond">Set to <c>true</c> to make the current tab control push into the right hand or bottom of the split.</param>
        /// <param name="firstItemProportion">Sets the proportion of the first tab control, with 0.5 being 50% of available space.</param>
        /// <remarks>The tab control to be split must be hosted in a layout control. </remarks>
        public static BranchResult Branch ( TabablzControl tabablzControl, TabablzControl newSiblingTabablzControl, Orientation orientation, bool makeCurrentSecond,
            double firstItemProportion )
        {
            if ( firstItemProportion < 0.0 || firstItemProportion > 1.0 ) throw new ArgumentOutOfRangeException ( nameof ( firstItemProportion ), "Must be >= 0.0 and <= 1.0" );

            var locationReport = Find(tabablzControl);

            Action < Branch > applier;
            object existingContent;
            if ( ! locationReport.IsLeaf )
            {
                existingContent = locationReport.RootLayout.Content;
                applier = branch => locationReport.RootLayout.Content = branch;
            }
            else if ( ! locationReport.IsSecondLeaf )
            {
                existingContent = locationReport.ParentBranch.FirstItem;
                applier = branch => locationReport.ParentBranch.FirstItem = branch;
            }
            else
            {
                existingContent = locationReport.ParentBranch.SecondItem;
                applier = branch => locationReport.ParentBranch.SecondItem = branch;
            }

            var selectedItem = tabablzControl.SelectedItem;
            var branchResult = Branch(orientation, firstItemProportion, makeCurrentSecond, locationReport.RootLayout.BranchTemplate, newSiblingTabablzControl, existingContent, applier);
            tabablzControl.SelectedItem = selectedItem;
            tabablzControl.Dispatcher.BeginInvoke ( new Action ( ( ) =>
                {
                    tabablzControl.SetCurrentValue ( Selector.SelectedItemProperty, selectedItem );
                    MarkItemLocations ( locationReport.RootLayout );
                } ),
                DispatcherPriority.Loaded );

            return branchResult;
        }

        /// <summary>
        /// Use in conjuction with the <see cref="InterTabController.Partition" /> on a <see cref="TabablzControl" />
        /// to isolate drag and drop spaces/control instances.
        /// </summary>
        public string Partition { get; set; }

        public static readonly DependencyProperty InterLayoutClientProperty = DependencyProperty.Register(
            nameof(InterLayoutClient), typeof (IInterLayoutClient), typeof (Layout), new PropertyMetadata(new DefaultInterLayoutClient ( )));

        public IInterLayoutClient InterLayoutClient
        {
            get { return (IInterLayoutClient) GetValue ( InterLayoutClientProperty ); }
            set { SetValue ( InterLayoutClientProperty, value ); }
        }

        internal static bool IsContainedWithinBranch ( DependencyObject dependencyObject )
        {
            do
            {
                dependencyObject = VisualTreeHelper.GetParent ( dependencyObject );
                if ( dependencyObject is Branch )
                    return true;
            } while ( dependencyObject != null );
            return false;
        }

        private static readonly DependencyPropertyKey IsParticipatingInDragPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(IsParticipatingInDrag), typeof (bool), typeof (Layout),
                new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty IsParticipatingInDragProperty =
            IsParticipatingInDragPropertyKey.DependencyProperty;

        public bool IsParticipatingInDrag
        {
            get { return (bool) GetValue ( IsParticipatingInDragProperty ); }
            private set { SetValue ( IsParticipatingInDragPropertyKey, value ); }
        }

        public static readonly DependencyProperty BranchTemplateProperty = DependencyProperty.Register(
            nameof(BranchTemplate), typeof (DataTemplate), typeof (Layout), new PropertyMetadata(default(DataTemplate)));

        public DataTemplate BranchTemplate
        {
            get { return (DataTemplate) GetValue ( BranchTemplateProperty ); }
            set { SetValue ( BranchTemplateProperty, value ); }
        }

        public static readonly DependencyProperty IsFloatDropZoneEnabledProperty = DependencyProperty.Register(
            nameof(IsFloatDropZoneEnabled), typeof (bool), typeof (Layout), new PropertyMetadata(default(bool)));

        public bool IsFloatDropZoneEnabled
        {
            get { return (bool) GetValue ( IsFloatDropZoneEnabledProperty ); }
            set { SetValue ( IsFloatDropZoneEnabledProperty, value ); }
        }

        /// <summary>
        /// Defines a margin for the container which hosts all floating items.
        /// </summary>
        public static readonly DependencyProperty FloatingItemsContainerMarginProperty = DependencyProperty.Register(
            nameof(FloatingItemsContainerMargin), typeof (Thickness), typeof (Layout), new PropertyMetadata(default(Thickness)));

        /// <summary>
        /// Defines a margin for the container which hosts all floating items.
        /// </summary>
        public Thickness FloatingItemsContainerMargin
        {
            get { return (Thickness) GetValue ( FloatingItemsContainerMarginProperty ); }
            set { SetValue ( FloatingItemsContainerMarginProperty, value ); }
        }

        /// <summary>
        /// Floating items, such as tool/MDI windows, which will sit above the <see cref="Content" />.
        /// </summary>
        public ItemCollection FloatingItems => _floatingItems.Items;

        public static readonly DependencyProperty FloatingItemsSourceProperty = DependencyProperty.Register(
            nameof(FloatingItemsSource), typeof (IEnumerable), typeof (Layout), new PropertyMetadata(default(IEnumerable)));

        /// <summary>
        /// Floating items, such as tool/MDI windows, which will sit above the <see cref="Content" />.
        /// </summary>
        public IEnumerable FloatingItemsSource
        {
            get { return (IEnumerable) GetValue ( FloatingItemsSourceProperty ); }
            set { SetValue ( FloatingItemsSourceProperty, value ); }
        }

        public static readonly DependencyProperty FloatingItemsControlStyleProperty = DependencyProperty.Register(
            nameof(FloatingItemsControlStyle), typeof (Style), typeof (Layout), new PropertyMetadata((Style)null));

        /// <summary>
        /// The style to be applied to the <see cref="DragablzItemsControl" /> which is used to display floating items.
        /// In most scenarios it should be OK to leave this to that applied by the default style.
        /// </summary>
        public Style FloatingItemsControlStyle
        {
            get { return (Style) GetValue ( FloatingItemsControlStyleProperty ); }
            set { SetValue ( FloatingItemsControlStyleProperty, value ); }
        }

        public static readonly DependencyProperty FloatingItemContainerStyleProperty = DependencyProperty.Register(
            nameof(FloatingItemContainerStyle), typeof (Style), typeof (Layout), new PropertyMetadata(default(Style)));

        public Style FloatingItemContainerStyle
        {
            get { return (Style) GetValue ( FloatingItemContainerStyleProperty ); }
            set { SetValue ( FloatingItemContainerStyleProperty, value ); }
        }

        public static readonly DependencyProperty FloatingItemContainerStyleSelectorProperty = DependencyProperty.Register(
            nameof(FloatingItemContainerStyleSelector), typeof (StyleSelector), typeof (Layout), new PropertyMetadata(new CouldBeHeaderedStyleSelector ( )));

        public StyleSelector FloatingItemContainerStyleSelector
        {
            get { return (StyleSelector) GetValue ( FloatingItemContainerStyleSelectorProperty ); }
            set { SetValue ( FloatingItemContainerStyleSelectorProperty, value ); }
        }

        public static readonly DependencyProperty FloatingItemTemplateProperty = DependencyProperty.Register(
            nameof(FloatingItemTemplate), typeof (DataTemplate), typeof (Layout), new PropertyMetadata(default(DataTemplate)));

        public DataTemplate FloatingItemTemplate
        {
            get { return (DataTemplate) GetValue ( FloatingItemTemplateProperty ); }
            set { SetValue ( FloatingItemTemplateProperty, value ); }
        }

        public static readonly DependencyProperty FloatingItemTemplateSelectorProperty = DependencyProperty.Register(
            nameof(FloatingItemTemplateSelector), typeof (DataTemplateSelector), typeof (Layout), new PropertyMetadata(default(DataTemplateSelector)));

        public DataTemplateSelector FloatingItemTemplateSelector
        {
            get { return (DataTemplateSelector) GetValue ( FloatingItemTemplateSelectorProperty ); }
            set { SetValue ( FloatingItemTemplateSelectorProperty, value ); }
        }

        public static readonly DependencyProperty FloatingItemHeaderMemberPathProperty = DependencyProperty.Register(
            nameof(FloatingItemHeaderMemberPath), typeof (string), typeof (Layout), new PropertyMetadata(default(string)));

        public string FloatingItemHeaderMemberPath
        {
            get { return (string) GetValue ( FloatingItemHeaderMemberPathProperty ); }
            set { SetValue ( FloatingItemHeaderMemberPathProperty, value ); }
        }

        public static readonly DependencyProperty FloatingItemDisplayMemberPathProperty = DependencyProperty.Register(
            nameof(FloatingItemDisplayMemberPath), typeof (string), typeof (Layout), new PropertyMetadata(default(string)));

        public string FloatingItemDisplayMemberPath
        {
            get { return (string) GetValue ( FloatingItemDisplayMemberPathProperty ); }
            set { SetValue ( FloatingItemDisplayMemberPathProperty, value ); }
        }

        public static readonly DependencyProperty ClosingFloatingItemCallbackProperty = DependencyProperty.Register(
            nameof(ClosingFloatingItemCallback), typeof (ClosingFloatingItemCallback), typeof (Layout), new PropertyMetadata(default(ClosingFloatingItemCallback)));

        public ClosingFloatingItemCallback ClosingFloatingItemCallback
        {
            get { return (ClosingFloatingItemCallback) GetValue ( ClosingFloatingItemCallbackProperty ); }
            set { SetValue ( ClosingFloatingItemCallbackProperty, value ); }
        }

        public static readonly DependencyPropertyKey IsFloatingInLayoutPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
            "IsFloatingInLayout", typeof (bool), typeof (Layout), new PropertyMetadata(default(bool)));

        private static void SetIsFloatingInLayout ( DependencyObject element, bool value )
        {
            element.SetValue ( IsFloatingInLayoutPropertyKey, value );
        }

        public static bool GetIsFloatingInLayout ( DependencyObject element )
        {
            return (bool) element.GetValue ( IsFloatingInLayoutPropertyKey.DependencyProperty );
        }

        private static readonly DependencyPropertyKey IsLeftItemPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly(
                "IsLeftItem", typeof(bool), typeof(Layout),
                new PropertyMetadata(default(bool)));

        /// <summary>
        /// Indicates if an item/tab control within a layout is contained at the left most branch item.
        /// </summary>
        public static readonly DependencyProperty IsLeftItemProperty = IsLeftItemPropertyKey.DependencyProperty;

        /// <summary>
        /// Indicates if an item/tab control within a layout is contained at the left most branch item.
        /// </summary>
        private static void SetIsLeftItem ( DependencyObject element, bool value )
        {
            element.SetValue ( IsLeftItemPropertyKey, value );
        }

        /// <summary>
        /// Indicates if an item/tab control within a layout is contained at the left most branch item.
        /// </summary>
        public static bool GetIsLeftItem ( DependencyObject element )
        {
            return (bool) element.GetValue ( IsLeftItemProperty );
        }

        private static readonly DependencyPropertyKey IsTopItemPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly(
                "IsTopItem", typeof(bool), typeof(Layout),
                new PropertyMetadata(default(bool)));

        /// <summary>
        /// Indicates if an item/tab control within a layout is contained at the top most branch item.
        /// </summary>
        public static readonly DependencyProperty IsTopItemProperty = IsTopItemPropertyKey.DependencyProperty;

        /// <summary>
        /// Indicates if an item/tab control within a layout is contained at the top most branch item.
        /// </summary>
        private static void SetIsTopItem ( DependencyObject element, bool value )
        {
            element.SetValue ( IsTopItemPropertyKey, value );
        }

        /// <summary>
        /// Indicates if an item/tab control within a layout is contained at the top most branch item.
        /// </summary>
        public static bool GetIsTopItem ( DependencyObject element )
        {
            return (bool) element.GetValue ( IsTopItemProperty );
        }

        private static readonly DependencyPropertyKey IsRightItemPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly(
                "IsRightItem", typeof(bool), typeof(Layout),
                new PropertyMetadata(default(bool)));

        /// <summary>
        /// Indicates if an item/tab control within a layout is contained at the right most branch item.
        /// </summary>
        public static readonly DependencyProperty IsRightItemProperty = IsRightItemPropertyKey.DependencyProperty;

        /// <summary>
        /// Indicates if an item/tab control within a layout is contained at the right most branch item.
        /// </summary>
        private static void SetIsRightItem ( DependencyObject element, bool value )
        {
            element.SetValue ( IsRightItemPropertyKey, value );
        }

        /// <summary>
        /// Indicates if an item/tab control within a layout is contained at the right most branch item.
        /// </summary>
        public static bool GetIsRightItem ( DependencyObject element )
        {
            return (bool) element.GetValue ( IsRightItemProperty );
        }

        private static readonly DependencyPropertyKey IsBottomItemPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly(
                "IsBottomItem", typeof(bool), typeof(Layout),
                new PropertyMetadata(default(bool)));

        /// <summary>
        /// Indicates if an item/tab control within a layout is contained at the bottom most branch item.
        /// </summary>
        public static readonly DependencyProperty IsBottomItemProperty = IsBottomItemPropertyKey.DependencyProperty;

        /// <summary>
        /// Indicates if an item/tab control within a layout is contained at the bottom most branch item.
        /// </summary>
        private static void SetIsBottomItem ( DependencyObject element, bool value )
        {
            element.SetValue ( IsBottomItemPropertyKey, value );
        }

        /// <summary>
        /// Indicates if an item/tab control within a layout is contained at the bottom most branch item.
        /// </summary>
        public static bool GetIsBottomItem ( DependencyObject element )
        {
            return (bool) element.GetValue ( IsBottomItemProperty );
        }

        private static readonly DependencyPropertyKey IsTopLeftItemPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly(
                "IsTopLeftItem", typeof(bool), typeof(Layout),
                new PropertyMetadata(default(bool)));

        /// <summary>
        /// Indicates if an item/tab control within a layout is contained at the top most and left most branch item.
        /// </summary>
        public static readonly DependencyProperty IsTopLeftItemProperty = IsTopLeftItemPropertyKey.DependencyProperty;

        /// <summary>
        /// Indicates if an item/tab control within a layout is contained at the top most and left most branch item.
        /// </summary>
        private static void SetIsTopLeftItem ( DependencyObject element, bool value )
        {
            element.SetValue ( IsTopLeftItemPropertyKey, value );
        }

        /// <summary>
        /// Indicates if an item/tab control within a layout is contained at the top most and left most branch item.
        /// </summary>
        public static bool GetIsTopLeftItem ( DependencyObject element )
        {
            return (bool) element.GetValue ( IsTopLeftItemProperty );
        }

        private static readonly DependencyPropertyKey IsTopRightItemPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly(
                "IsTopRightItem", typeof(bool), typeof(Layout),
                new PropertyMetadata(default(bool)));

        /// <summary>
        /// Indicates if an item/tab control within a layout is contained at the top most and right most branch item.
        /// </summary>
        public static readonly DependencyProperty IsTopRightItemProperty = IsTopRightItemPropertyKey.DependencyProperty;

        /// <summary>
        /// Indicates if an item/tab control within a layout is contained at the top most and right most branch item.
        /// </summary>
        private static void SetIsTopRightItem ( DependencyObject element, bool value )
        {
            element.SetValue ( IsTopRightItemPropertyKey, value );
        }

        /// <summary>
        /// Indicates if an item/tab control within a layout is contained at the top most and right most branch item.
        /// </summary>
        public static bool GetIsTopRightItem ( DependencyObject element )
        {
            return (bool) element.GetValue ( IsTopRightItemProperty );
        }

        /// <summary>When overridden in a derived class, is invoked whenever application code or internal processes call <see cref="M:System.Windows.FrameworkElement.ApplyTemplate" />.</summary>
        public override void OnApplyTemplate ( )
        {
            base.OnApplyTemplate ( );

            if ( GetTemplateChild ( FloatingContentPresenterPartName ) is ContentPresenter floatingItemsContentPresenter )
                floatingItemsContentPresenter.Content = _floatingItems;

            _dropZones [ DropZoneLocation.Top ] = GetTemplateChild ( TopDropZonePartName ) as DropZone;
            _dropZones [ DropZoneLocation.Right ] = GetTemplateChild ( RightDropZonePartName ) as DropZone;
            _dropZones [ DropZoneLocation.Bottom ] = GetTemplateChild ( BottomDropZonePartName ) as DropZone;
            _dropZones [ DropZoneLocation.Left ] = GetTemplateChild ( LeftDropZonePartName ) as DropZone;
            _dropZones [ DropZoneLocation.Floating ] = GetTemplateChild ( FloatingDropZonePartName ) as DropZone;
        }

        internal IEnumerable < DragablzItem > FloatingDragablzItems ( )
        {
            return _floatingItems.DragablzItems ( );
        }

        internal static void RestoreFloatingItemSnapShots ( DependencyObject ancestor, IEnumerable < FloatingItemSnapShot > floatingItemSnapShots )
        {
            var layouts = ancestor.VisualTreeDepthFirstTraversal ( ).OfType < Layout > ( ).ToList ( );
            foreach ( var floatingDragablzItem in layouts.SelectMany ( l => l.FloatingDragablzItems ( ) ) )
            {
                var itemSnapShots = floatingItemSnapShots as FloatingItemSnapShot[] ?? floatingItemSnapShots.ToArray ( );
                var floatingItemSnapShot = Array.Find ( itemSnapShots, ss => ss.Content == floatingDragablzItem.Content );
                floatingItemSnapShot?.Apply ( floatingDragablzItem );
            }
        }

        private static void ItemDragStarted ( object sender, DragablzDragStartedEventArgs e )
        {
            //we wait until drag is in full flow so we know the partition has been setup by the owning tab control
            _isDragOpWireUpPending = true;
        }

        private static void SetupParticipatingLayouts ( DragablzItem dragablzItem )
        {
            if ( ! ( ItemsControl.ItemsControlFromItemContainer ( dragablzItem ) is DragablzItemsControl sourceOfDragItemsControl ) || sourceOfDragItemsControl.Items.Count != 1 ) return;

            var draggingWindow = Window.GetWindow(dragablzItem);
            if ( draggingWindow == null ) return;

            foreach ( var loadedLayout in LoadedLayouts.Where ( l =>
                   l.Partition == dragablzItem.PartitionAtDragStart &&
                   ! Equals ( Window.GetWindow ( l ), draggingWindow ) ) )

            {
                loadedLayout.IsParticipatingInDrag = true;
            }
        }

        private void MonitorDropZones ( Point cursorPos )
        {
            var myWindow = Window.GetWindow(this);
            if ( myWindow == null ) return;

            foreach ( var dropZone in _dropZones.Values.Where ( dz => dz != null ) )
            {
                var pointFromScreen = myWindow.PointFromScreen(cursorPos);
                var pointRelativeToDropZone = myWindow.TranslatePoint(pointFromScreen, dropZone);
                var inputHitTest = dropZone.InputHitTest(pointRelativeToDropZone);
                //TODO better halding when windows are layered over each other
                if ( inputHitTest != null )
                {
                    if ( _currentlyOfferedDropZone != null )
                        _currentlyOfferedDropZone.Item2.IsOffered = false;
                    dropZone.IsOffered = true;
                    _currentlyOfferedDropZone = new Tuple < Layout, DropZone > ( this, dropZone );
                }
                else
                {
                    dropZone.IsOffered = false;
                    if ( _currentlyOfferedDropZone != null && _currentlyOfferedDropZone.Item2 == dropZone )
                        _currentlyOfferedDropZone = null;
                }
            }
        }

        private static bool TryGetSourceTabControl ( DragablzItem dragablzItem, out TabablzControl tabablzControl )
        {
            if ( ! ( ItemsControl.ItemsControlFromItemContainer ( dragablzItem ) is DragablzItemsControl sourceOfDragItemsControl ) ) throw new ApplicationException ( "Unable to determine source items control." );

            tabablzControl = TabablzControl.GetOwnerOfHeaderItems ( sourceOfDragItemsControl );

            return tabablzControl != null;
        }

        private void Branch ( DropZoneLocation location, DragablzItem sourceDragablzItem )
        {
            if ( InterLayoutClient == null )
                throw new InvalidOperationException ( "InterLayoutClient is not set." );

            if ( ! ( ItemsControl.ItemsControlFromItemContainer ( sourceDragablzItem ) is DragablzItemsControl sourceOfDragItemsControl ) ) throw new ApplicationException ( "Unable to determine source items control." );

            var sourceTabControl = TabablzControl.GetOwnerOfHeaderItems(sourceOfDragItemsControl);
            if ( sourceTabControl == null ) throw new ApplicationException ( "Unable to determine source tab control." );

            var floatingItemSnapShots = sourceTabControl.VisualTreeDepthFirstTraversal ( )
                    .OfType < Layout > ( )
                    .SelectMany(l => l.FloatingDragablzItems ( ).Select(FloatingItemSnapShot.Take))
                    .ToList ( );

            var sourceItem = sourceOfDragItemsControl.ItemContainerGenerator.ItemFromContainer(sourceDragablzItem);
            sourceTabControl.RemoveItem ( sourceDragablzItem );

            var branchItem = new Branch
            {
                Orientation = (location == DropZoneLocation.Right || location == DropZoneLocation.Left) ? Orientation.Horizontal : Orientation.Vertical
            };

            object newContent;
            if ( BranchTemplate == null )
            {
                var newTabHost = InterLayoutClient.GetNewHost(Partition, sourceTabControl);
                if ( newTabHost == null )
                    throw new ApplicationException ( "InterLayoutClient did not provide a new tab host." );
                newTabHost.TabablzControl.AddToSource ( sourceItem );
                newTabHost.TabablzControl.SelectedItem = sourceItem;
                newContent = newTabHost.Container;

                Dispatcher.BeginInvoke ( new Action ( ( ) => RestoreFloatingItemSnapShots ( newTabHost.TabablzControl, floatingItemSnapShots ) ), DispatcherPriority.Loaded );
            }
            else
            {
                newContent = new ContentControl
                {
                    Content = new object ( ),
                    ContentTemplate = BranchTemplate,
                };
                ( (ContentControl) newContent ).Dispatcher.BeginInvoke ( new Action ( ( ) =>
                      {
                    //TODO might need to improve this a bit, make it a bit more declarative for complex trees
                    var newTabControl = ((ContentControl)newContent).VisualTreeDepthFirstTraversal ( ).OfType < TabablzControl > ( ).FirstOrDefault ( );
                          if ( newTabControl == null ) return;

                          newTabControl.DataContext = sourceTabControl.DataContext;
                          newTabControl.AddToSource ( sourceItem );
                          newTabControl.SelectedItem = sourceItem;
                          Dispatcher.BeginInvoke ( new Action ( ( ) => RestoreFloatingItemSnapShots ( newTabControl, floatingItemSnapShots ) ), DispatcherPriority.Loaded );
                      } ), DispatcherPriority.Loaded );
            }

            if ( location == DropZoneLocation.Right || location == DropZoneLocation.Bottom )
            {
                branchItem.FirstItem = Content;
                branchItem.SecondItem = newContent;
            }
            else
            {
                branchItem.FirstItem = newContent;
                branchItem.SecondItem = Content;
            }

            SetCurrentValue ( ContentProperty, branchItem );

            Dispatcher.BeginInvoke ( new Action ( ( ) => MarkItemLocations ( this ) ), DispatcherPriority.Loaded );
        }

        internal static bool ConsolidateBranch ( DependencyObject redundantNode )
        {
            if ( ! ( FindLayoutOrBranchOwner ( redundantNode, out var isSecondLineageWhenOwnerIsBranch ) is Branch ownerBranch ) ) return false;

            var survivingItem = isSecondLineageWhenOwnerIsBranch ? ownerBranch.FirstItem : ownerBranch.SecondItem;

            var grandParent = FindLayoutOrBranchOwner(ownerBranch, out isSecondLineageWhenOwnerIsBranch);
            if ( grandParent == null ) throw new ApplicationException ( "Unexpected structure, grandparent Layout or Branch not found" );

            if ( grandParent is Layout layout )
            {
                layout.Content = survivingItem;
                MarkItemLocations ( layout, true );
                return true;
            }

            var branch = (Branch) grandParent;
            if ( isSecondLineageWhenOwnerIsBranch )
                branch.SecondItem = survivingItem;
            else
                branch.FirstItem = survivingItem;

            var rootLayout = branch.VisualTreeAncestory ( ).OfType < Layout > ( ).FirstOrDefault ( );
            if ( rootLayout != null )
                MarkItemLocations ( rootLayout, true );

            return true;
        }

        private static object FindLayoutOrBranchOwner ( DependencyObject node, out bool isSecondLineageWhenOwnerIsBranch )
        {
            isSecondLineageWhenOwnerIsBranch = false;

            var ancestoryStack = new Stack < DependencyObject > ( );
            do
            {
                ancestoryStack.Push ( node );
                node = VisualTreeHelper.GetParent ( node );
                if ( node is Layout )
                    return node;

                if ( ! ( node is Branch branch ) ) continue;

                isSecondLineageWhenOwnerIsBranch = ancestoryStack.Contains ( branch.SecondContentPresenter );
                return branch;
            } while ( node != null );

            return null;
        }

        private static BranchResult Branch ( Orientation orientation, double proportion, bool makeSecond, DataTemplate branchTemplate, TabablzControl newSibling, object existingContent, Action < Branch > applier )
        {
            var branchItem = new Branch
            {
                Orientation = orientation
            };

            var newContent = new ContentControl
            {
                Content = newSibling ?? new object ( ),
                ContentTemplate = branchTemplate,
            };

            if ( ! makeSecond )
            {
                branchItem.FirstItem = existingContent;
                branchItem.SecondItem = newContent;
            }
            else
            {
                branchItem.FirstItem = newContent;
                branchItem.SecondItem = existingContent;
            }

            branchItem.SetCurrentValue ( Dockablz.Branch.FirstItemLengthProperty, new GridLength ( proportion, GridUnitType.Star ) );
            branchItem.SetCurrentValue ( Dockablz.Branch.SecondItemLengthProperty, new GridLength ( 1 - proportion, GridUnitType.Star ) );

            applier ( branchItem );

            newContent.Dispatcher.Invoke ( new Action ( ( ) => { } ), DispatcherPriority.Loaded );
            var newTabablzControl = newContent.VisualTreeDepthFirstTraversal ( ).OfType < TabablzControl > ( ).FirstOrDefault ( );
            if ( newTabablzControl != null ) return new BranchResult ( branchItem, newTabablzControl );

            //let#s be kinf and give WPF an extra change to gen the controls
            newContent.Dispatcher.Invoke ( new Action ( ( ) => { } ), DispatcherPriority.Background );
            newTabablzControl = newContent.VisualTreeDepthFirstTraversal ( ).OfType < TabablzControl > ( ).FirstOrDefault ( );

            if ( newTabablzControl == null )
                throw new ApplicationException ( "New TabablzControl was not generated inside branch." );

            return new BranchResult ( branchItem, newTabablzControl );
        }

        private static void ItemDragCompleted ( object sender, DragablzDragCompletedEventArgs e )
        {
            _isDragOpWireUpPending = false;

            foreach ( var loadedLayout in LoadedLayouts )
                loadedLayout.IsParticipatingInDrag = false;

            if ( _currentlyOfferedDropZone == null || e.DragablzItem.IsDropTargetFound ) return;

            if ( TryGetSourceTabControl ( e.DragablzItem, out var tabablzControl ) )
            {
                if ( tabablzControl.Items.Count > 1 ) return;

                if ( _currentlyOfferedDropZone.Item2.Location == DropZoneLocation.Floating )
                    Float ( _currentlyOfferedDropZone.Item1, e.DragablzItem );
                else
                    _currentlyOfferedDropZone.Item1.Branch ( _currentlyOfferedDropZone.Item2.Location, e.DragablzItem );
            }

            _currentlyOfferedDropZone = null;
        }

        private static void Float ( Layout layout, DragablzItem dragablzItem )
        {
            //TODO we need eq of IManualInterTabClient here, so consumer can control this op'.

            //remove from source
            if ( ! ( ItemsControl.ItemsControlFromItemContainer ( dragablzItem ) is DragablzItemsControl sourceOfDragItemsControl ) ) throw new ApplicationException ( "Unable to determine source items control." );
            var sourceTabControl = TabablzControl.GetOwnerOfHeaderItems(sourceOfDragItemsControl);
            layout._floatTransfer = FloatTransfer.TakeSnapshot ( dragablzItem, sourceTabControl );
            var floatingItemSnapShots = sourceTabControl.VisualTreeDepthFirstTraversal ( )
                    .OfType < Layout > ( )
                    .SelectMany(l => l.FloatingDragablzItems ( ).Select(FloatingItemSnapShot.Take))
                    .ToList ( );
            if ( sourceTabControl == null ) throw new ApplicationException ( "Unable to determine source tab control." );
            sourceTabControl.RemoveItem ( dragablzItem );

            //add to float layer
            if ( CollectionTeaser.TryCreate ( layout.FloatingItemsSource, out var collectionTeaser ) )
                collectionTeaser.Add ( layout._floatTransfer.Content );
            else
                layout.FloatingItems.Add ( layout._floatTransfer.Content );

            layout.Dispatcher.BeginInvoke ( new Action ( ( ) => RestoreFloatingItemSnapShots ( layout, floatingItemSnapShots ) ), DispatcherPriority.Loaded );
        }

        private static void PreviewItemDragDelta ( object sender, DragablzDragDeltaEventArgs e )
        {
            if ( e.Cancel ) return;

            if ( _isDragOpWireUpPending )
            {
                SetupParticipatingLayouts ( e.DragablzItem );
                _isDragOpWireUpPending = false;
            }

            foreach ( var layout in LoadedLayouts.Where ( l => l.IsParticipatingInDrag ) )
            {
                var cursorPos = Native.GetCursorPos ( );
                layout.MonitorDropZones ( cursorPos );
            }
        }

        private void PrepareFloatingContainerForItemOverride ( DependencyObject dependencyObject, object o )
        {
            if ( ! ( dependencyObject is HeaderedDragablzItem headeredDragablzItem ) ) return;

            SetIsFloatingInLayout ( dependencyObject, true );

            var headerBinding = new Binding(FloatingItemHeaderMemberPath) {Source = o};
            headeredDragablzItem.SetBinding ( HeaderedDragablzItem.HeaderContentProperty, headerBinding );

            if ( ! string.IsNullOrWhiteSpace ( FloatingItemDisplayMemberPath ) )
            {
                var contentBinding = new Binding(FloatingItemDisplayMemberPath) {Source = o};
                headeredDragablzItem.SetBinding ( ContentProperty, contentBinding );
            }

            if ( _floatTransfer == null ||  o != _floatTransfer.Content && dependencyObject != _floatTransfer.Content  )
                return;

            var dragablzItem = (DragablzItem) dependencyObject;

            Dispatcher.BeginInvoke ( new Action ( ( ) =>
                {
                //TODO might be nice to allow user a bit of control over sizing...especially the .75 thing i have handily hard coded.  shoot me.
                dragablzItem.Measure ( new Size ( _floatingItems.ActualWidth, _floatingItems.ActualHeight ) );
                    var newWidth = Math.Min(_floatingItems.ActualWidth*.75, dragablzItem.DesiredSize.Width);
                    var newHeight = Math.Min(_floatingItems.ActualHeight * .75, dragablzItem.DesiredSize.Height);
                    dragablzItem.SetCurrentValue ( DragablzItem.XProperty, _floatingItems.ActualWidth / 2 - newWidth / 2 );
                    dragablzItem.SetCurrentValue ( DragablzItem.YProperty, _floatingItems.ActualHeight / 2 - newHeight / 2 );
                    dragablzItem.SetCurrentValue ( WidthProperty, newWidth );
                    dragablzItem.SetCurrentValue ( HeightProperty, newHeight );
                } ), DispatcherPriority.Loaded );

            _floatTransfer = null;
        }

        private DragablzItem GetFloatingContainerForItemOverride ( )
        {
            return string.IsNullOrWhiteSpace ( FloatingItemHeaderMemberPath ) ? new DragablzItem ( ) : new HeaderedDragablzItem ( );
        }

        private static void ClearingFloatingContainerForItemOverride ( DependencyObject dependencyObject, object o )
        {
            SetIsFloatingInLayout ( dependencyObject, false );
        }

        private void TileFloatingItemsExecuted ( object sender, ExecutedRoutedEventArgs executedRoutedEventArgs )
        {
            var dragablzItems = _floatingItems.DragablzItems ( );
            Tiler.Tile ( dragablzItems, new Size ( _floatingItems.ActualWidth, _floatingItems.ActualHeight ) );
        }

        private void TileFloatingItemsHorizontallyExecuted ( object sender, ExecutedRoutedEventArgs e )
        {
            var dragablzItems = _floatingItems.DragablzItems ( );
            Tiler.TileHorizontally ( dragablzItems, new Size ( _floatingItems.ActualWidth, _floatingItems.ActualHeight ) );
        }

        private void TileFloatingItemsVerticallyExecuted ( object sender, ExecutedRoutedEventArgs e )
        {
            var dragablzItems = _floatingItems.DragablzItems ( );
            Tiler.TileVertically ( dragablzItems, new Size ( _floatingItems.ActualWidth, _floatingItems.ActualHeight ) );
        }

        public static readonly DependencyProperty FloatingItemStateProperty = DependencyProperty.RegisterAttached(
            "FloatingItemState", typeof (WindowState), typeof (Layout), new PropertyMetadata(default(WindowState)));

        public static void SetFloatingItemState ( DependencyObject element, WindowState value )
        {
            element.SetValue ( FloatingItemStateProperty, value );
        }

        public static WindowState GetFloatingItemState ( DependencyObject element )
        {
            return (WindowState) element.GetValue ( FloatingItemStateProperty );
        }

        internal static readonly DependencyProperty LocationSnapShotProperty = DependencyProperty.RegisterAttached(
            "LocationSnapShot", typeof (LocationSnapShot), typeof (Layout), new PropertyMetadata(default(LocationSnapShot)));

        internal static void SetLocationSnapShot ( FrameworkElement element, LocationSnapShot value )
        {
            element.SetValue ( LocationSnapShotProperty, value );
        }

        internal static LocationSnapShot GetLocationSnapShot ( FrameworkElement element )
        {
            return (LocationSnapShot) element.GetValue ( LocationSnapShotProperty );
        }

        private static void CanExecuteMaximiseFloatingItem ( object sender, CanExecuteRoutedEventArgs canExecuteRoutedEventArgs )
        {
            canExecuteRoutedEventArgs.CanExecute = false;
            canExecuteRoutedEventArgs.Handled = true;

            if ( canExecuteRoutedEventArgs.Parameter is DragablzItem dragablzItem )
            {
                canExecuteRoutedEventArgs.CanExecute = new [ ] { WindowState.Normal, WindowState.Minimized }.Contains ( GetFloatingItemState ( dragablzItem ) );
            }
        }

        private static void CanExecuteRestoreFloatingItem ( object sender, CanExecuteRoutedEventArgs canExecuteRoutedEventArgs )
        {
            canExecuteRoutedEventArgs.CanExecute = false;
            canExecuteRoutedEventArgs.Handled = true;

            if ( canExecuteRoutedEventArgs.Parameter is DragablzItem dragablzItem )
            {
                canExecuteRoutedEventArgs.CanExecute = new [ ] { WindowState.Maximized, WindowState.Minimized }.Contains ( GetFloatingItemState ( dragablzItem ) );
            }
        }

        private static void CanExecuteCloseFloatingItem ( object sender, CanExecuteRoutedEventArgs canExecuteRoutedEventArgs )
        {
            canExecuteRoutedEventArgs.CanExecute = true;
            canExecuteRoutedEventArgs.Handled = true;
        }

        private void CloseFloatingItemExecuted ( object sender, ExecutedRoutedEventArgs executedRoutedEventArgs )
        {
            if ( ! ( executedRoutedEventArgs.Parameter is DragablzItem dragablzItem ) ) throw new ApplicationException ( "Parameter must be a DragablzItem" );

            var cancel = false;
            if ( ClosingFloatingItemCallback != null )
            {
                var callbackArgs = new ItemActionCallbackArgs < Layout >(Window.GetWindow(this), this, dragablzItem);
                ClosingFloatingItemCallback ( callbackArgs );
                cancel = callbackArgs.IsCancelled;
            }

            if ( cancel ) return;

            //TODO ...need a similar tp manual inter tab controlller here for the extra hook

            var item = _floatingItems.ItemContainerGenerator.ItemFromContainer(dragablzItem);

            if ( CollectionTeaser.TryCreate ( _floatingItems.ItemsSource, out var collectionTeaser ) )
                collectionTeaser.Remove ( item );
            else
                _floatingItems.Items.Remove ( item );
        }

        private static void MaximiseFloatingItemExecuted ( object sender, ExecutedRoutedEventArgs e )
        {
            if ( ! ( e.Parameter is DragablzItem dragablzItem ) ) return;

            SetLocationSnapShot ( dragablzItem, LocationSnapShot.Take ( dragablzItem ) );
            SetFloatingItemState ( dragablzItem, WindowState.Maximized );
        }

        private static void RestoreFloatingItemExecuted ( object sender, ExecutedRoutedEventArgs e )
        {
            if ( ! ( e.Parameter is DragablzItem dragablzItem ) ) return;

            SetFloatingItemState ( dragablzItem, WindowState.Normal );
            var locationSnapShot = GetLocationSnapShot(dragablzItem);
            locationSnapShot?.Apply ( dragablzItem );
        }

        private bool IsHostingTab ( )
        {
            return this.VisualTreeDepthFirstTraversal ( ).OfType < TabablzControl > ( )
                .Any ( t => t.InterTabController != null && t.InterTabController.Partition == Partition );
        }

        private static void MarkItemLocations ( Layout layout, bool consolidating = false )
        {
            var layoutAccessor = layout.Query ( );
            if ( layoutAccessor.TabablzControl == null )
            {
                var branchAccessor = layoutAccessor.BranchAccessor;
                while ( branchAccessor != null && branchAccessor.FirstItemTabablzControl == null )
                    branchAccessor = branchAccessor.FirstItemBranchAccessor;

                foreach ( var tabablzControl in layoutAccessor.TabablzControls ( ) )
                    SetIsTopLeftItem ( tabablzControl, branchAccessor != null && Equals ( tabablzControl, branchAccessor.FirstItemTabablzControl ) );
            }
            else
                SetIsTopLeftItem ( layoutAccessor.TabablzControl, true );

            var rootLayout = layout.VisualTreeAncestory ( ).OfType < Layout > ( ).FirstOrDefault ( ) ?? layout;

            foreach ( var tabablzControl in layoutAccessor.TabablzControls ( ) )
            {
                var area = GetRelativeArea ( tabablzControl, rootLayout );

                SetIsLeftItem     ( tabablzControl, area.Left   == 0.0 );
                SetIsTopItem      ( tabablzControl, area.Top    == 0.0 );
                SetIsRightItem    ( tabablzControl, area.Right  == 1.0 );
                SetIsBottomItem   ( tabablzControl, area.Bottom == 1.0 );
                SetIsTopRightItem ( tabablzControl, area.Top    == 0.0 &&
                                                    area.Right  == 1.0 );
            }

            if ( consolidating )
                layout.Dispatcher.BeginInvoke ( new Action ( ( ) => MarkItemLocations ( layout ) ), DispatcherPriority.Background );
        }

        private static Rect GetRelativeArea ( TabablzControl tabablzControl, FrameworkElement relativeTo )
        {
            const double SnapThreshold = 20.0;

            var width       = tabablzControl.ActualWidth;
            var height      = tabablzControl.ActualHeight;
            var topLeft     = tabablzControl.TranslatePoint ( new Point ( 0,     0      ), relativeTo );
            var bottomRight = tabablzControl.TranslatePoint ( new Point ( width, height ), relativeTo );
            var area        = new Size ( relativeTo.ActualWidth, relativeTo.ActualHeight );

            if ( topLeft    .X <= SnapThreshold               ) topLeft    .X = 0;
            if ( topLeft    .Y <= SnapThreshold               ) topLeft    .Y = 0;
            if ( bottomRight.X >= area.Width  - SnapThreshold ) bottomRight.X = area.Width;
            if ( bottomRight.Y >= area.Height - SnapThreshold ) bottomRight.Y = area.Height;

            topLeft    .X /= area.Width;
            topLeft    .Y /= area.Height;
            bottomRight.X /= area.Width;
            bottomRight.Y /= area.Height;

            return new Rect ( topLeft, bottomRight );
        }

        private void CanExecuteUnfloat ( object sender, CanExecuteRoutedEventArgs canExecuteRoutedEventArgs )
        {
            canExecuteRoutedEventArgs.CanExecute = IsHostingTab ( );
            canExecuteRoutedEventArgs.ContinueRouting = false;
            canExecuteRoutedEventArgs.Handled = true;
        }

        private void UnfloatExecuted ( object sender, ExecutedRoutedEventArgs executedRoutedEventArgs )
        {
            if ( ! ( executedRoutedEventArgs.Parameter is DragablzItem dragablzItem ) ) return;

            var exemplarTabControl = this.VisualTreeDepthFirstTraversal ( ).OfType < TabablzControl > ( )
                .FirstOrDefault(t => t.InterTabController != null && t.InterTabController.Partition == Partition);

            if ( exemplarTabControl == null ) return;

            //TODO passing the exemplar tab in here isnt ideal, as strictly speaking there isnt one.
            var newTabHost = exemplarTabControl.InterTabController.InterTabClient.GetNewHost(exemplarTabControl.InterTabController.InterTabClient,
                exemplarTabControl.InterTabController.Partition, exemplarTabControl);
            if ( newTabHost == null || newTabHost.TabablzControl == null || newTabHost.Container == null )
                throw new ApplicationException ( "New tab host was not correctly provided" );

            var floatingItemSnapShots = dragablzItem.VisualTreeDepthFirstTraversal ( )
                    .OfType < Layout > ( )
                    .SelectMany(l => l.FloatingDragablzItems ( ).Select(FloatingItemSnapShot.Take))
                    .ToList ( );

            var content = dragablzItem.Content ?? dragablzItem;

            //remove from source
            if ( CollectionTeaser.TryCreate ( FloatingItemsSource, out var collectionTeaser ) )
                collectionTeaser.Remove ( content );
            else
                FloatingItems.Remove ( content );

            var myWindow = Window.GetWindow(this);
            if ( myWindow == null ) throw new ApplicationException ( "Unable to find owning window." );
            newTabHost.Container.Width = myWindow.RestoreBounds.Width;
            newTabHost.Container.Height = myWindow.RestoreBounds.Height;

            newTabHost.Container.Left = myWindow.Left + 20;
            newTabHost.Container.Top = myWindow.Top + 20;

            Dispatcher.BeginInvoke ( new Action ( ( ) =>
                {
                    newTabHost.TabablzControl.AddToSource ( content );
                    newTabHost.TabablzControl.SelectedItem = content;
                    newTabHost.Container.Show ( );
                    newTabHost.Container.Activate ( );

                    Dispatcher.BeginInvoke (
                        new Action ( ( ) => RestoreFloatingItemSnapShots ( newTabHost.TabablzControl, floatingItemSnapShots ) ) );
                } ), DispatcherPriority.DataBind );
        }
    }
}