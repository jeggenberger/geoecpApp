using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using geoecpApp.Model;

namespace geoecpApp
{
	/// <summary>
	///     Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private const int Wgs1984 = 4326;
		private static readonly SpatialReference Wgs1984SpatialRef = SpatialReference.Create(Wgs1984);
		private readonly GraphicsLayer _graphicsLayer;
		private bool _isHitTesting;
		private Graphic _lineGraphic;
		private Graphic _selectedGraphic;

		public MainWindow()
		{
			InitializeComponent();

			MyMapView.Map.SpatialReference = Wgs1984SpatialRef;
			_graphicsLayer = MyMapView.Map.Layers["GraphicsLayer"] as GraphicsLayer;

			ViewModel = new TweetMapViewModel();
			ViewModel.MapPointsOfRowChanged += ViewModelOnMapPointsOfRowChanged;
			ViewModel.RemoveMapPoints += ViewModelOnRemoveMapPoints;
			DataContext = ViewModel;

			DataContext = ViewModel;
		}

		private TweetMapViewModel ViewModel { get; set; }

		private void ViewModelOnMapPointsOfRowChanged(object sender, EventArgs eventArgs)
		{
			var rowViewModel = sender as TweetListRowViewModel;

			if (rowViewModel == null)
				return;

			Graphic lastGraphic = rowViewModel.Graphics.LastOrDefault();
			if (lastGraphic == null)
				return;

			_graphicsLayer.Graphics.Add(lastGraphic);
		}

		private void ViewModelOnRemoveMapPoints(object sender, EventArgs e)
		{
			var rowToRemove = sender as TweetListRowViewModel;
			if (rowToRemove == null)
				return;

			rowToRemove.Graphics.ForEach(g => _graphicsLayer.Graphics.Remove(g));
		}

		private async void MapView_MouseMove(object sender, MouseEventArgs e)
		{
			if (_isHitTesting)
				return;

			try
			{
				_isHitTesting = true;

				Point screenPoint = e.GetPosition(MyMapView);
				Graphic graphic = await _graphicsLayer.HitTestAsync(MyMapView, screenPoint);

				if (graphic != null)
				{
					mapTip.DataContext = graphic;
					mapTip.Visibility = Visibility.Visible;
				}
				else
					mapTip.Visibility = Visibility.Collapsed;
			}
			catch
			{
				mapTip.Visibility = Visibility.Collapsed;
			}
			finally
			{
				_isHitTesting = false;
			}
		}

		private async void MapView_MouseDown(object sender, MouseButtonEventArgs e)
		{
			Point screenPoint = e.GetPosition(MyMapView);
			Graphic graphic = await _graphicsLayer.HitTestAsync(MyMapView, screenPoint);


			if (_lineGraphic != null)
			{
				_graphicsLayer.Graphics.Remove(_lineGraphic);
			}

			if (graphic == null)
			{
				DistanceTextBlock.Text = "";
				return;
			}


			graphic.IsSelected = true;

			if (_selectedGraphic != null)
			{
				var mapPoint = graphic.Geometry as MapPoint;
				var selectedMapPoint = _selectedGraphic.Geometry as MapPoint;

				if (mapPoint != null && selectedMapPoint != null)
				{
					double distance = GeometryEngine.GeodesicDistance(mapPoint, selectedMapPoint, LinearUnits.Kilometers);

					DistanceTextBlock.Text = String.Format("Distance: {0:0.00}", distance);

					_lineGraphic = new Graphic(Geodesic.GetGeodesicLine(selectedMapPoint, mapPoint));
					_lineGraphic.Symbol = new SimpleLineSymbol {Color = Colors.DarkOrange, Width = 3};

					_graphicsLayer.Graphics.Add(_lineGraphic);
				}

				_selectedGraphic.IsSelected = false;
			}

			_selectedGraphic = graphic;
		}
	}
}