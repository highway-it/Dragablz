using System;

namespace Dragablz.Dockablz
{
    public class BranchResult
    {
        public BranchResult ( Branch branch, TabablzControl tabablzControl )
        {
            Branch = branch ?? throw new ArgumentNullException ( nameof ( branch ) );
            TabablzControl = tabablzControl ?? throw new ArgumentNullException ( nameof ( tabablzControl ) );
        }

        /// <summary>
        /// The new branch.
        /// </summary>
        public Branch Branch { get; }

        /// <summary>
        /// The new tab control.
        /// </summary>
        public TabablzControl TabablzControl { get; }
    }
}