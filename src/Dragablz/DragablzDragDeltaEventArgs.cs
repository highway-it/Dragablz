using System;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace Dragablz
{
    public delegate void DragablzDragDeltaEventHandler ( object sender, DragablzDragDeltaEventArgs e );

    public class DragablzDragDeltaEventArgs : DragablzItemEventArgs
    {
        public DragablzDragDeltaEventArgs ( DragablzItem dragablzItem, DragDeltaEventArgs dragDeltaEventArgs )
            : base ( dragablzItem )
        {
            DragDeltaEventArgs = dragDeltaEventArgs ?? throw new ArgumentNullException ( nameof ( dragDeltaEventArgs ) );
        }

        public DragablzDragDeltaEventArgs ( RoutedEvent routedEvent, DragablzItem dragablzItem, DragDeltaEventArgs dragDeltaEventArgs )
            : base ( routedEvent, dragablzItem )
        {
            DragDeltaEventArgs = dragDeltaEventArgs ?? throw new ArgumentNullException ( nameof ( dragDeltaEventArgs ) );
        }

        public DragablzDragDeltaEventArgs ( RoutedEvent routedEvent, object source, DragablzItem dragablzItem, DragDeltaEventArgs dragDeltaEventArgs )
            : base ( routedEvent, source, dragablzItem )
        {
            DragDeltaEventArgs = dragDeltaEventArgs ?? throw new ArgumentNullException ( nameof ( dragDeltaEventArgs ) );
        }

        public DragDeltaEventArgs DragDeltaEventArgs { get; }

        public bool Cancel { get; set; }
    }
}