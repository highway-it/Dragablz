namespace Dragablz.Savablz
{
    /// <summary>
    /// Represents the state of a tab set, in a serializable way
    /// </summary>
    /// <typeparam name="TTabModel">The type of the tab content model</typeparam>
    public class TabSetState < TTabModel >
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TabSetState" /> class.
        /// </summary>
        public TabSetState ( ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="TabSetState" /> class.
        /// </summary>
        /// <param name="tabItems">The tab items</param>
        /// <param name="selectedTabItemIndex">The index of the tab item that is currently selected in the TabSet</param>
        public TabSetState ( TTabModel [ ] tabItems, int? selectedTabItemIndex )
        {
            SelectedTabItemIndex = selectedTabItemIndex;
            TabItems = tabItems;
        }

        /// <summary>
        /// The tab items.
        /// </summary>
        public TTabModel [ ] TabItems { get; set; }

        /// <summary>
        /// The tab item that is currently selected in the tab set.
        /// </summary>
        public int? SelectedTabItemIndex { get; set; }
    }
}