using System;
using System.Linq;

namespace geoecpApp.Model
{
	public class TweetMapViewModel
	{
		public TweetMapViewModel()
		{
			TweetListRows = new ThreadSaveObservableCollection<TweetListRowViewModel>();
			AddTweetListRowCommand = new RelayCommand(OnAddTweetListRow);
		}

		public ThreadSaveObservableCollection<TweetListRowViewModel> TweetListRows { get; private set; }

		public RelayCommand AddTweetListRowCommand { get; private set; }
		public event EventHandler MapPointsOfRowChanged;

		public event EventHandler RemoveMapPoints;

		private void OnAddTweetListRow(object obj)
		{
			if (obj == null)
				return;

			string hashtag = obj.ToString();
			if (string.IsNullOrEmpty(hashtag) ||
			    TweetListRows.Any(item => item.Hashtag.Equals(hashtag, StringComparison.OrdinalIgnoreCase)))
				return;

			var listRow = new TweetListRowViewModel(hashtag);
			listRow.MapPointAdded += TweetListRowMapPointAdded;
			listRow.RemoveThisRow += TweetListrowRemoveThis;

			TweetListRows.Add(listRow);
			listRow.StartStreaming();
		}

		private void TweetListrowRemoveThis(object sender, EventArgs e)
		{
			var rowToRemove = sender as TweetListRowViewModel;
			if (rowToRemove == null)
				return;

			rowToRemove.StopStreaming();
			TweetListRows.Remove(rowToRemove);
			RaisRemoveMapPoints(rowToRemove);
		}

		private void TweetListRowMapPointAdded(object sender, EventArgs e)
		{
			if (MapPointsOfRowChanged != null)
				MapPointsOfRowChanged(sender, e);
		}

		private void RaisRemoveMapPoints(TweetListRowViewModel rowViewModel)
		{
			if (RemoveMapPoints != null)
				RemoveMapPoints(rowViewModel, EventArgs.Empty);
		}
	}
}