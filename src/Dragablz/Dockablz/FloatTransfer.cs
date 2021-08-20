using System;

namespace Dragablz.Dockablz
{
    internal class FloatTransfer
    {
        public FloatTransfer ( object content )
        {
            Content = content ?? throw new ArgumentNullException ( nameof ( content ) );
        }

        public static FloatTransfer TakeSnapshot ( DragablzItem dragablzItem )
        {
            if ( dragablzItem == null ) throw new ArgumentNullException ( nameof ( dragablzItem ) );

            return new FloatTransfer ( dragablzItem.UnderlyingContent ?? dragablzItem.Content ?? dragablzItem );
        }

        public object Content { get; }
    }
}