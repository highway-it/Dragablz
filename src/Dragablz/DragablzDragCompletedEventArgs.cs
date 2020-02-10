using System;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace Dragablz
{
    public delegate void DragablzDragCompletedEventHandler ( object sender, DragablzDragCompletedEventArgs e );

    public class DragablzDragCompletedEventArgs : RoutedEventArgs
    {
        public DragablzDragCompletedEventArgs ( DragablzItem dragablzItem, DragCompletedEventArgs dragCompletedEventArgs )
        {
            DragablzItem = dragablzItem ?? throw new ArgumentNullException ( nameof ( dragablzItem ) );
            DragCompletedEventArgs = dragCompletedEventArgs ?? throw new ArgumentNullException ( nameof ( dragCompletedEventArgs ) );
        }

        public DragablzDragCompletedEventArgs ( RoutedEvent routedEvent, DragablzItem dragablzItem, DragCompletedEventArgs dragCompletedEventArgs )
            : base ( routedEvent )
        {
            DragablzItem = dragablzItem ?? throw new ArgumentNullException ( nameof ( dragablzItem ) );
            DragCompletedEventArgs = dragCompletedEventArgs ?? throw new ArgumentNullException ( nameof ( dragCompletedEventArgs ) );
        }

        public DragablzDragCompletedEventArgs ( RoutedEvent routedEvent, object source, DragablzItem dragablzItem, DragCompletedEventArgs dragCompletedEventArgs )
            : base ( routedEvent, source )
        {
            DragablzItem = dragablzItem ?? throw new ArgumentNullException ( nameof ( dragablzItem ) );
            DragCompletedEventArgs = dragCompletedEventArgs ?? throw new ArgumentNullException ( nameof ( dragCompletedEventArgs ) );
        }

        public DragablzItem DragablzItem { get; }

        public DragCompletedEventArgs DragCompletedEventArgs { get; }
    }
}