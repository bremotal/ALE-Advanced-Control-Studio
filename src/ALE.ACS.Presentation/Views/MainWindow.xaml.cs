using System.Windows;
using System.Windows.Controls;
using ALE.ACS.Presentation.Models;
using ALE.ACS.Presentation.ViewModels;

namespace ALE.ACS.Presentation.Views;

/// <summary>
/// Janela principal da aplicação.
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    // Este método captura o item clicado na árvore e passa para o ViewModel
    private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (DataContext is MainWindowViewModel vm && e.NewValue is TreeNode node)
        {
            vm.SelectedNode = node;
        }
    }
}