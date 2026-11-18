using System;
using System.Windows;
using System.Windows.Media;

namespace ALE.ACS.Presentation.Shell.Themes;

/// <summary>
/// Modos de tema suportados pelo ALE ACS.
/// </summary>
public enum AppThemeMode
{
    /// <summary>Segue o tema do sistema operacional (light/dark conforme preferência do Windows).</summary>
    Auto,

    /// <summary>Tema claro forçado.</summary>
    Light,

    /// <summary>Tema escuro forçado.</summary>
    Dark
}

/// <summary>
/// Gerencia a troca de tema da aplicação (Auto / Light / Dark).
/// </summary>
/// <remarks>
/// Em modo Auto, escuta a chave de registro do Windows
/// <c>HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize\AppsUseLightTheme</c>
/// via <see cref="SystemParameters"/> / evento de configuração do sistema para reagir à mudança
/// do tema do Windows em tempo de execução.
/// </remarks>
public static class ThemeManager
{
    private const string LightThemeUri =
        "pack://application:,,,/ALE.ACS.Presentation;component/Shell/Themes/LightTheme.xaml";
    private const string DarkThemeUri =
        "pack://application:,,,/ALE.ACS.Presentation;component/Shell/Themes/DarkTheme.xaml";

    private static AppThemeMode _currentMode = AppThemeMode.Auto;
    private static bool _systemIsDark;

    /// <summary>
    /// Evento disparado quando o tema efetivamente aplicado muda.
    /// </summary>
    public static event EventHandler<ThemeChangedEventArgs>? ThemeChanged;

    /// <summary>Modo configurado pelo usuário (Auto / Light / Dark).</summary>
    public static AppThemeMode CurrentMode
    {
        get => _currentMode;
        private set
        {
            if (_currentMode == value) return;
            _currentMode = value;
            PersistMode(value);
            ApplyEffectiveTheme();
        }
    }

    /// <summary>Indica se o tema efetivamente aplicado no momento é o dark.</summary>
    public static bool EffectiveIsDark =>
        _currentMode == AppThemeMode.Dark ||
        (_currentMode == AppThemeMode.Auto && _systemIsDark);

    /// <summary>
    /// Inicializa o ThemeManager: lê a preferência salva, detecta o tema do sistema,
    /// registra o listener de mudança do tema do Windows e aplica o tema inicial.
    /// Deve ser chamado uma única vez no startup da aplicação (em App.OnStartup).
    /// </summary>
    public static void Initialize()
    {
        _currentMode = LoadPersistedMode();
        _systemIsDark = DetectSystemDarkMode();

        // Hook de mudança do tema do sistema: SystemParameters não dispara diretamente
        // para AppsUseLightTheme, então usamos o evento UserPreferenceChanged.
        Microsoft.Win32.SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;

        ApplyEffectiveTheme();
    }

    /// <summary>
    /// Alterna para o próximo modo no ciclo Auto → Light → Dark → Auto.
    /// </summary>
    public static void Cycle()
    {
        CurrentMode = _currentMode switch
        {
            AppThemeMode.Auto => AppThemeMode.Light,
            AppThemeMode.Light => AppThemeMode.Dark,
            _ => AppThemeMode.Auto
        };
    }

    /// <summary>
    /// Aplica um modo explícito.
    /// </summary>
    public static void SetMode(AppThemeMode mode) => CurrentMode = mode;

    /// <summary>
    /// Força a reaplicação do tema atual (útil após mudança externa do sistema).
    /// </summary>
    public static void Refresh() => ApplyEffectiveTheme();

    /// <summary>
    /// Remove o listener de SystemEvents.UserPreferenceChanged.
    /// Deve ser chamado no OnExit da App para evitar leak após shutdown.
    /// </summary>
    public static void Shutdown()
    {
        Microsoft.Win32.SystemEvents.UserPreferenceChanged -= OnUserPreferenceChanged;
    }

    // ===== Implementation =====

    private static void ApplyEffectiveTheme()
    {
        var wantDark = EffectiveIsDark;
        // Qualificado totalmente: dentro do namespace ALE.ACS.Presentation.Shell.Themes,
        // "Application" resolveria para o namespace ALE.ACS.Application (não System.Windows.Application).
        var app = System.Windows.Application.Current;
        if (app is null) return;

        var themeUri = wantDark ? DarkThemeUri : LightThemeUri;

        // Recupera (ou cria) o dicionário dinâmico que atua como slot do tema ativo.
        ResourceDictionary? themeDict = null;
        foreach (var d in app.Resources.MergedDictionaries)
        {
            if (d.Source != null &&
                d.Source.OriginalString.Contains("Shell/Themes/", StringComparison.Ordinal))
            {
                themeDict = d;
                break;
            }
        }

        if (themeDict is null)
        {
            themeDict = new ResourceDictionary { Source = new Uri(themeUri, UriKind.Absolute) };
            app.Resources.MergedDictionaries.Add(themeDict);
        }
        else if (themeDict.Source?.OriginalString != themeUri)
        {
            themeDict.Source = new Uri(themeUri, UriKind.Absolute);
        }

        ThemeChanged?.Invoke(null, new ThemeChangedEventArgs(_currentMode, wantDark));
    }

    private static void OnUserPreferenceChanged(object? sender, Microsoft.Win32.UserPreferenceChangedEventArgs e)
    {
        if (e.Category != Microsoft.Win32.UserPreferenceCategory.General) return;

        var newIsDark = DetectSystemDarkMode();
        if (newIsDark == _systemIsDark) return;

        _systemIsDark = newIsDark;

        // SystemEvents pode disparar em thread fora do UI thread.
        // Qualificado totalmente (mesma razão do ApplyEffectiveTheme).
        if (System.Windows.Application.Current?.Dispatcher is null) return;
        System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
        {
            if (_currentMode == AppThemeMode.Auto)
            {
                ApplyEffectiveTheme();
            }
        }));
    }

    private static bool DetectSystemDarkMode()
    {
        try
        {
            using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            if (key?.GetValue("AppsUseLightTheme") is int value)
            {
                // AppsUseLightTheme = 1 (light) / 0 (dark)
                return value == 0;
            }
        }
        catch
        {
            // Registry pode não estar acessível em sandboxed environments — ignore.
        }
        return false;
    }

    private static AppThemeMode LoadPersistedMode()
    {
        try
        {
            var path = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "ALE.ACS", "theme.txt");
            if (!System.IO.File.Exists(path)) return AppThemeMode.Auto;
            var raw = System.IO.File.ReadAllText(path).Trim();
            return Enum.TryParse<AppThemeMode>(raw, ignoreCase: true, out var parsed)
                ? parsed
                : AppThemeMode.Auto;
        }
        catch
        {
            return AppThemeMode.Auto;
        }
    }

    private static void PersistMode(AppThemeMode mode)
    {
        // Persistência simples via IsolatedStorage não está disponível sem setup;
        // em WPF desktop usamos %AppData%\ALE.ACS\theme.txt
        try
        {
            var dir = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "ALE.ACS");
            System.IO.Directory.CreateDirectory(dir);
            var path = System.IO.Path.Combine(dir, "theme.txt");
            System.IO.File.WriteAllText(path, mode.ToString());
        }
        catch
        {
            // Persistência é best-effort; tema volta ao Auto se falhar.
        }
    }
}

/// <summary>
/// Argumentos do evento <see cref="ThemeManager.ThemeChanged"/>.
/// </summary>
public sealed class ThemeChangedEventArgs : EventArgs
{
    public AppThemeMode Mode { get; }
    public bool EffectiveIsDark { get; }

    public ThemeChangedEventArgs(AppThemeMode mode, bool effectiveIsDark)
    {
        Mode = mode;
        EffectiveIsDark = effectiveIsDark;
    }
}
