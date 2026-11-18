# FIX CS0234 — Patch incremental para corrigir erro de compilação

## Sintoma

Ao rodar `dotnet build` após aplicar o `fase2-ui.patch`:

```
ThemeManager.cs(118,19): error CS0234: O nome de tipo ou namespace "Current" não existe no namespace "ALE.ACS.Application"
ThemeManager.cs(158,13): error CS0234: ...
ThemeManager.cs(159,9):  error CS0234: ...
```

## Causa

Dentro do namespace `ALE.ACS.Presentation.Shell.Themes`, quando eu escrevia `Application.Current`, o compilador C# resolvia o nome `Application` para o **sub-namespace** `ALE.ACS.Application` (que está "pai-irmão" do namespace atual na árvore `ALE.ACS.*`) em vez de `System.Windows.Application`. Como o namespace `ALE.ACS.Application` não tem um membro `Current`, ocorre CS0234.

## Solução

Qualificar totalmente como `System.Windows.Application.Current` nas 3 ocorrências:
- `ApplyEffectiveTheme()` → `var app = System.Windows.Application.Current;`
- `OnUserPreferenceChanged` → `if (System.Windows.Application.Current?.Dispatcher is null) return;`
- `OnUserPreferenceChanged` → `System.Windows.Application.Current.Dispatcher.BeginInvoke(...)`

## Como aplicar (você já tem o fase2-ui.patch aplicado)

```powershell
cd D:\Projetos\ALE-Advanced-Control-Studio
git apply D:\Projetos\fase2-fix-application.patch
dotnet build src\ALE.ACS.Presentation\ALE.ACS.Presentation.csproj
```

Se verde:

```powershell
git add -A
git commit -m "fix(ui): qualify Application.Current as System.Windows.Application (CS0234)"
git push
```

## Validação que fiz

- ✅ Patch incremental aplica limpo (`git apply --check` passou) por cima do `fase2-ui.patch` em clone fresco
- ✅ Diff é de **modificação** (27 linhas, sem `new file mode`)
- ✅ Altera exatamente as 3 linhas problemáticas + 2 linhas de comentário explicativo
- ✅ Nenhum outro arquivo tocado

## Estrutura do patch

```
diff --git a/src/ALE.ACS.Presentation/Shell/Themes/ThemeManager.cs b/src/ALE.ACS.Presentation/Shell/Themes/ThemeManager.cs
@@ -115,7 +115,9 @@  ApplyEffectiveTheme()
-        var app = Application.Current;
+        // Qualificado totalmente: dentro do namespace ALE.ACS.Presentation.Shell.Themes,
+        // "Application" resolveria para o namespace ALE.ACS.Application (não System.Windows.Application).
+        var app = System.Windows.Application.Current;

@@ -155,8 +157,9 @@  OnUserPreferenceChanged
-        if (Application.Current?.Dispatcher is null) return;
-        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
+        // Qualificado totalmente (mesma razão do ApplyEffectiveTheme).
+        if (System.Windows.Application.Current?.Dispatcher is null) return;
+        System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
```

## Por que `App.xaml.cs` não teve o mesmo erro

O `App.xaml.cs` declara `public partial class App : System.Windows.Application` — o tipo `Application` já aparece qualificado com `System.Windows.`, então qualquer referência posterior a `Application` naquele arquivo resolve para `System.Windows.Application` (resolução lexical do C# favorece o tipo herdado). Já o `ThemeManager.cs` não herda de nada relacionado a `Application`, então o nome não-qualificado vira ambíguo e o compilador escolhe o namespace `ALE.ACS.Application`.
