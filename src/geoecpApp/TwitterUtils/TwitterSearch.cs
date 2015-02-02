using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Core.Interfaces;
using Tweetinvi.Core.Interfaces.Models.Parameters;

namespace geoecpApp.TwitterUtils
{
	public class TwitterSearch
	{
		public TwitterSearch()
		{
			TwitterBase.SetCredentials();
		}

		public async Task<List<ITweet>> SearchTweetsAsync(string tagName)
		{
			return await Task.Run(() => SearchTweets(tagName));
		}

		public List<ITweet> SearchTweets(string tagName)
		{
			return Search.SearchTweets(tagName).ToList();
		}

		public List<ITweet> SearchTweetsEx(string tagName)
		{
			ITweetSearchParameters searchParameter = Search.GenerateTweetSearchParameter(tagName);
			//searchParameter.SetGeoCode(-122.398720, 37.781157, 1, DistanceMeasure.Miles);
			//searchParameter.Lang = Language.English;
			//searchParameter.SearchType = SearchResultType.Popular;
			searchParameter.MaximumNumberOfResults = 100;
			//searchParameter.Until = new DateTime(2013, 12, 1);
			//searchParameter.SinceId = 399616835892781056;
			//searchParameter.MaxId = 405001488843284480;

			return Search.SearchTweets(searchParameter).ToList();
		}
	}
}