using System;
using System.Windows;

namespace Dragablz
{
    public delegate void ItemActionCallback ( ItemActionCallbackArgs < TabablzControl > args );

    public class ItemActionCallbackArgs < TOwner > where TOwner : FrameworkElement
    {
        public ItemActionCallbackArgs ( Window window, TOwner owner, DragablzItem dragablzItem )
        {
            Window = window ?? throw new ArgumentNullException ( nameof ( window ) );
            Owner = owner ?? throw new ArgumentNullException ( nameof ( owner ) );
            DragablzItem = dragablzItem ?? throw new ArgumentNullException ( nameof ( dragablzItem ) );
        }

        public Window Window { get; }

        public TOwner Owner { get; }

        public DragablzItem DragablzItem { get; }

        public bool IsCancelled { get; private set; }

        public void Cancel ( )
        {
            IsCancelled = true;
        }
    }
}