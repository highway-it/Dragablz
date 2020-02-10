using Dragablz.Dockablz;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Dragablz.Core
{
    internal enum InterTabTransferReason
    {
        Breach,
        Reentry
    }

    internal class InterTabTransfer
    {
        public InterTabTransfer ( object item, DragablzItem originatorContainer, Orientation breachOrientation, Point dragStartWindowOffset, Point dragStartItemOffset, Point itemPositionWithinHeader, Size itemSize, IList < FloatingItemSnapShot > floatingItemSnapShots, bool isTransposing )
        {
            TransferReason = InterTabTransferReason.Breach;

            Item = item ?? throw new ArgumentNullException ( nameof ( item ) );
            OriginatorContainer = originatorContainer ?? throw new ArgumentNullException ( nameof ( originatorContainer ) );
            BreachOrientation = breachOrientation;
            DragStartWindowOffset = dragStartWindowOffset;
            DragStartItemOffset = dragStartItemOffset;
            ItemPositionWithinHeader = itemPositionWithinHeader;
            ItemSize = itemSize;
            FloatingItemSnapShots = floatingItemSnapShots ?? throw new ArgumentNullException ( nameof ( floatingItemSnapShots ) );
            IsTransposing = isTransposing;
        }

        public InterTabTransfer ( object item, DragablzItem originatorContainer, Point dragStartItemOffset,
            IList < FloatingItemSnapShot > floatingItemSnapShots )
        {
            TransferReason = InterTabTransferReason.Reentry;

            Item = item ?? throw new ArgumentNullException ( nameof ( item ) );
            OriginatorContainer = originatorContainer ?? throw new ArgumentNullException ( nameof ( originatorContainer ) );
            DragStartItemOffset = dragStartItemOffset;
            FloatingItemSnapShots = floatingItemSnapShots ?? throw new ArgumentNullException ( nameof ( floatingItemSnapShots ) );
        }

        public Orientation BreachOrientation { get; }

        public Point DragStartWindowOffset { get; }

        public object Item { get; }

        public DragablzItem OriginatorContainer { get; }

        public InterTabTransferReason TransferReason { get; }

        public Point DragStartItemOffset { get; }

        public Point ItemPositionWithinHeader { get; }

        public Size ItemSize { get; }

        public IList<FloatingItemSnapShot> FloatingItemSnapShots { get; }

        public bool IsTransposing { get; }
    }
}