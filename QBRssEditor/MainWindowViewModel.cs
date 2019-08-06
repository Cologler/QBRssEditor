using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.Packaging;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Documents;
using QBRssEditor.Model;
using QBRssEditor.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Windows;

namespace QBRssEditor
{
    class MainWindowViewModel : INotifyPropertyChanged
    {
        private string _searchText = null;
        private string _totalCount = "?";
        private readonly JournalService _journal;

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindowViewModel(JournalService journal)
        {
            this._journal = journal;
        }

        void OnPropertyChanged([CallerMemberName] string propertyName = "") =>
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        bool ChangeValue<T>(ref T field, T newValue, [CallerMemberName] string propertyName = "")
        {
            if (!EqualityComparer<T>.Default.Equals(field, newValue))
            {
                field = newValue;
                this.OnPropertyChanged(propertyName);
                return true;
            }
            return false;
        }

        public string SearchText
        {
            get => this._searchText;
            set
            {
                if (this.ChangeValue(ref this._searchText, value))
                {
                    this.OnSearchTextUpdate(value);
                }
            }
        }

        public bool IsIncludeAll { get; set; } = false;

        private async void OnSearchTextUpdate(string text)
        {
            await Task.Delay(300);
            if (text != this.SearchText) return;

            var service = App.ServiceProvider.GetRequiredService<RssItemsService>();

            var items = await service.ListAsync();
            var source = items.AsEnumerable();
            if (text != this.SearchText) return;

            if (!this.IsIncludeAll)
            {
                source = source.Where(z => !z.IsRead);
            }

            if (!string.IsNullOrEmpty(text))
            {
                source = source.Where(z => z.Title.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0);
            }
            var result = source.ToList();

            if (string.IsNullOrEmpty(text) && result.Count > 300)
            {
                this.TotalCount = $"300/{result.Count}";
            }
            else
            {
                this.TotalCount = result.Count.ToString();
            }
            
            this.UpdateItems(string.IsNullOrEmpty(text) ? result.Take(300) : result);
        }

        private void UpdateItems(IEnumerable<RssItem> items)
        {
            this.Items.Clear();
            foreach (var item in items)
            {
                this.Items.Add(new ItemViewModel(item));
            }
        }

        public ObservableCollection<ItemViewModel> Items { get; } = new ObservableCollection<ItemViewModel>();

        public class ItemViewModel : INotifyPropertyChanged
        {
            public ItemViewModel(RssItem rssItem)
            {
                this.RssItem = rssItem;
            }

            public RssItem RssItem { get; }

            public string Title => this.RssItem.Title;

            public Visibility IsReadFlagVisibility => this.RssItem.IsRead
                ? Visibility.Visible
                : Visibility.Hidden;

            public event PropertyChangedEventHandler PropertyChanged;

            public void Updated()
            {
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.IsReadFlagVisibility)));
            }
        }

        public async void MarkReaded(IList items)
        {
            var viewModels = items.OfType<ItemViewModel>().ToArray();
            if (viewModels.Length == 0) return;
            var rssItems = viewModels.Select(z => z.RssItem).ToArray();
            var service = App.ServiceProvider.GetRequiredService<RssItemsService>();
            service.MarkReaded(rssItems);
            foreach (var viewModel in viewModels)
            {
                viewModel.Updated();
            }
            await service.FlushAsync();
            this.OnPropertyChanged(nameof(this.JournalCount));
        }

        public void OpenTorrentUrl(IList items)
        {
            var rssItems = items.OfType<ItemViewModel>().Select(z => z.RssItem).ToArray();
            if (rssItems.Length == 0) return;
            foreach (var item in rssItems)
            {
                using (var proc = Process.Start(item.TorrentUrl))
                {
                    proc.WaitForExit();
                }
            }
        }

        internal void AsSearchText(IList items)
        {
            var viewModel = items.OfType<ItemViewModel>().FirstOrDefault();
            if (viewModel == null) return;
            this.SearchText = viewModel.RssItem.Title ?? string.Empty;
        }

        public int JournalCount => this._journal.Count;

        public string TotalCount
        {
            get => this._totalCount;
            set => this.ChangeValue(ref this._totalCount, value);
        }
    }
}
