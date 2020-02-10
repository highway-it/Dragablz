using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace Dragablz
{
    /// <summary>
    /// A linear position monitor simplifies the montoring of the order of items, where they are laid out
    /// horizontally or vertically (typically via a <see cref="StackOrganiser" />.
    /// </summary>
    public abstract class StackPositionMonitor : PositionMonitor
    {
        private readonly Func < DragablzItem, double > _getLocation;

        protected StackPositionMonitor ( Orientation orientation )
        {
            _getLocation = orientation switch
            {
                Orientation.Horizontal => item => item.X,
                Orientation.Vertical => item => item.Y,
                _ => throw new ArgumentOutOfRangeException ( nameof ( orientation ) ),
            };
        }

        public event EventHandler < OrderChangedEventArgs > OrderChanged;

        internal virtual void OnOrderChanged ( OrderChangedEventArgs e )
        {
            OrderChanged?.Invoke ( this, e );
        }

        internal IEnumerable < DragablzItem > Sort ( IEnumerable < DragablzItem > items )
        {
            if ( items == null ) throw new ArgumentNullException ( nameof ( items ) );

            return items.OrderBy ( i => _getLocation ( i ) );
        }
    }
}