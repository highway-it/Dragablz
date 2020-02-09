using System.Windows;

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
        /// Initializes a new instance of the <see cref="LayoutWindowState"/> class.
        /// </summary>
        public LayoutWindowState ( ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="LayoutWindowState"/> class.
        /// </summary>
        /// <param name="x">The X position of the window</param>
        /// <param name="y">The Y position of the window</param>
        /// <param name="width">The window width</param>
        /// <param name="height">The window height</param>
        /// <param name="windowState">The window state</param>
        /// <param name="child">The root of this layout</param>
        public LayoutWindowState ( double x, double y, double width, double height, WindowState windowState, BranchItemState < TTabModel > child, TWindowSettings settings )
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            WindowState = windowState;
            Child = child;
            Settings = settings;
        }

        /// <summary>
        /// The window's X position
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// The window's Y position
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// The window's width
        /// </summary>
        public double Width { get; set; }

        /// <summary>
        /// The window's height
        /// </summary>
        public double Height { get; set; }

        /// <summary>
        /// The window state (maximized, restored, minimize)
        /// </summary>
        public WindowState WindowState { get; set; }

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