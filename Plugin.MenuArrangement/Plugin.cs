using System;
using System.Collections.Generic;
using System.Diagnostics;
using Plugin.MenuArrangement.Logic;
using SAL.Flatbed;
using SAL.Windows;

namespace Plugin.MenuArrangement
{
	public class Plugin : IPlugin, IPluginSettings<Settings>
	{
		private Settings _settings;
		private TraceSource _trace;
		private Dictionary<String, DockState> _documentTypes;

		internal TraceSource Trace => this._trace ?? (this._trace = Plugin.CreateTraceSource<Plugin>());

		internal IHostWindows HostWindows { get; }

		private IMenuItem ArrangeMenu { get; set; }

		Object IPluginSettings.Settings => this.Settings;

		public Settings Settings
		{
			get
			{
				if(this._settings == null)
				{
					this._settings = new Settings();
					this.HostWindows.Plugins.Settings(this).LoadAssemblyParameters(this._settings);
				}
				return this._settings;
			}
		}

		private Dictionary<String, DockState> DocumentTypes
		{
			get
			{
				if(this._documentTypes == null)
					this._documentTypes = new Dictionary<String, DockState>()
					{
						{ typeof(PanelMenuArrangement).ToString(), DockState.DockLeftAutoHide },
					};
				return this._documentTypes;
			}
		}

		public Plugin(IHostWindows hostWindows)
			=> this.HostWindows = hostWindows ?? throw new ArgumentNullException(nameof(hostWindows));

		public IWindow GetPluginControl(String typeName, Object args)
			=> this.CreateWindow(typeName, false, args);

		Boolean IPlugin.OnConnection(ConnectMode mode)
		{
			IMenuItem menuView = this.HostWindows.MainMenu.FindMenuItem("View");
			if(menuView == null)
			{
				this.Trace.TraceEvent(TraceEventType.Error, 10, "Menu item 'View' not found");
				return false;
			}

			this.HostWindows.Plugins.PluginsLoaded += this.Host_PluginsLoaded;
			this.ArrangeMenu = menuView.Create("&Arrange Menu...");
			this.ArrangeMenu.Name = "View.ArrangeMenu";
			this.ArrangeMenu.Click += (sender, e) => this.CreateWindow(typeof(PanelMenuArrangement).ToString(), true);
			menuView.Items.Insert(0, this.ArrangeMenu);
			return true;
		}

		Boolean IPlugin.OnDisconnection(DisconnectMode mode)
		{
			if(this.ArrangeMenu != null)
				this.HostWindows.MainMenu.Items.Remove(this.ArrangeMenu);
			return true;
		}

		private void Host_PluginsLoaded(Object sender, EventArgs e)
		{
			// Restore custom menu order if available
			CustomMenuOrderStorage.RestoreMenuOrder(this);
			CustomMenuOrderStorage.RestoreHiddenItems(this);
		}

		internal IWindow CreateWindow(String typeName, Boolean searchForOpened, Object args = null)
			=> this.DocumentTypes.TryGetValue(typeName, out DockState state)
				? this.HostWindows.Windows.CreateWindow(this, typeName, searchForOpened, state, args)
				: null;

		private static TraceSource CreateTraceSource<T>(String name = null) where T : IPlugin
		{
			TraceSource result = new TraceSource(typeof(T).Assembly.GetName().Name + name);
			result.Switch.Level = SourceLevels.All;
			result.Listeners.Remove("Default");
			result.Listeners.AddRange(System.Diagnostics.Trace.Listeners);
			return result;
		}
	}
}