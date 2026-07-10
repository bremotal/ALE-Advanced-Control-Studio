using System.Windows;
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
}