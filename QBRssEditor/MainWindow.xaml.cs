﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using QBRssEditor.Model;
using QBRssEditor.Services;
using Microsoft.Extensions.DependencyInjection;

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
            this.ViewModel = App.ServiceProvider.GetService<MainWindowViewModel>();
            this.ViewModel.SearchText = string.Empty;
        }

        MainWindowViewModel ViewModel
        {
            get => this.DataContext as MainWindowViewModel;
            set => this.DataContext = value;
        }

        private void MarkReadMenuItem_Click(object sender, RoutedEventArgs e) => 
            this.ViewModel.MarkReaded(this.ItemsListView.SelectedItems);

        private void OpenTorrentUrlMenuItem_Click(object sender, RoutedEventArgs e) => 
            this.ViewModel.OpenTorrentUrl(this.ItemsListView.SelectedItems);

        private void AsSearchTextMenuItem_Click(object sender, RoutedEventArgs e) => 
            this.ViewModel.AsSearchText(this.ItemsListView.SelectedItems);

        private async void Flush_Click(object sender, RoutedEventArgs e) => 
            await this.ViewModel.FlushAsync();

        private void CopyToClipboard_Click(object sender, RoutedEventArgs e) =>
            this.ViewModel.CopyToClipboard(this.ItemsListView.SelectedItems);
    }
}
