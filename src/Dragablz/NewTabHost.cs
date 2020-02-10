using System;
using System.Windows;

namespace Dragablz
{
    public class NewTabHost < TElement > : INewTabHost < TElement > where TElement : UIElement
    {
        public NewTabHost ( TElement container, TabablzControl tabablzControl )
        {
            Container = container ?? throw new ArgumentNullException ( nameof ( container ) );
            TabablzControl = tabablzControl ?? throw new ArgumentNullException ( nameof ( tabablzControl ) );
        }

        public TElement Container { get; }

        public TabablzControl TabablzControl { get; }
    }
}