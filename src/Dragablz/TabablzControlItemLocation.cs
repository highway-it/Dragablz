using System;

namespace Dragablz
{
    public class TabablzControlItemLocation
    {
        public TabablzControlItemLocation ( TabablzControl tabControl, object item ) : this ( tabControl, null, item ) { }
        public TabablzControlItemLocation ( TabablzControl tabControl, DragablzItem container, object item )
        {
            TabControl = tabControl ?? throw new ArgumentNullException ( nameof ( tabControl ) );
            Item       = item       ?? throw new ArgumentNullException ( nameof ( item       ) );
            Container  = container;
        }

        public TabablzControl TabControl { get; }
        public object         Item       { get; }

        private DragablzItem container;
        public  DragablzItem Container
        {
            get => container ??= TabControl.ItemContainerGenerator.ContainerFromItem ( Item ) as DragablzItem;
            set => container = value;
        }
    }
}