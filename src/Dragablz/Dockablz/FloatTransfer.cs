using System;

namespace Dragablz.Dockablz
{
    internal class FloatTransfer
    {
        public FloatTransfer ( double width, double height, object content )
        {
            Width = width;
            Height = height;
            Content = content ?? throw new ArgumentNullException ( nameof ( content ) );
        }

        public static FloatTransfer TakeSnapshot ( DragablzItem dragablzItem, TabablzControl sourceTabControl )
        {
            if ( dragablzItem == null ) throw new ArgumentNullException ( nameof ( dragablzItem ) );

            return new FloatTransfer ( sourceTabControl.ActualWidth, sourceTabControl.ActualHeight, dragablzItem.UnderlyingContent ?? dragablzItem.Content ?? dragablzItem );
        }

        [Obsolete]
        //TODO width and height transfer obsolete
        public double Width { get; }

        [Obsolete]
        //TODO width and height transfer obsolete
        public double Height { get; }

        public object Content { get; }
    }
}