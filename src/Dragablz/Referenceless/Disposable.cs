using System;

namespace Dragablz.Referenceless
{
    internal static class Disposable
    {
        public static IDisposable Empty => DefaultDisposable.Instance;

        public static IDisposable Create ( Action dispose )
        {
            if ( dispose == null )
                throw new ArgumentNullException ( nameof ( dispose ) );
            else
                return new AnonymousDisposable ( dispose );
        }
    }
}