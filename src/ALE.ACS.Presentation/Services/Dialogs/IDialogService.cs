using System.Windows;

namespace ALE.ACS.Presentation.Services.Dialogs;

/// <summary>
/// Serviço responsável por exibir diálogos da aplicação.
/// </summary>
public interface IDialogService
{
    TDialog ShowDialog<TDialog>()
        where TDialog : Window;
}