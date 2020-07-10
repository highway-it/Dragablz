namespace Dragablz.Savablz
{
    /// <summary>
    /// Represents the state of a window
    /// </summary>
    /// <typeparam name="TTabModel">The type of the tab content model</typeparam>
    /// <typeparam name="TWindowSettings">The type for custom window settings</typeparam>
    public class LayoutWindowState < TTabModel, TWindowSettings >
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LayoutWindowState" /> class.
        /// </summary>
        public LayoutWindowState ( ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="LayoutWindowState" /> class.
        /// </summary>
        /// <param name="placement">The window placement</param>
        /// <param name="windowState">The window state</param>
        /// <param name="child">The root of this layout</param>
        public LayoutWindowState ( byte [ ] placement, BranchItemState < TTabModel > child, TWindowSettings settings )
        {
            Placement = placement;
            Child     = child;
            Settings  = settings;
        }

        /// <summary>
        /// The window placement and state
        /// </summary>
        public byte [ ] Placement { get; set; }

        /// <summary>
        /// The window settings
        /// </summary>
        public TWindowSettings Settings { get; set; }

        /// <summary>
        /// The root of this layout
        /// </summary>
        public BranchItemState < TTabModel > Child { get; set; }
    }
}