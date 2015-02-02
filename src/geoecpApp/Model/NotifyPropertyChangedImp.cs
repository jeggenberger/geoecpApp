using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace geoecpApp.Model
{
	/// <summary>
	///     Default implementation of the <see cref="INotifyPropertyChanged" /> interface
	///     used for the viewmodel of the avgbs media configuration control.
	/// </summary>
	public abstract class NotifyPropertyChangedImp : INotifyPropertyChanged
	{
		/// <summary>
		///     Event raised when a property is changed.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		///     Raises the <see cref="PropertyChanged" /> event for multiply properties.
		/// </summary>
		/// <param name="propertyNames">Multiple names of properties that where changed.</param>
		protected void OnPropertiesChanged(params string[] propertyNames)
		{
			if (PropertyChanged != null && propertyNames.Length > 0)
			{
				foreach (string propertyName in propertyNames)
				{
					PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
				}
			}
		}

		/// <summary>
		///     Raises the <see cref="PropertyChanged" /> event for one specified property.
		///     This function can be used if the change of a property does also change
		///     another one.
		/// </summary>
		/// <param name="propertyName">Name of the property that is changed.</param>
		protected void OnOtherPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		/// <summary>
		///     Raises the <see cref="PropertyChanged" /> event for the caller.
		/// </summary>
		/// <param name="propertyName">Name of the caller will be used (e.g. name of the property that uses this method).</param>
		protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}