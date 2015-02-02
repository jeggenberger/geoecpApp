using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using geoecpApp.TwitterUtils;
using Tweetinvi.Core.Interfaces;
using Geometry = Esri.ArcGISRuntime.Geometry.Geometry;

namespace geoecpApp.Model
{
	public class TweetListRowViewModel : NotifyPropertyChangedImp, ITweetReceiver
	{
		private const int Wgs1984 = 4326;
		private static readonly SpatialReference TweetSpatialRef = SpatialReference.Create(Wgs1984);
		private static readonly SpatialReference WebMercatorSpatialRef = SpatialReference.Create(102100);
		private static TwitterStreamer streamer;

		private readonly List<Graphic> _graphics = new List<Graphic>();

		private readonly object _mapPointLock = new object();
		private readonly List<MapPoint> _mapPoints = new List<MapPoint>();
		private readonly HashSet<long> _tweetIds = new HashSet<long>();
		private double _avgDistance;

		private double _distanceSum;
		private double _maxDistance;
		private double _minDistance;

		public TweetListRowViewModel(string hashtag)
		{
			Hashtag = hashtag;
			CreateRandomColorPointSymbol();
			RemoveTweetListRowCommand = new RelayCommand(RaiseRemoveThisRow);

			if (streamer == null)
				streamer = new TwitterStreamer();
		}

		public Symbol PointSymbol { get; private set; }

		public Color Color { get; private set; }

		public Brush Brush { get; private set; }

		public int TweetCount
		{
			get { return _tweetIds.Count; }
		}

		public int TweetCountNoCoords
		{
			get { return _tweetIds.Count - _graphics.Count; }
		}

		public int TweetCountWithCoords
		{
			get { return _graphics.Count; }
		}

		public List<Graphic> Graphics
		{
			get { return _graphics; }
		}

		public double DistanceSum
		{
			get { return _distanceSum; }
			private set
			{
				if (Math.Abs(_distanceSum - value) > 0)
				{
					_distanceSum = value;
					OnPropertyChanged();
				}
			}
		}

		public double MaxDistance
		{
			get { return _maxDistance; }
			private set
			{
				if (Math.Abs(_maxDistance - value) > 0)
				{
					_maxDistance = value;
					OnPropertyChanged();
				}
			}
		}

		public double MinDistance
		{
			get { return _minDistance; }
			private set
			{
				if (Math.Abs(_minDistance - value) > 0)
				{
					_minDistance = value;
					OnPropertyChanged();
				}
			}
		}

		public double AvgDistance
		{
			get { return _avgDistance; }
			private set
			{
				if (Math.Abs(_avgDistance - value) > 0)
				{
					_avgDistance = value;
					OnPropertyChanged();
				}
			}
		}

		public RelayCommand RemoveTweetListRowCommand { get; private set; }

		private TwitterStream Stream { get; set; }
		public string Hashtag { get; private set; }

		public void ProcessTweet(ITweet tweet)
		{
			if (!_tweetIds.Contains(tweet.Id))
			{
				_tweetIds.Add(tweet.Id);
				OnOtherPropertyChanged("TweetCount");
				UpdateMapGraphics(tweet);
			}
		}

		public event EventHandler MapPointAdded;

		public event EventHandler RemoveThisRow;

		public void StartStreaming()
		{
			////streamer.GetTweets(this);
			if (Stream != null)
				return;

			Stream = new TwitterStream(Hashtag);
			Stream.TweetAdded += StreamOnTweetAdded;
			Stream.StartStream();
		}

		private static MapPoint CreateMapPoint(ITweet tweet)
		{
			return new MapPoint(tweet.Coordinates.Longitude, tweet.Coordinates.Latitude, TweetSpatialRef);
		}

		private Graphic CreateGraphic(ITweet tweet)
		{
			MapPoint mapPoint = CreateMapPoint(tweet);
			_mapPoints.Add(mapPoint);

			Geometry mp = GeometryEngine.Project(mapPoint, WebMercatorSpatialRef);

			var g = new Graphic(mp, PointSymbol);

			g.Attributes.Add("creator", tweet.Creator.Name);
			g.Attributes.Add("text", tweet.Text);

			return g;
		}

		private void StreamOnTweetAdded(object sender, ITweet tweet)
		{
			if (!_tweetIds.Contains(tweet.Id))
			{
				_tweetIds.Add(tweet.Id);
				OnOtherPropertyChanged("TweetCount");
				UpdateMapGraphics(tweet);
			}
		}

		private void UpdateMapGraphics(ITweet tweet)
		{
			if (tweet.Coordinates == null)
			{
				OnOtherPropertyChanged("TweetCountNoCoords");
				return;
			}

			lock (_mapPointLock)
			{
				_graphics.Add(CreateGraphic(tweet));
			}

			OnOtherPropertyChanged("TweetCountWithCoords");
			RaiseMapGraphicAdded();
			CalculateDistances();
		}

		public void StopStreaming()
		{
			////streamer.StopGetTweets(this);
			if (Stream != null)
				Stream.StopStream();
		}

		private void CreateRandomColorPointSymbol()
		{
			var rand = new Random();
			var colorParts = new byte[3];
			rand.NextBytes(colorParts);

			Color = new Color
			{
				A = 255,
				R = (byte) rand.Next(50, 200),
				G = (byte) rand.Next(50, 200),
				B = (byte) rand.Next(50, 200)
			};

			Brush = new SolidColorBrush(Color);

			PointSymbol = new SimpleMarkerSymbol
			{
				Color = Color,
				Style = SimpleMarkerStyle.X,
				Size = 12
			};
		}

		private void RaiseMapGraphicAdded()
		{
			if (MapPointAdded != null)
				MapPointAdded(this, EventArgs.Empty);
		}

		private void RaiseRemoveThisRow(object obj)
		{
			if (RemoveThisRow != null)
				RemoveThisRow(this, EventArgs.Empty);
		}

		private void CalculateDistances()
		{
			double sum = 0;
			double maxDistance = 0;
			double minDistance = double.MaxValue;
			int distanceCount = 0;

			Task task = Task.Factory.StartNew(() =>
			{
				lock (_mapPointLock)
				{
					for (int i = 0; i < _mapPoints.Count - 1; i++)
					{
						MapPoint outer = _mapPoints[i];

						for (int j = i + 1; j < _mapPoints.Count; j++)
						{
							MapPoint inner = _mapPoints[j];

							double distance = GeometryEngine.GeodesicDistance(outer, inner, LinearUnits.Kilometers);
							sum += distance;
							if (maxDistance < distance)
								maxDistance = distance;
							if (minDistance > distance)
								minDistance = distance;

							distanceCount++;
						}
					}
				}
			});

			task.Wait();

			DistanceSum = sum;
			MaxDistance = maxDistance;
			MinDistance = minDistance == double.MaxValue ? 0 : minDistance;
			if (sum > 0 && distanceCount > 0)
				AvgDistance = sum/distanceCount;
		}
	}
}