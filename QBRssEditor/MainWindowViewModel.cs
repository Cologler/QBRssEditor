﻿using System;
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
        private string _filterdCount;
        private readonly JournalService _journal;
        private readonly RssItemsService _rssItems;

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindowViewModel(JournalService journal, RssItemsService rssItems)
        {
            this._journal = journal;
            this._rssItems = rssItems;
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

            var states = await this._rssItems.ListAsync();
            if (text != this.SearchText) return;

            var items = states.ToArray();

            if (!this.IsIncludeAll)
            {
                items = items.Where(z => !z.IsRead).ToArray();
            }
            this.TotalCount = items.Length.ToString();

            var maxShowCount = 1000;
            if (string.IsNullOrEmpty(text))
            {
                items = items.Take(maxShowCount).ToArray();
                this.FilterdCount = items.Length > maxShowCount ? $"{maxShowCount}..." : items.Length.ToString();
            }
            else
            {
                items = items.Where(z => z.Title.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0).ToArray();
                this.FilterdCount = items.Length.ToString();
            }

            this.Replace(items);
        }

        private void Replace(IEnumerable<RssItem> items)
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
            this._rssItems.MarkReaded(rssItems);
            foreach (var viewModel in viewModels)
            {
                viewModel.Updated();
            }
            await this._rssItems.FlushAsync();
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

        public void AsSearchText(IList items)
        {
            var viewModel = items.OfType<ItemViewModel>().FirstOrDefault();
            if (viewModel == null) return;
            this.SearchText = viewModel.RssItem.Title ?? string.Empty;
        }

        public void CopyToClipboard(IList items)
        {
            var viewModel = items.OfType<ItemViewModel>().FirstOrDefault();
            if (viewModel == null) return;
            var title = viewModel.RssItem.Title;
            try
            {
                Clipboard.SetText(title);
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to copy to clipboard.");
            }
        }

        public async Task FlushAsync()
        {
            await this._rssItems.FlushAsync();
            this.OnPropertyChanged(nameof(this.JournalCount));
        }

        public string TotalCount
        {
            get => this._totalCount;
            set => this.ChangeValue(ref this._totalCount, value);
        }

        public string FilterdCount
        {
            get => this._filterdCount;
            set => this.ChangeValue(ref this._filterdCount, value);
        }

        public string JournalCount => this._journal.Count.ToString();
    }
}
