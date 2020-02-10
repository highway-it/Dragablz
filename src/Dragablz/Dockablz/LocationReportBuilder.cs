using System;

namespace Dragablz.Dockablz
{
    internal class LocationReportBuilder
    {
        private Branch _branch;
        private bool _isSecondLeaf;
        private Layout _layout;

        public LocationReportBuilder ( TabablzControl targetTabablzControl )
        {
            TargetTabablzControl = targetTabablzControl;
        }

        public TabablzControl TargetTabablzControl { get; }

        public bool IsFound { get; private set; }

        public void MarkFound ( )
        {
            if ( IsFound )
                throw new InvalidOperationException ( "Already found." );

            IsFound = true;

            _layout = CurrentLayout;
        }

        public void MarkFound ( Branch branch, bool isSecondLeaf )
        {
            if ( IsFound )
                throw new InvalidOperationException ( "Already found." );

            IsFound = true;

            _layout = CurrentLayout;
            _branch = branch ?? throw new ArgumentNullException ( nameof ( branch ) );
            _isSecondLeaf = isSecondLeaf;
        }

        public Layout CurrentLayout { get; set; }

        public LocationReport ToLocationReport ( )
        {
            return new LocationReport ( TargetTabablzControl, _layout, _branch, _isSecondLeaf );
        }
    }
}