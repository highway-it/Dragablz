using System;
using System.Collections.Generic;
using System.Linq;

namespace Dragablz
{
    public partial class TabablzControl
    {
        /// <summary>
        /// Helper method to add an item next to an existing item.
        /// </summary>
        /// <remarks>
        /// Due to the organisable nature of the control, the order of items may not reflect the order in the source collection.  This method
        /// will add items to the source collection, managing their initial appearance on screen at the same time.
        /// If you are using a <see cref="InterTabController.InterTabClient" /> this will be used to add the item into the source collection.
        /// </remarks>
        /// <param name="item">New item to add.</param>
        /// <param name="nearItem">Existing object/tab item content which defines which tab control should be used to add the object.</param>
        /// <param name="addLocationHint">Location, relative to the <paramref name="nearItem" /> object</param>
        public static void AddItem ( object item, object nearItem, AddLocationHint addLocationHint )
        {
            if ( nearItem == null )
                throw new ArgumentNullException ( nameof ( nearItem ) );

            var existingLocation = FindItem ( item ).SingleOrDefault ( );
            if ( existingLocation == null )
                throw new ArgumentException ( "Did not find precisely one instance of adjacentTo", nameof ( nearItem ) );

            existingLocation.TabControl.AddToSource ( item );
            existingLocation.TabControl._dragablzItemsControl?.MoveItem ( new MoveItemRequest ( item, nearItem, addLocationHint ) );
        }

        /// <summary>
        /// Finds and selects an item.
        /// </summary>
        /// <param name="item"></param>
        public static void SelectItem ( object item )
        {
            var existingLocation = FindItem ( item ).FirstOrDefault ( );
            if ( existingLocation == null )
                return;

            existingLocation.TabControl.SelectedItem = item;
        }

        /// <summary>
        /// Finds and closes an item.
        /// </summary>
        /// <param name="item"></param>
        public static void CloseItem ( object item )
        {
            foreach ( var existingLocation in FindItems ( foundItem => item.Equals ( foundItem ) ) )
                CloseItem ( existingLocation.Container, existingLocation.TabControl );
        }

        /// <summary>
        /// Helper method to close all tabs where the item is the tab's content (helpful with MVVM scenarios)
        /// </summary>
        /// <remarks>
        /// In MVVM scenarios where you don't want to bind the routed command to your ViewModel,
        /// with this helper method and embedding the TabablzControl in a UserControl, you can keep
        /// the View-specific dependencies out of the ViewModel.
        /// </remarks>
        /// <param name="tabContent">An existing Tab item content (a ViewModel in MVVM scenarios) which is backing a tab control</param>
        public static void CloseItemByTabContent ( object tabContent )
        {
            if ( tabContent == null )
                return;

            foreach ( var tabWithItemContent in FindItemsByTabContent ( foundTabContent => tabContent.Equals ( foundTabContent ) ) )
                CloseItem ( tabWithItemContent.Container, tabWithItemContent.TabControl );
        }

        /// <summary>
        /// Finds an item.
        /// </summary>
        /// <param name="item"></param>
        public static IEnumerable < TabablzControlItemLocation > FindItem ( object item )
        {
            if ( item == null )
                throw new ArgumentNullException ( nameof ( item ) );

            return FindItems ( foundItem => item.Equals ( foundItem ) );
        }

        /// <summary>
        /// Finds items.
        /// </summary>
        /// <param name="item"></param>
        public static IEnumerable < TabablzControlItemLocation > FindItems ( Func < object, bool > predicate )
        {
            if ( predicate == null )
                throw new ArgumentNullException ( nameof ( predicate ) );

            return GetLoadedInstances ( ).SelectMany ( tabControl => FindItems ( tabControl, predicate ) );
        }

        private static IEnumerable < TabablzControlItemLocation > FindItems ( TabablzControl tabControl, Func < object, bool > predicate )
        {
            var source = tabControl.ItemsSource ?? tabControl.Items;

            return source.Cast < object > ( )
                         .Where  ( predicate )
                         .Select ( item => new TabablzControlItemLocation ( tabControl, item ) );
        }

        /// <summary>
        /// Finds items by tab content.
        /// </summary>
        /// <param name="item"></param>
        public static IEnumerable < TabablzControlItemLocation > FindItemsByTabContent ( Func < object, bool > predicate )
        {
            if ( predicate == null )
                throw new ArgumentNullException ( nameof ( predicate ) );

            return GetLoadedInstances ( ).SelectMany ( tabControl => FindItemsByTabContent ( tabControl, predicate ) );
        }

        private static IEnumerable < TabablzControlItemLocation > FindItemsByTabContent ( TabablzControl tabControl, Func < object, bool > predicate )
        {
            return tabControl._dragablzItemsControl
                             .DragablzItems ( )
                             .Where  ( container => predicate ( container.Content ) )
                             .Select ( container => new TabablzControlItemLocation ( tabControl, container, container.DataContext ) );
        }

        /// <summary>
        /// Helper method which returns all the currently loaded instances.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable < TabablzControl > GetLoadedInstances ( )
        {
            return LoadedInstances.Union ( VisibleInstances ).Distinct ( ).ToList ( );
        }
    }
}