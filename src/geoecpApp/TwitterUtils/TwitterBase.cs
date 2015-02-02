using Tweetinvi;

namespace geoecpApp.TwitterUtils
{
	public static class TwitterBase
	{
		private const string AccessToken = "YourAccessTokenHere";
		private const string AccessSecret = "YourAccessSecretHere";
		private const string ConsumerToken = "YourConsumerTokenHere";
		private const string ConsumerSecret = "YourConsumerSecretHere";

		public static void SetCredentials()
		{
			TwitterCredentials.SetCredentials(AccessToken, AccessSecret, ConsumerToken, ConsumerSecret);
		}
	}
}