using System.Windows.Controls;

namespace Dragablz.Savablz
{
    /// <summary>
    /// The state of a layout branching
    /// </summary>
    /// <typeparam name="TTabModel">The type of the tab content model</typeparam>
    public class BranchState < TTabModel >
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BranchState" /> class.
        /// </summary>
        public BranchState ( ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BranchState" /> class.
        /// </summary>
        /// <param name="firstChild">The first child</param>
        /// <param name="secondChild">The second child</param>
        /// <param name="orientation">The split orientation</param>
        /// <param name="ratio">The split ratio</param>
        public BranchState ( BranchItemState < TTabModel > firstChild, BranchItemState < TTabModel > secondChild, Orientation orientation, double ratio )
        {
            FirstChild = firstChild;
            SecondChild = secondChild;
            Orientation = orientation;
            Ratio = ratio;
        }

        /// <summary>
        /// The first branch
        /// </summary>
        public BranchItemState < TTabModel > FirstChild { get; set; }

        /// <summary>
        /// The second branch
        /// </summary>
        public BranchItemState < TTabModel > SecondChild { get; set; }

        /// <summary>
        /// The split orientation
        /// </summary>
        public Orientation Orientation { get; set; }

        /// <summary>
        /// The split ratio
        /// </summary>
        public double Ratio { get; set; }
    }
}