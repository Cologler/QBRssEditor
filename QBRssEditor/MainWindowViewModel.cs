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
using System.Text.RegularExpressions;

namespace QBRssEditor
{
    class MainWindowViewModel : INotifyPropertyChanged
    {
        private string _searchText = null;
        private string _totalCount = "?";
        private string _filterdCount;
        private readonly JournalService _journal;
        private readonly RssItemsService _rssItems;
        private readonly List<Func<RssItem, string>> _copyItemFactorys = new List<Func<RssItem, string>>();

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindowViewModel(JournalService journal, RssItemsService rssItems)
        {
            this._journal = journal;
            this._rssItems = rssItems;

            this._copyItemFactorys.Add(z => z.Title);
            var regex1 = new Regex("^(.*)\\.\\d{4}\\.(.*)$"); // *.2019.*
            this._copyItemFactorys.Add(z =>
            {
                var match = regex1.Match(z.Title);
                if (match.Success)
                {
                    return match.Groups[1].Value;
                }
                return null;
            });
            var regex2 = new Regex("^(.*)\\.(ep\\d{1,3}|s\\d{1,3}e\\d{1,3})\\.(.*)$", RegexOptions.IgnoreCase); // *.ep1.*
            this._copyItemFactorys.Add(z =>
            {
                var match = regex2.Match(z.Title);
                if (match.Success)
                {
                    return match.Groups[1].Value;
                }
                return null;
            });
            var regex3 = new Regex("^(.*)\\[\\d{1,3}\\](.*)$", RegexOptions.IgnoreCase); // *[01]*
            this._copyItemFactorys.Add(z =>
            {
                var match = regex3.Match(z.Title);
                if (match.Success)
                {
                    return match.Groups[1].Value;
                }
                return null;
            });
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
                var any = ".";
                var regex = new Regex(
                    Regex.Escape(text).Replace("\\*", any + "*").Replace("\\?", any), RegexOptions.IgnoreCase
                );
                items = items.Where(z => regex.IsMatch(z.Title)).ToArray();
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

            public Visibility ReadFlagVisibility => this.RssItem.IsRead
                ? Visibility.Visible
                : Visibility.Hidden;

            public event PropertyChangedEventHandler PropertyChanged;

            public void Updated()
            {
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.ReadFlagVisibility)));
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

        public void OpeningCopy(IList items)
        {
            var viewModel = items.OfType<ItemViewModel>().FirstOrDefault();
            if (viewModel == null) return;

            this.CopyItems.Clear();
            this._copyItemFactorys
                .Select(z => z(viewModel.RssItem))
                .Where(z => z != null)
                .Select(z => new CopyItemViewModel { Header = z })
                .ToList()
                .ForEach(this.CopyItems.Add);
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

        public ObservableCollection<CopyItemViewModel> CopyItems { get; } = new ObservableCollection<CopyItemViewModel>();

        public class CopyItemViewModel
        {
            public string Header { get; set; }
        }
    }
}
