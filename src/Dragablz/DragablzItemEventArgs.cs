using System;
using System.Windows;

namespace Dragablz
{
    public delegate void DragablzItemEventHandler ( object sender, DragablzItemEventArgs e );

    public class DragablzItemEventArgs : RoutedEventArgs
    {
        public DragablzItemEventArgs ( DragablzItem dragablzItem )
        {
            DragablzItem = dragablzItem ?? throw new ArgumentNullException ( nameof ( dragablzItem ) );
        }

        public DragablzItemEventArgs ( RoutedEvent routedEvent, DragablzItem dragablzItem )
            : base ( routedEvent )
        {
            DragablzItem = dragablzItem;
        }

        public DragablzItemEventArgs ( RoutedEvent routedEvent, object source, DragablzItem dragablzItem )
            : base ( routedEvent, source )
        {
            DragablzItem = dragablzItem;
        }

        public DragablzItem DragablzItem { get; }
    }
}