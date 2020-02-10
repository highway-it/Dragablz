using System;
using System.Windows;
using System.Windows.Controls;

namespace Dragablz.Dockablz
{
    /// <summary>
    /// experimentational.  might have to puish this back to mvvm only
    /// </summary>
    internal class FloatingItemSnapShot
    {
        public FloatingItemSnapShot ( object content, Rect location, int zIndex, WindowState state )
        {
            Content = content ?? throw new ArgumentNullException ( nameof ( content ) );
            Location = location;
            ZIndex = zIndex;
            State = state;
        }

        public static FloatingItemSnapShot Take ( DragablzItem dragablzItem )
        {
            if ( dragablzItem == null ) throw new ArgumentNullException ( nameof ( dragablzItem ) );

            return new FloatingItemSnapShot (
                dragablzItem.Content,
                new Rect ( dragablzItem.X, dragablzItem.Y, dragablzItem.ActualWidth, dragablzItem.ActualHeight ),
                Panel.GetZIndex ( dragablzItem ),
                Layout.GetFloatingItemState ( dragablzItem ) );
        }

        public void Apply ( DragablzItem dragablzItem )
        {
            if ( dragablzItem == null ) throw new ArgumentNullException ( nameof ( dragablzItem ) );

            dragablzItem.SetCurrentValue ( DragablzItem.XProperty, Location.Left );
            dragablzItem.SetCurrentValue ( DragablzItem.YProperty, Location.Top );
            dragablzItem.SetCurrentValue ( FrameworkElement.WidthProperty, Location.Width );
            dragablzItem.SetCurrentValue ( FrameworkElement.HeightProperty, Location.Height );
            Layout.SetFloatingItemState ( dragablzItem, State );
            Panel.SetZIndex ( dragablzItem, ZIndex );
        }

        public object Content { get; }

        public Rect Location { get; }

        public int ZIndex { get; }

        public WindowState State { get; }
    }
}