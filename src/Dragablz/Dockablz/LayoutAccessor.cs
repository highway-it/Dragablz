using System;
using System.Collections.Generic;

namespace Dragablz.Dockablz
{
    /// <summary>
    /// Provides information about the <see cref="Layout" /> instance.
    /// </summary>
    public class LayoutAccessor
    {
        public LayoutAccessor ( Layout layout )
        {
            Layout = layout ?? throw new ArgumentNullException ( nameof ( layout ) );

            if ( Layout.Content is Branch branch )
                BranchAccessor = new BranchAccessor ( branch );
            else
                TabablzControl = Layout.Content as TabablzControl;
        }

        public Layout Layout { get; }

        public IEnumerable < DragablzItem > FloatingItems => Layout.FloatingDragablzItems ( );

        /// <summary>
        /// <see cref="BranchAccessor" /> and <see cref="TabablzControl" /> are mutually exclusive, according to whether the layout has been split, or just contains a tab control.
        /// </summary>
        public BranchAccessor BranchAccessor { get; }

        /// <summary>
        /// <see cref="BranchAccessor" /> and <see cref="TabablzControl" /> are mutually exclusive, according to whether the layout has been split, or just contains a tab control.
        /// </summary>
        public TabablzControl TabablzControl { get; }

        /// <summary>
        /// Visits the content of the layout, according to its content type.  No more than one of the provided <see cref="Action" />
        /// callbacks will be called.
        /// </summary>
        public LayoutAccessor Visit (
            Action < BranchAccessor > branchVisitor = null,
            Action < TabablzControl > tabablzControlVisitor = null,
            Action < object > contentVisitor = null )
        {
            if ( BranchAccessor != null )
            {
                branchVisitor?.Invoke ( BranchAccessor );

                return this;
            }

            if ( TabablzControl != null )
            {
                tabablzControlVisitor?.Invoke ( TabablzControl );

                return this;
            }

            if ( Layout.Content != null && contentVisitor != null )
                contentVisitor ( Layout.Content );

            return this;
        }

        /// <summary>
        /// Gets all the Tabablz controls in a Layout, regardless of location.
        /// </summary>
        /// <returns></returns>
        public IEnumerable < TabablzControl > TabablzControls ( )
        {
            var tabablzControls = new List < TabablzControl > ( );
            this.Visit ( tabablzControls, BranchAccessorVisitor, TabablzControlVisitor );
            return tabablzControls;
        }

        private static void TabablzControlVisitor ( IList < TabablzControl > resultSet, TabablzControl tabablzControl )
        {
            resultSet.Add ( tabablzControl );
        }

        private static void BranchAccessorVisitor ( IList < TabablzControl > resultSet, BranchAccessor branchAccessor )
        {
            branchAccessor
                .Visit ( resultSet, BranchItem.First, BranchAccessorVisitor, TabablzControlVisitor )
                .Visit ( resultSet, BranchItem.Second, BranchAccessorVisitor, TabablzControlVisitor );
        }
    }
}