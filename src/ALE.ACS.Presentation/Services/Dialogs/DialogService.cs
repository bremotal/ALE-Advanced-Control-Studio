using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace ALE.ACS.Presentation.Services.Dialogs;

/// <summary>
/// Implementação do serviço de diálogos.
/// </summary>
public sealed class DialogService : IDialogService
{
    private readonly IServiceProvider _serviceProvider;

    public DialogService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public TDialog ShowDialog<TDialog>()
        where TDialog : Window
    {
        var dialog = _serviceProvider.GetRequiredService<TDialog>();

        dialog.ShowDialog();

        return dialog;
    }
}