using System;
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

        private void ItemMarkReadMenuItem_Click(object sender, RoutedEventArgs e) => 
            this.ViewModel.MarkReaded(this.ItemsListView.SelectedItems);

        private void OpenTorrentUrlMenuItem_Click(object sender, RoutedEventArgs e) => 
            this.ViewModel.OpenTorrentUrl(this.ItemsListView.SelectedItems);

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

        private void GroupMarkReadMenuItem_Click(object sender, RoutedEventArgs e) => 
            this.ViewModel.MarkReaded(this.GroupsListView.SelectedItems);
    }
}
