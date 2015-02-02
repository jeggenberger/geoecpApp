using System;
using System.Collections.Generic;
using Tweetinvi;
using Tweetinvi.Core.Interfaces;
using Tweetinvi.Core.Interfaces.Models.Entities;
using Tweetinvi.Core.Interfaces.Streaminvi;

namespace geoecpApp.TwitterUtils
{
	public class TwitterStreamer
	{
		private static IFilteredStream _stream;

		private readonly Dictionary<string, ITweetReceiver> _receivers =
			new Dictionary<string, ITweetReceiver>(StringComparer.OrdinalIgnoreCase);

		private bool started = false;

		private IFilteredStream FilteredStream
		{
			get
			{
				if (_stream == null)
				{
					TwitterBase.SetCredentials();
					_stream = CreateStream();
				}

				return _stream;
			}
		}

		public void GetTweets(ITweetReceiver receiver)
		{
			if (_receivers.ContainsKey(receiver.Hashtag))
				return;

			StopStream();

			_receivers.Add(receiver.Hashtag, receiver);
			FilteredStream.AddTrack(receiver.Hashtag);

			if (!started)
				StartStream();
			else
				ResumeStream();
		}

		public void StopGetTweets(ITweetReceiver receiver)
		{
			if (_receivers.ContainsKey(receiver.Hashtag))
			{
				StopStream();

				_receivers.Remove(receiver.Hashtag);
				FilteredStream.Tracks.Remove(receiver.Hashtag);

				if (_receivers.Count > 0)
					ResumeStream();
			}
		}

		private void StartStream()
		{
			FilteredStream.StartStreamMatchingAllConditionsAsync();
			bool started = true;
		}

		private void PauseStream()
		{
			FilteredStream.PauseStream();
		}

		private void ResumeStream()
		{
			FilteredStream.ResumeStream();
		}

		private void StopStream()
		{
			FilteredStream.StopStream();
		}

		private IFilteredStream CreateStream()
		{
			IFilteredStream stream = Stream.CreateFilteredStream();
			stream.MatchingTweetReceived += (sender, args) => DispatchTweet(args.Tweet);

			return stream;
		}

		private void DispatchTweet(ITweet tweet)
		{
			PauseStream();

			foreach (IHashtagEntity hashtag in tweet.Hashtags)
			{
				ITweetReceiver receiver;
				if (_receivers.TryGetValue(hashtag.Text, out receiver))
					receiver.ProcessTweet(tweet);
			}

			foreach (var receiver in _receivers)
			{
				if (tweet.Text.IndexOf(receiver.Key, StringComparison.OrdinalIgnoreCase) >= 0)
					receiver.Value.ProcessTweet(tweet);
			}

			ResumeStream();
		}
	}
}