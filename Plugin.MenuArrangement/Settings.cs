using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Plugin.MenuArrangement
{
	public class Settings : INotifyPropertyChanged
	{
		private String _customMenuOrder;
		private String _hiddenMenuItems;

		[Category("UI")]
		[DisplayName("Custom Menu Order")]
		[Description("Specify the custom order of menu items as a comma-separated list of item identifiers.")]
		public String CustomMenuOrder
		{
			get => this._customMenuOrder;
			set => this.SetField(ref this._customMenuOrder, value, nameof(this.CustomMenuOrder));
		}

		[Category("UI")]
		[DisplayName("Hidden Menu Items")]
		[Description("Specify the menu items to hide as a comma-separated list of item identifiers.")]
		public String HiddenMenuItems
		{
			get => this._hiddenMenuItems;
			set => this.SetField(ref this._hiddenMenuItems, value, nameof(this.HiddenMenuItems));
		}

		internal HashSet<String> GetHiddenMenuItemsSet()
		{
			HashSet<String> hiddenItems = new HashSet<String>();
			if(String.IsNullOrEmpty(this.HiddenMenuItems))
				return hiddenItems;//No hidden items

			String[] items = this.HiddenMenuItems.Split(new Char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
			foreach(String item in items)
				hiddenItems.Add(item.Trim());
			return hiddenItems;
		}

		internal void SetHiddenMenuItemsSet(HashSet<String> hiddenItems)
		{
			if(hiddenItems == null || hiddenItems.Count == 0)
			{
				this.HiddenMenuItems = null;
				return;
			}
			this.HiddenMenuItems = String.Join(", ", hiddenItems.ToArray());
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