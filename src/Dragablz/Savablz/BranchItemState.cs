namespace Dragablz.Savablz
{
    /// <summary>
    /// The item that is stored in a branch state (first or second)
    /// </summary>
    /// <typeparam name="TTabItem">The tab item type</typeparam>
    public class BranchItemState < TTabItem >
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BranchItemState" /> class.
        /// </summary>
        public BranchItemState ( ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BranchItemState" /> class.
        /// </summary>
        /// <param name="branch">If this item is a branch, this parameter must contain the branch state (<c>null</c> otherwise)</param>
        /// <param name="tabSet">If this item is a tab set, this parameter must contain the tab set state (<c>null</c> otherwise)</param>
        public BranchItemState ( BranchState < TTabItem > branch, TabSetState < TTabItem > tabSet )
        {
            Branch = branch;
            TabSet = tabSet;
        }

        /// <summary>
        /// The branch, if this item is a branch, <c>null</c> otherwise
        /// </summary>
        public BranchState < TTabItem > Branch { get; set; }

        /// <summary>
        /// The tab set, if this item is a tab set, <c>null</c> otherwise
        /// </summary>
        public TabSetState < TTabItem > TabSet { get; set; }
    }
}