using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Plugin.MenuArrangement
{
	public class Settings : INotifyPropertyChanged
	{
		private String _customMenuOrder;
		private String _hiddenMenuItems;

		public String CustomMenuOrder
		{
			get => this._customMenuOrder;
			set => this.SetField(ref this._customMenuOrder, value, nameof(this.CustomMenuOrder));
		}

		public String HiddenMenuItems
		{
			get => this._hiddenMenuItems;
			set => this.SetField(ref this._hiddenMenuItems, value, nameof(this.HiddenMenuItems));
		}

		#region INotifyPropertyChanged
		public event PropertyChangedEventHandler PropertyChanged;
		private Boolean SetField<T>(ref T field, T value, String propertyName)
		{
			if(EqualityComparer<T>.Default.Equals(field, value))
				return false;

			field = value;
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
			return true;
		}
		#endregion INotifyPropertyChanged
	}
}