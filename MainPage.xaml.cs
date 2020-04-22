using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace MartinZikmunOnRx
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class MainPage : Page
	{
		public MainPage()
		{
			this.InitializeComponent();
			Unloaded += (sender, e) => ViewModel.Dispose();
		}
		

		public ViewModel ViewModel { get; } = new ViewModel();
	}

	public class ViewModel : INotifyPropertyChanged, IDisposable
	{
	 readonly	IDisposable _Subscription;

		public ViewModel()
		{
			_Subscription =
				Observable
				.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
					h => PropertyChanged += h,
					h => PropertyChanged -= h)
				.Throttle(TimeSpan.FromSeconds(.7))
				.Where(ep => ep.EventArgs.PropertyName == nameof(ViewModel.SearchTerm))
				.Select(ep => ((ViewModel)ep.Sender).SearchTerm)
				.Select(term => term.Trim())
				.Where(term => term.Length == 0 || term.Length >= 2)
				.DistinctUntilChanged()
				.StartWith(default(string))
				.SelectMany(term => LoadNames(term))
				.ObserveOnDispatcher()
				.Subscribe(names => UpdateValues(names));
		}

		public event PropertyChangedEventHandler PropertyChanged;

		string _SearchTerm;
		public string SearchTerm
		{
			get => _SearchTerm;
			set
			{
				if (value == null)
				{
					value = string.Empty;
				}


				if (_SearchTerm == value)
				{
					return;
				}

				_SearchTerm = value.Trim();
				RaisePropertyChanged();
			}
		}

		private readonly string[] NamesSource = new string[] {
			"Martin",
			"Shimmy",
			"Josh",
			"John",
			"Jerome",
			"Francois",
			"Sasha",
			"Billy"
		};

		public IList<string> Names { get; } = new ObservableCollection<string>();

		private void RaisePropertyChanged([CallerMemberName]string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public void UpdateValues(IEnumerable<string> serverValues)
		{
			Names.Clear();
			foreach (var name in serverValues)
			{
				Names.Add(name);
			}
		}

		private async Task<IEnumerable<string>> LoadNames(string searchTerm = null)
		{
			await Task.Delay(500);

			var skipFilter = string.IsNullOrWhiteSpace(searchTerm);

			if (skipFilter)
			{
				return NamesSource;
			}
			else
			{
				return NamesSource
					.Where(name => name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
			}
		}

		public void Dispose() => _Subscription.Dispose();
	}
}
