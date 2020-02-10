using System;
using System.Threading;

namespace Dragablz.Referenceless
{
    internal sealed class AnonymousDisposable : ICancelable
    {
        private volatile Action _dispose;

        public bool IsDisposed => _dispose == null;

        public AnonymousDisposable ( Action dispose )
        {
            _dispose = dispose;
        }

        public void Dispose ( )
        {
            var action = Interlocked.Exchange(ref _dispose, null);
            if ( action == null )
                return;
            action ( );
        }
    }
}