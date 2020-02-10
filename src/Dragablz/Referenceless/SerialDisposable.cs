using System;

namespace Dragablz.Referenceless
{
    internal sealed class SerialDisposable : ICancelable
    {
        private readonly object _gate = new object ( );
        private IDisposable _current;
        private bool _disposed;

        /// <summary>
        /// Gets a value that indicates whether the object is disposed.
        ///
        /// </summary>
        public bool IsDisposed
        {
            get
            {
                lock ( _gate )
                    return _disposed;
            }
        }

        /// <summary>
        /// Gets or sets the underlying disposable.
        ///
        /// </summary>
        ///
        /// <remarks>
        /// If the SerialDisposable has already been disposed, assignment to this property causes immediate disposal of the given disposable object. Assigning this property disposes the previous disposable object.
        /// </remarks>
        public IDisposable Disposable
        {
            get
            {
                return _current;
            }
            set
            {
                bool flag = false;
                var disposable = (IDisposable)null;
                lock ( _gate )
                {
                    flag = _disposed;
                    if ( ! flag )
                    {
                        disposable = _current;
                        _current = value;
                    }
                }
                disposable?.Dispose ( );
                if ( ! flag || value == null )
                    return;
                value.Dispose ( );
            }
        }

        /// <summary>
        /// Disposes the underlying disposable as well as all future replacements.
        ///
        /// </summary>
        public void Dispose ( )
        {
            var disposable = (IDisposable)null;
            lock ( _gate )
            {
                if ( ! _disposed )
                {
                    _disposed = true;
                    disposable = _current;
                    _current = null;
                }
            }
            if ( disposable == null )
                return;
            disposable.Dispose ( );
        }
    }
}