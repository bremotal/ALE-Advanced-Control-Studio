namespace ALE.ACS.Presentation.Events;

/// <summary>
/// Representa uma solicitação de fechamento de um diálogo.
/// </summary>
public sealed class DialogCloseRequestedEventArgs : EventArgs
{
    public bool DialogResult { get; }

    public DialogCloseRequestedEventArgs(bool dialogResult)
    {
        DialogResult = dialogResult;
    }
}