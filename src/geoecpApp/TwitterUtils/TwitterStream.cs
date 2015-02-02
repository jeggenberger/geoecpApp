using System;
using Tweetinvi;
using Tweetinvi.Core.Enum;
using Tweetinvi.Core.Interfaces;
using Tweetinvi.Core.Interfaces.Streaminvi;

namespace geoecpApp.TwitterUtils
{
	public class TwitterStream
	{
		public TwitterStream(string hashtag)
		{
			TwitterBase.SetCredentials();
			Hashtag = hashtag;
		}

		public string Hashtag { get; private set; }

		private IFilteredStream FilteredStream { get; set; }
		public event EventHandler<ITweet> TweetAdded;

		public void StartStream()
		{
			FilteredStream = Stream.CreateFilteredStream();
			FilteredStream.AddTrack(Hashtag);
			FilteredStream.MatchingTweetReceived += (sender, args) => RaiseTweetAdded(args.Tweet);
			FilteredStream.StartStreamMatchingAllConditionsAsync();
		}

		public void StopStream()
		{
			if (FilteredStream != null && FilteredStream.StreamState != StreamState.Stop)
				FilteredStream.StopStream();
		}

		private void RaiseTweetAdded(ITweet tweet)
		{
			if (TweetAdded != null)
				TweetAdded(this, tweet);
		}
	}
}