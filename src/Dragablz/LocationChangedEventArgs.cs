using System;
using System.Windows;

namespace Dragablz
{
    public class LocationChangedEventArgs : EventArgs
    {
        public LocationChangedEventArgs ( object item, Point location )
        {
            Item = item ?? throw new ArgumentNullException ( nameof ( item ) );
            Location = location;
        }

        public object Item { get; }

        public Point Location { get; }
    }
}