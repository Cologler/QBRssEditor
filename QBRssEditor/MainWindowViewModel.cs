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
using System.Text.RegularExpressions;
using QBRssEditor.Services.KeywordEmitter;
using System.Threading;

namespace QBRssEditor
{
    class MainWindowViewModel : INotifyPropertyChanged
    {
        private string _searchText = null;
        private string _totalCount = "?";
        private string _filterdCount;
        private string _openingUrl = string.Empty;
        private readonly JournalService _journal;
        private readonly RssItemsService _rssItems;
        private readonly IEnumerable<IKeywordEmitter> _keywordEmitters;

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindowViewModel(JournalService journal, RssItemsService rssItems, IEnumerable<IKeywordEmitter> keywordEmitters)
        {
            this._journal = journal;
            this._rssItems = rssItems;
            this._keywordEmitters = keywordEmitters;
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

            var fetch = new FetchState(text);
            await fetch.FetchAsync(this._rssItems, this.IsIncludeAll, CancellationToken.None);
            if (fetch.SearchText != this.SearchText) return;
            this.TotalCount = fetch.TotalCount;
            this.FilterdCount = fetch.FilterdCount;
            this.Replace(fetch.Items);
        }

        class FetchState
        {
            public FetchState(string searchText)
            {
                this.SearchText = searchText;
            }

            public string SearchText { get; }

            public RssItem[] Items { get; private set; }

            public string FilterdCount { get; private set; }

            public string TotalCount { get; private set; }

            public async Task FetchAsync(RssItemsService rssItemsService, bool isIncludeAll, CancellationToken token)
            {
                var states = await rssItemsService.ListAsync();
                if (token.IsCancellationRequested) return;
                this.Items = await Task.Run(() =>
                {
                    var items = states.ToArray();

                    if (!isIncludeAll)
                    {
                        items = items.Where(z => !z.IsRead).ToArray();
                    }
                    this.TotalCount = items.Length.ToString();

                    var maxShowCount = 1000;
                    if (string.IsNullOrEmpty(this.SearchText))
                    {
                        items = items.Take(maxShowCount).OrderBy(z => z.Title).ToArray();
                        this.FilterdCount = items.Length > maxShowCount ? $"{maxShowCount}..." : items.Length.ToString();
                    }
                    else
                    {
                        var any = ".";
                        var regex = new Regex(
                            Regex.Escape(this.SearchText).Replace("\\*", any + "*").Replace("\\?", any), RegexOptions.IgnoreCase
                        );
                        items = items.Where(z => regex.IsMatch(z.Title)).OrderBy(z => z.Title).ToArray();
                        this.FilterdCount = items.Length.ToString();
                    }
                    return items;
                });
            }
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

            public Visibility ReadFlagVisibility => this.RssItem.IsRead
                ? Visibility.Visible
                : Visibility.Hidden;

            public event PropertyChangedEventHandler PropertyChanged;

            public void Updated()
            {
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.ReadFlagVisibility)));
            }
        }

        public async void MarkReaded(IList items) => await this.MarkReaded(items.OfType<ItemViewModel>().ToArray());

        async Task MarkReaded(ItemViewModel[] viewModels)
        {
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

        public async void OpenTorrentUrl(IList items)
        {
            var viewModels = items.OfType<ItemViewModel>().ToArray();
            if (viewModels.Length == 0) return;

            var rssItems = viewModels.Select(z => z.RssItem).ToArray();
            foreach (var item in rssItems)
            {
                var url = item.TorrentUrl;
                this.OpeningUrl = url;
                using (var proc = Process.Start(item.TorrentUrl))
                {
                    proc.WaitForExit();
                }
                this.OpeningUrl = string.Empty;
            }
            await this.MarkReaded(viewModels);
        }

        public string OpeningUrl
        {
            get => _openingUrl;
            private set => this.ChangeValue(ref this._openingUrl, value);
        }

        public void OpeningCopy(IList items)
        {
            var viewModel = items.OfType<ItemViewModel>().FirstOrDefault();
            if (viewModel == null) return;

            this.KeywordItems.Clear();
            this._keywordEmitters
                .SelectMany(z => z.GetKeywords(viewModel.RssItem.Title))
                .Distinct()
                .Select(z => new KeywordItemViewModel { Header = z })
                .ToList()
                .ForEach(this.KeywordItems.Add);
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

        public ObservableCollection<KeywordItemViewModel> KeywordItems { get; } = new ObservableCollection<KeywordItemViewModel>();

        public class KeywordItemViewModel
        {
            public string Header { get; set; }
        }
    }
}
