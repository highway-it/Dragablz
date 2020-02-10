using System;
using System.Collections.Generic;
using System.Windows.Media.Animation;

namespace Dragablz
{
    internal class StoryboardCompletionListener
    {
        private readonly Storyboard _storyboard;
        private readonly Action < Storyboard > _continuation;

        public StoryboardCompletionListener ( Storyboard storyboard, Action < Storyboard > continuation )
        {
            _storyboard = storyboard ?? throw new ArgumentNullException ( nameof ( storyboard ) );
            _continuation = continuation ?? throw new ArgumentNullException ( nameof ( continuation ) );

            _storyboard.Completed += StoryboardOnCompleted;
        }

        private void StoryboardOnCompleted ( object sender, EventArgs eventArgs )
        {
            _storyboard.Completed -= StoryboardOnCompleted;
            _continuation ( _storyboard );
        }
    }

    internal static class StoryboardCompletionListenerExtension
    {
        public static void WhenComplete ( this Storyboard storyboard, Action < Storyboard > continuation )
        {
            // ReSharper disable once ObjectCreationAsStatement
            new StoryboardCompletionListener ( storyboard, continuation );
        }
    }
}