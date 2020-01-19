using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using QBRssEditor.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Windows;
using System.Text.RegularExpressions;
using QBRssEditor.Services.KeywordEmitter;
using System.Threading;
using QBRssEditor.LocalDb;

namespace QBRssEditor
{
    class MainWindowViewModel : INotifyPropertyChanged
    {
        private string _searchText = null;
        private string _totalCount = "?";
        private string _filterdCount;
        private string _openingUrl = string.Empty;
        private GroupViewModel _selectedGroup;
        private readonly ResourceItemsService _itemsService;
        private readonly IEnumerable<IKeywordEmitter> _keywordEmitters;
        private readonly GroupingService _groupingService;

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindowViewModel(ResourceItemsService itemsService, IEnumerable<IKeywordEmitter> keywordEmitters,
            GroupingService groupingService)
        {
            this._itemsService = itemsService;
            this._keywordEmitters = keywordEmitters;
            this._groupingService = groupingService;
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
                    this.SelectedGroup = null;
                    this.OnSearchTextUpdate(value);
                }
            }
        }

        public bool IsIncludeAll { get; set; } = false;

        private async void OnSearchTextUpdate(string text)
        {
            await Task.Delay(300);
            if (text != this.SearchText) return;

            var state = new FetchState(text);
            await state.FetchAsync(this._groupingService, this.IsIncludeAll, CancellationToken.None);
            if (state.SearchText != this.SearchText) return;
            this.TotalCount = state.TotalCount;
            this.FilterdCount = state.FilterdCount;

            this.Replace(state.Items);

            var groups = new List<GroupViewModel>();
            if (state.Items.Length > 0)
            {
                groups.Add(new GroupViewModel("<all>", state.Items));
                groups.AddRange(state.GroupsMap.Select(kvp => new GroupViewModel(kvp.Key, kvp.Value)).OrderBy(z => z.GroupName));
            }
            Replace(this.Groups, groups);
        }

        class FetchState
        {
            public FetchState(string searchText)
            {
                this.SearchText = searchText;
            }

            public string SearchText { get; }

            public ResourceItem[] Items { get; private set; }

            public Dictionary<string, List<ResourceItem>> GroupsMap { get; private set; }

            public string FilterdCount { get; private set; }

            public string TotalCount { get; private set; }

            public async Task FetchAsync(GroupingService groupingService, bool includeHided, CancellationToken token)
            {
                var states = await App.ServiceProvider.GetRequiredService<ResourceItemsService>()
                    .ListAllAsync(includeHided)
                    .ConfigureAwait(false);

                if (token.IsCancellationRequested) return;

                await Task.Run(() =>
                {
                    var items = states.OrderBy(z => z.Title).ToArray();
                    this.TotalCount = items.Length.ToString();

                    if (!string.IsNullOrEmpty(this.SearchText))
                    {
                        var any = ".";
                        var regex = new Regex(
                            Regex.Escape(this.SearchText.Trim())
                                .Replace("\\*", any + "*")
                                .Replace("\\?", any), 
                            RegexOptions.IgnoreCase
                        );
                        items = items.Where(z => regex.IsMatch(z.Title)).ToArray();
                    }
                    this.Items = items;
                    this.FilterdCount = items.Length.ToString();

                    this.GroupsMap = groupingService.GetGroups(items);
                }).ConfigureAwait(false);
            }
        }

        public class GroupViewModel
        {
            public GroupViewModel(string groupName, IEnumerable<ResourceItem> items)
            {
                this.GroupName = groupName;
                this.Items = items.ToArray();
            }

            public string GroupName { get; }

            public string DisplayName => GroupName == string.Empty ? "<default>" : GroupName;

            public ResourceItem[] Items { get; }
        }

        private void Replace(IEnumerable<ResourceItem> items) => Replace(this.Items, items.Select(z => new ItemViewModel(z)));

        private static void Replace<T>(ObservableCollection<T> collection, IEnumerable<T> items)
        {
            collection.Clear();
            foreach (var item in items)
            {
                collection.Add(item);
            }
        }

        public ObservableCollection<ItemViewModel> Items { get; } = new ObservableCollection<ItemViewModel>();

        public ObservableCollection<GroupViewModel> Groups { get; } = new ObservableCollection<GroupViewModel>();

        public GroupViewModel SelectedGroup
        {
            get => _selectedGroup;
            set
            {
                if (this.ChangeValue(ref _selectedGroup, value))
                {
                    if (value != null)
                    {
                        this.Replace(value.Items);
                    }
                }
            }
        }

        public class ItemViewModel : INotifyPropertyChanged
        {
            public ItemViewModel(ResourceItem rssItem)
            {
                this.Entity = rssItem;
            }

            public ResourceItem Entity { get; }

            public string Title => this.Entity.Title;

            public Visibility ViewVisibility => this.Entity.IsHided
                ? Visibility.Visible
                : Visibility.Hidden;

            public event PropertyChangedEventHandler PropertyChanged;

            public void Updated()
            {
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.ViewVisibility)));
            }
        }

        public async void MarkReaded(IList items)
        {
            await this.MarkReadedAsync(items.OfType<ItemViewModel>().ToArray());
            await this.MarkReadedAsync(items.OfType<GroupViewModel>().ToArray());
        }

        async Task MarkReadedAsync(ItemViewModel[] viewModels)
        {
            if (viewModels.Length == 0) return;

            var rssItems = viewModels.Select(z => z.Entity).ToArray();
            await this._itemsService.HideAsync(rssItems);
            foreach (var viewModel in viewModels)
            {
                viewModel.Updated();
            }
            this.OnPropertyChanged(nameof(this.JournalCount));
        }

        async Task MarkReadedAsync(GroupViewModel[] viewModels)
        {
            if (viewModels.Length == 0) return;

            var rssItems = viewModels.SelectMany(z => z.Items).ToArray();
            await this._itemsService.HideAsync(rssItems);
            if (viewModels.Contains(this.SelectedGroup))
            {
                foreach (var vm in this.Items)
                {
                    vm.Updated();
                }
            }
            this.OnPropertyChanged(nameof(this.JournalCount));
        }

        public async void OpenUrl(IList items)
        {
            var viewModels = items.OfType<ItemViewModel>().ToArray();
            if (viewModels.Length == 0) return;

            var rssItems = viewModels.Select(z => z.Entity).ToArray();
            foreach (var item in rssItems)
            {
                var url = item.Url;
                this.OpeningUrl = url;
                using (var proc = Process.Start(item.Url))
                {
                    proc.WaitForExit(10 * 1000);
                }
                this.OpeningUrl = string.Empty;
            }
            await this.MarkReadedAsync(viewModels);
        }

        public void CopyUrl(IList items)
        {
            var viewModels = items.OfType<ItemViewModel>().ToArray();
            if (viewModels.Length == 0) return;

            var rssItems = viewModels.Select(z => z.Entity)
                .Select(z => z.Url)
                .ToArray();
            var text = string.Join("\r\n", rssItems);
            try
            {
                Clipboard.SetText(text);
            }
            catch (Exception)
            {
                MessageBox.Show("unable to copy.");
            }
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
                .SelectMany(z => z.GetKeywords(viewModel.Entity.Title))
                .Distinct()
                .Select(z => new KeywordItemViewModel { Header = z })
                .ToList()
                .ForEach(this.KeywordItems.Add);
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

        public string JournalCount => "0";

        public ObservableCollection<KeywordItemViewModel> KeywordItems { get; } = new ObservableCollection<KeywordItemViewModel>();

        public class KeywordItemViewModel
        {
            public string Header { get; set; }
        }
    }
}
