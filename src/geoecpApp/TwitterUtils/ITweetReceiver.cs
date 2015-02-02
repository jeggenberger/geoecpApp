using Tweetinvi.Core.Interfaces;

namespace geoecpApp.TwitterUtils
{
	public interface ITweetReceiver
	{
		string Hashtag { get; }

		void ProcessTweet(ITweet tweet);
	}
}