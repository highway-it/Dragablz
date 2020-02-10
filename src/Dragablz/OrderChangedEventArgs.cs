using System;

namespace Dragablz
{
    public class OrderChangedEventArgs : EventArgs
    {
        public OrderChangedEventArgs ( object [ ] previousOrder, object [ ] newOrder )
        {
            PreviousOrder = previousOrder;
            NewOrder = newOrder ?? throw new ArgumentNullException ( nameof ( newOrder ) );
        }

        public object [ ] PreviousOrder { get; }

        public object [ ] NewOrder { get; }
    }
}