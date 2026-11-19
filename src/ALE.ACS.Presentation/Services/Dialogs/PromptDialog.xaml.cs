using System.Windows;

namespace ALE.ACS.Presentation.Services.Dialogs;

/// <summary>
/// Diálogo de prompt simples: pede um valor de texto ao usuário.
/// Usado pelo RenameSelectedNode (REMED-001) para renomear nós do Explorer.
/// </summary>
public partial class PromptDialog : Window
{
    /// <summary>Valor digitado pelo usuário.</summary>
    public string Value { get; set; } = string.Empty;

    private PromptDialog(string title, string message, string defaultValue)
    {
        InitializeComponent();

        Title = string.IsNullOrWhiteSpace(title) ? "Entrada" : title;
        TitleLabel.Text = title;
        MessageLabel.Text = message;
        Value = defaultValue ?? string.Empty;

        DataContext = this;

        Loaded += (_, _) =>
        {
            ValueBox.Focus();
            ValueBox.SelectAll();
        };
    }

    /// <summary>
    /// Exibe o diálogo de forma síncrona e retorna o valor digitado,
    /// ou null se o usuário cancelou.
    /// </summary>
    public static string? Show(string title, string message, string defaultValue)
    {
        var dialog = new PromptDialog(title, message, defaultValue);
        dialog.ShowDialog();
        return dialog.DialogResult == true ? dialog.Value : null;
    }

    private void OnOk(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void OnCancel(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
