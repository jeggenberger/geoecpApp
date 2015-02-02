using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Threading;

namespace geoecpApp.Model
{
	/// <summary>
	///     Thread save implementation of the <see cref="ObservableCollection{T}" /> class.
	/// </summary>
	/// <remarks>
	///     Code from: http://www.codeproject.com/Tips/414407/Thread-Safe-Improvement-for-ObservableCollection
	/// </remarks>
	/// <typeparam name="T">Type of the list elements.</typeparam>
	public class ThreadSaveObservableCollection<T> : ObservableCollection<T>
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="ThreadSaveObservableCollection{T}" /> class.
		/// </summary>
		public ThreadSaveObservableCollection()
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="ThreadSaveObservableCollection{T}" /> class.
		/// </summary>
		public ThreadSaveObservableCollection(IEnumerable<T> collection)
			: base(collection)
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="ThreadSaveObservableCollection{T}" /> class.
		/// </summary>
		public ThreadSaveObservableCollection(List<T> collection)
			: base(collection)
		{
		}

		/// <summary>
		///     Overriden collection changed event handler.
		/// </summary>
		public override event NotifyCollectionChangedEventHandler CollectionChanged;

		/// <summary>
		///     Overriden collection changed event handling.
		/// </summary>
		protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			// Be nice - use BlockReentrancy like MSDN said
			using (BlockReentrancy())
			{
				NotifyCollectionChangedEventHandler eventHandler = CollectionChanged;
				if (eventHandler != null)
				{
					Delegate[] delegates = eventHandler.GetInvocationList();

					// Walk thru invocation list
					foreach (NotifyCollectionChangedEventHandler handler in delegates)
					{
						var dispatcherObject = handler.Target as DispatcherObject;
						// If the subscriber is a DispatcherObject and different thread
						if (dispatcherObject != null && dispatcherObject.CheckAccess() == false)
							// Invoke handler in the target dispatcher's thread
							dispatcherObject.Dispatcher.Invoke(DispatcherPriority.DataBind, handler, this, e);
						else // Execute handler as is
							handler(this, e);
					}
				}
			}
		}
	}
}