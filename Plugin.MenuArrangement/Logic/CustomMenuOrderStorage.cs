using System;
using System.Collections.Generic;
using System.Windows.Forms;
using SAL.Flatbed;
using SAL.Windows;

namespace Plugin.MenuArrangement.Logic
{
	internal static class CustomMenuOrderStorage
	{
		/// <summary>Save current menu order to settings</summary>
		public static void SaveMenuOrder(Plugin plugin)
		{
			String serializedOrder = SerializeMenuOrder(plugin.HostWindows.MainMenu);
			plugin.Settings.CustomMenuOrder = serializedOrder;
		}

		/// <summary>Restore menu order from settings</summary>
		public static void RestoreMenuOrder(Plugin plugin)
		{
			String menuOrder = plugin.Settings.CustomMenuOrder;
			if(String.IsNullOrEmpty(menuOrder))
				return; // No custom order saved

			MenuOrderRestorer restorer = new MenuOrderRestorer(plugin.HostWindows.MainMenu);
			restorer.DeserializeAndApply(menuOrder);
		}

		/// <summary>Restore hidden items visibility from settings</summary>
		public static void RestoreHiddenItems(Plugin plugin)
		{
			HashSet<String> hiddenItems = plugin.Settings.GetHiddenMenuItemsSet();
			if(hiddenItems.Count == 0)
				return;

			MenuVisibilityRestorer restorer = new MenuVisibilityRestorer(plugin.HostWindows.MainMenu);
			restorer.ApplyHiddenItems(hiddenItems);
		}

		/// <summary>Serialize menu order to string</summary>
		private static String SerializeMenuOrder(IMenu menu)
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			SerializeMenuItems(menu.Items, sb, 0);
			return sb.ToString();
		}

		/// <summary>Serialize menu items recursively</summary>
		private static void SerializeMenuItems(IMenuItemCollection items, System.Text.StringBuilder sb, Int32 level)
		{
			foreach(IMenuItem item in items)
			{
				// Get menu item name or text as identifier
				String identifier = !String.IsNullOrEmpty(item.Name) ? item.Name : item.Text;
				if(String.IsNullOrEmpty(identifier))
					identifier = "_separator_"; // Handle separators

				// Add level indicator and identifier
				sb.Append(level).Append(':').Append(identifier).Append(';');

				// Recursively serialize children
				if(item is IMenu subMenu && subMenu.IsDropDown)
					SerializeMenuItems(subMenu.Items, sb, level + 1);
			}
		}
	}

	/// <summary>Helper class for restoring menu order with root menu stored as instance field</summary>
	internal class MenuOrderRestorer
	{
		private readonly IMenu _rootMenu;
		private Dictionary<String, IMenuItem> _menuItemMap;

		public MenuOrderRestorer(IMenu rootMenu)
		{
			this._rootMenu = rootMenu ?? throw new ArgumentNullException(nameof(rootMenu));
		}

		/// <summary>Deserialize and apply menu order</summary>
		public void DeserializeAndApply(String menuOrder)
		{
			if(String.IsNullOrEmpty(menuOrder))
				return;

			// Parse the menu order string
			String[] entries = menuOrder.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

			// Build a map of menu items by identifier for quick lookup
			this._menuItemMap = new Dictionary<String, IMenuItem>();
			BuildMenuItemMap(this._rootMenu.Items, this._menuItemMap);

			// Apply the order hierarchically
			ApplyMenuOrderFromEntries(this._rootMenu, entries, 0);
		}

		/// <summary>Build a map of all menu items by their identifier</summary>
		private void BuildMenuItemMap(IMenuItemCollection items, Dictionary<String, IMenuItem> map)
		{
			foreach(IMenuItem item in items)
			{
				String identifier = !String.IsNullOrEmpty(item.Name) ? item.Name : item.Text;
				if(String.IsNullOrEmpty(identifier))
					identifier = "_separator_";

				if(!map.ContainsKey(identifier))
					map[identifier] = item;

				// Recursively map children
				if(item is IMenu subMenu && subMenu.IsDropDown)
				{
					BuildMenuItemMap(subMenu.Items, map);
				}
			}
		}

		/// <summary>Apply menu order from parsed entries hierarchically</summary>
		private void ApplyMenuOrderFromEntries(IMenu menu, String[] entries, Int32 currentLevel)
		{
			List<IMenuItem> itemsAtCurrentLevel = new List<IMenuItem>();
			Int32 index = 0;

			// Parse entries and collect items at current level
			while(index < entries.Length)
			{
				String entry = entries[index];
				String[] parts = entry.Split(':');
				if(parts.Length != 2)
				{
					index++;
					continue;
				}

				Int32 level = Int32.Parse(parts[0]);
				String identifier = parts[1];

				// If we've moved past current level, stop
				if(level < currentLevel)
					break;

				// If this is current level, add to list
				if(level == currentLevel && this._menuItemMap.ContainsKey(identifier))
				{
					IMenuItem item = this._menuItemMap[identifier];
					itemsAtCurrentLevel.Add(item);
					index++;

					// If this item has children (is a menu), recursively reorder them
					if(item is IMenu subMenu && subMenu.IsDropDown)
					{
						// Find the range of entries that are children of this item
						List<String> childEntries = new List<String>();
						while(index < entries.Length)
						{
							String nextEntry = entries[index];
							String[] nextParts = nextEntry.Split(':');
							if(nextParts.Length == 2)
							{
								Int32 nextLevel = Int32.Parse(nextParts[0]);
								if(nextLevel <= currentLevel)
									break; // No more children
								childEntries.Add(nextEntry);
							}
							index++;
						}

						// Recursively apply order to children
						if(childEntries.Count > 0)
						{
							ApplyMenuOrderFromEntries(subMenu, childEntries.ToArray(), currentLevel + 1);
						}
					}
				} else
				{
					index++;
				}
			}

			// Reorder items at current level by moving them to correct positions
			// This preserves items not in the saved order at their original positions
			// Items can be moved from different levels, so we search the entire hierarchy
			if(itemsAtCurrentLevel.Count > 0)
			{
				for(Int32 i = 0; i < itemsAtCurrentLevel.Count; i++)
				{
					IMenuItem item = itemsAtCurrentLevel[i];

					// Find where item currently exists (search from root to find it anywhere)
					IMenu currentParent = null;
					Int32 currentIndex = -1;
					FindItemLocation(this._rootMenu, item, ref currentParent, ref currentIndex);

					// Check if item is already at correct position in this menu
					Boolean isAtCorrectPosition = false;
					if(currentParent.Object == menu.Object && currentIndex == i)
					{
						isAtCorrectPosition = true;
					}

					// If item needs to be moved
					if(!isAtCorrectPosition && currentParent != null && currentIndex >= 0)
					{
						// Remove from current location (could be different parent/level)
						currentParent.Items.RemoveAt(currentIndex);

						// Insert at correct position in this menu
						if(i <= menu.Items.Count)
							menu.Items.Insert(i, item);
						else
							menu.Items.Add(item);
					}
				}
			}
		}

		/// <summary>Find the location of an item anywhere in the menu hierarchy</summary>
		private Boolean FindItemLocation(IMenu menu, IMenuItem itemToFind, ref IMenu parentMenu, ref Int32 index)
		{
			// Search at current level
			for(Int32 i = 0; i < menu.Items.Count; i++)
			{
				if(menu.Items[i].Object == itemToFind.Object)
				{
					parentMenu = menu;
					index = i;
					return true;
				}
			}

			// Recursively search in submenus
			foreach(IMenuItem item in menu.Items)
			{
				if(item is IMenu subMenu && subMenu.IsDropDown)
				{
					if(FindItemLocation(subMenu, itemToFind, ref parentMenu, ref index))
						return true;
				}
			}

			return false;
		}
	}

	/// <summary>Helper class for restoring menu visibility with root menu stored as instance field</summary>
	internal class MenuVisibilityRestorer
	{
		private readonly IMenu _rootMenu;

		public MenuVisibilityRestorer(IMenu rootMenu)
		{
			this._rootMenu = rootMenu ?? throw new ArgumentNullException(nameof(rootMenu));
		}

		/// <summary>Apply hidden items visibility</summary>
		public void ApplyHiddenItems(HashSet<String> hiddenItems)
		{
			ApplyHiddenItemsRecursive(this._rootMenu.Items, hiddenItems);
		}

		/// <summary>Apply hidden state recursively</summary>
		private void ApplyHiddenItemsRecursive(IMenuItemCollection items, HashSet<String> hiddenItems)
		{
			foreach(IMenuItem item in items)
			{
				String identifier = !String.IsNullOrEmpty(item.Name) ? item.Name : item.Text;

				// Recursively process children FIRST (depth-first)
				if(item is IMenu subMenu && subMenu.IsDropDown)
				{
					ApplyHiddenItemsRecursive(subMenu.Items, hiddenItems);
				}

				// Then apply visibility to this item
				if(!String.IsNullOrEmpty(identifier))
				{
					// Access the underlying ToolStripItem to set visibility
					if(item is IHostItem hostItem && hostItem.Object is ToolStripItem toolStripItem)
					{
						// Only hide if this specific item is in the hidden list
						if(hiddenItems.Contains(identifier))
						{
							toolStripItem.Visible = false;
						}
					}
				}
			}
		}
	}
}