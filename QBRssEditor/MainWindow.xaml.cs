﻿using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using QBRssEditor.Abstractions;
using QBRssEditor.Model;

namespace QBRssEditor
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.OnServiceProviderInit(App.ServiceProvider);
        }

        private void OnServiceProviderInit(IServiceProvider serviceProvider)
        {
            this.ViewModel = serviceProvider.GetService<MainWindowViewModel>();
            this.ViewModel.SearchText = string.Empty;

            var settings = serviceProvider.GetRequiredService<AppSettings>();
            var orps = serviceProvider.GetServices<IResourceProvider>()
                .OfType<IOptionalResourceProvider>()
                .ToArray();
            foreach (var orp in orps)
            {
                var mi = new MenuItem
                {
                    IsCheckable = true,
                    Header = orp.Name,
                    IsChecked = settings.GetProviderSettings(orp.Name).IsEnabled
                };
                mi.Click += (_, _2) =>
                {
                    settings.GetProviderSettings(orp.Name).IsEnabled = mi.IsChecked;
                };
                this.ProvidersMenuItem.Items.Add(mi);
            }
        }

        MainWindowViewModel ViewModel
        {
            get => this.DataContext as MainWindowViewModel;
            set => this.DataContext = value;
        }

        private void ItemMarkHidedMenuItem_Click(object sender, RoutedEventArgs e) => 
            this.ViewModel.MarkHided(this.ItemsListView.SelectedItems);

        private async void Flush_Click(object sender, RoutedEventArgs e) { }

        private void ListViewMenuItem_ContextMenuOpening(object sender, ContextMenuEventArgs e) => 
            this.ViewModel.OpeningCopy(this.ItemsListView.SelectedItems);

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = ((FrameworkElement)e.OriginalSource).DataContext as MainWindowViewModel.KeywordItemViewModel;
            var header = viewModel?.Header;
            if (!string.IsNullOrEmpty(header))
            {
                try
                {
                    Clipboard.SetText(header);
                }
                catch (Exception)
                {
                    MessageBox.Show("Unable to copy to clipboard.");
                }
            }
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = ((FrameworkElement)e.OriginalSource).DataContext as MainWindowViewModel.KeywordItemViewModel;
            var header = viewModel?.Header;
            if (!string.IsNullOrEmpty(header))
            {
                this.ViewModel.SearchText = header;
            }
        }

        private void GroupMarkHidedMenuItem_Click(object sender, RoutedEventArgs e) => 
            this.ViewModel.MarkHided(this.GroupsListView.SelectedItems);

        private void OpenUrlMenuItem_Click(object sender, RoutedEventArgs e) =>
            this.ViewModel.OpenUrl(this.ItemsListView.SelectedItems);

        private void CopyUrlMenuItem_Click(object sender, RoutedEventArgs e) =>
            this.ViewModel.CopyUrl(this.ItemsListView.SelectedItems);
    }
}
