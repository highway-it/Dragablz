using System;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace Dragablz
{
    public delegate void DragablzDragStartedEventHandler ( object sender, DragablzDragStartedEventArgs e );

    public class DragablzDragStartedEventArgs : DragablzItemEventArgs
    {
        public DragablzDragStartedEventArgs ( DragablzItem dragablzItem, DragStartedEventArgs dragStartedEventArgs )
            : base ( dragablzItem )
        {
            DragStartedEventArgs = dragStartedEventArgs ?? throw new ArgumentNullException ( nameof ( dragStartedEventArgs ) );
        }

        public DragablzDragStartedEventArgs ( RoutedEvent routedEvent, DragablzItem dragablzItem, DragStartedEventArgs dragStartedEventArgs )
            : base ( routedEvent, dragablzItem )
        {
            DragStartedEventArgs = dragStartedEventArgs;
        }

        public DragablzDragStartedEventArgs ( RoutedEvent routedEvent, object source, DragablzItem dragablzItem, DragStartedEventArgs dragStartedEventArgs )
            : base ( routedEvent, source, dragablzItem )
        {
            DragStartedEventArgs = dragStartedEventArgs;
        }

        public DragStartedEventArgs DragStartedEventArgs { get; }
    }
}