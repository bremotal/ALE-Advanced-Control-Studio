# Fase 2 — Redesenho da UI (Dark Theme + Titlebar Custom + Sidebar Unificada + Tabs + Output)

## TL;DR — 3 comandos

```powershell
cd D:\Projetos\ALE-Advanced-Control-Studio
git apply D:\Projetos\fase2-ui.patch
dotnet build src\ALE.ACS.Presentation\ALE.ACS.Presentation.csproj
dotnet run --project src\ALE.ACS.Presentation\ALE.ACS.Presentation.csproj
```

Se tudo verde:

```powershell
git add -A
git commit -m "feat(ui): Fase 2 — Dark theme (Auto/Light/Dark) + titlebar custom + sidebar unificada + tabs no content + output panel"
git push
```

## O que está incluído

### Etapa 2.1 — Theme infrastructure (4 arquivos novos + 2 editados)

| Arquivo | Ação | Função |
|---|---|---|
| `Shell/Themes/DarkTheme.xaml` | NOVO | ResourceDictionary com 39 keys (cores, brushes, estilos de Button/TextBox/Menu/TreeView/Tab/StatusBar/etc) para tema escuro |
| `Shell/Themes/LightTheme.xaml` | NOVO | Mesma estrutura, cores claras |
| `Shell/Themes/ThemeManager.cs` | NOVO | Classe estática com Initialize/Cycle/SetMode/Refresh/Shutdown. Detecta tema do Windows via registry, escuta `SystemEvents.UserPreferenceChanged` para reagir em tempo real, persiste preferência em `%AppData%\ALE.ACS\theme.txt` |
| `App.xaml` | EDITADO | Adiciona slot dinâmico no `MergedDictionaries` apontando para `LightTheme.xaml` (será trocado pelo ThemeManager em runtime) |
| `App.xaml.cs` | EDITADO | Chama `ThemeManager.Initialize()` no `OnStartup` e `ThemeManager.Shutdown()` no `OnExit` |

### Etapa 2.2 — Layout novo (3 arquivos editados)

| Arquivo | Ação | Mudança |
|---|---|---|
| `Views/MainWindow.xaml` | REESCRITO | Titlebar custom (brand + breadcrumb + ícones Run/Validate + pill Theme + pill Output) + Menu mantido + Sidebar unificada (Explorer em cima + Properties embaixo com GridSplitter horizontal) + Content area com Toolbar + Tabs (Designer + Output opcional) + Output panel inferior redimensionável |
| `ViewModels/MainWindowViewModel.cs` | EDITADO | + `OutputModeIsTab` property + `CycleThemeCommand` + `ToggleOutputModeCommand` |
| `Shell/Panes/ProjectExplorerPane.xaml` | EDITADO | Removido `Background="White"` / `Foreground="Black"` / `BorderBrush="#C0C0C0"` hardcoded → agora usa `DynamicResource` para herdar o tema ativo |

## Funcionalidades novas

### 1. Tema Auto / Light / Dark
- **Auto** (padrão): segue o tema do Windows. Se você mudar o tema do Windows para dark/light, o app reage **em tempo real** (sem reiniciar).
- **Light/Dark**: forçado pelo usuário.
- Botão **🎨 Theme** na titlebar clica no ciclo `Auto → Light → Dark → Auto`.
- Preferência persistida em `%AppData%\ALE.ACS\theme.txt` (sobrevive entre sessões).

### 2. Output: Bottom ↔ Tab
- **Bottom** (padrão): painel inferior com `GridSplitter` horizontal (redimensionável, altura inicial 180px, min 100, max 500).
- **Tab**: vira 2ª aba no content (Designer | Output).
- Botão **📊 Output** na titlebar alterna entre os dois modos.
- A aba "Output" só aparece no `TabControl` quando o modo Tab está ativo (via `Visibility` binding).

### 3. Titlebar custom
- Brand (logo "A" + "ALE ACS") à esquerda
- Breadcrumb no meio: `ProjectStatus / DocumentStatus`
- Ícones de ação: ▶ Run (Simulate) e ✓ Validate
- Pills: 🎨 Theme e 📊 Output
- Altura fixa de 36px

### 4. Sidebar unificada
- Explorer pane (TreeView) na parte superior
- Properties pane (Name/Type/Description/Apply + PropertiesContent) na parte inferior
- `GridSplitter` horizontal entre eles (altura inicial 200px para Properties)
- Coluna inteira tem largura inicial 260px, min 200, max 400, com `GridSplitter` vertical para o content

### 5. Content area
- Toolbar no topo: título (`DocumentStatus`) + subtítulo (`StatusText`) + botões Validate / ▶ Simulate
- Abas: Designer (sempre) + Output (quando modo Tab ativo)
- Output panel inferior (quando modo Bottom ativo) com header próprio e TextBox monospace

## Bindings preservados (NENHUMA quebra)

Todos os bindings do `MainWindow.xaml` original foram mantidos:
- `WindowTitle`, `ProjectStatus`, `DocumentStatus`, `StateStatus`, `StatusText`
- `SelectedName`, `SelectedType`, `SelectedDescription`, `PropertiesContent`
- `OutputText`, `RecentProjects`, `WorkspaceView`
- `IsExplorerVisible`, `IsWorkspaceVisible`, `IsPropertiesVisible`, `IsOutputVisible`, `IsStatusBarVisible`
- Todos os commands: `NewProject/OpenProject/SaveProject/SaveProjectAs/CloseProject/OpenRecentProject`
- `NewModel/NewFuzzyController/NewMpcController/NewAdaptiveController/NewNeuralController/NewIdentificationController/NewOptimizationController/NewPidController`
- `RunValidation/RunSimulation/OpenOptions`
- `ToggleExplorer/ToggleWorkspace/ToggleProperties/ToggleOutput/ToggleStatusBar`
- `ShowAbout/OpenDocumentation/OpenReleaseNotes/ExitApplication`
- `ApplyProperties` + Undo/Redo (não mexidos)

## Bindings NOVOS

- `CycleThemeCommand` → botão 🎨 Theme na titlebar
- `ToggleOutputModeCommand` → botão 📊 Output na titlebar
- `OutputModeIsTab` (bool) → visibilidade da aba "Output" no TabControl

## Validação que fiz

- ✅ Patch aplica limpo (`git apply --check` passou num clone fresco do GitHub)
- ✅ Estrutura XAML balanceada (153 opens = 56 closes + 97 self-closing, net = 0)
- ✅ Todos os 18 resource keys usados no MainWindow estão definidos em DarkTheme.xaml (8 DynamicResource + 10 StaticResource)
- ✅ Code-behind `MainWindow.xaml.cs` não precisou de mudança (compatível)
- ✅ `ProjectExplorerPane.xaml` foi ajustado para herdar tema via `DynamicResource`
- ⚠️ **Não compilei** (sandbox Linux não tem `dotnet` SDK) — rode `dotnet build` para confirmar 100%

## Após aplicar

```powershell
dotnet build src\ALE.ACS.Presentation\ALE.ACS.Presentation.csproj
dotnet run --project src\ALE.ACS.Presentation\ALE.ACS.Presentation.csproj
```

Você deve ver:
1. App abre em **dark theme** (se seu Windows estiver em dark) ou **light** (se estiver em light)
2. **Titlebar custom** no topo (brand + breadcrumb + ícones + pills)
3. **Sidebar esquerda** com Explorer em cima e Properties embaixo
4. **Content central** com toolbar + tabs (Designer)
5. **Output panel inferior** com header e texto monospace
6. Clique no **🎨 Theme** → tema troca (Auto → Light → Dark)
7. Clique no **📊 Output** → output muda de painel inferior para aba

## Possíveis ajustes pós-aplicação

Se o `dotnet build` reclamar de algo (improvável given a análise), me cola o erro. Casos comuns:

| Erro | Causa | Fix |
|---|---|---|
| `MC3002: A 'DynamicResource' was set on 'X'` | Style referenciado antes da definição | Garantir que `App.xaml` tem o `MergedDictionaries` antes do `MainWindow` carregar |
| `CS0103: 'ThemeManager' does not exist` | Namespace não importado | Já importado em `App.xaml.cs` (`using ALE.ACS.Presentation.Shell.Themes;`) |
| `XDG0007: Resource 'X' not found` | Key faltando no tema | Verificar se a key existe nos dois themes (DarkTheme.xaml + LightTheme.xaml) |

## Estado dos entregáveis

| Item | Status |
|---|---|
| Fase 1 (Mamdani fix) | ✅ pushed no GitHub |
| Preview HTML Fase 2 | ✅ `download/preview-ui-fase2.html` |
| Patch Fase 2 | ✅ `download/fase2-ui.patch` (1707 linhas) |
| Aplicação + build + testes | ⏳ Pendente de você aplicar localmente |

## Próximos passos (após você confirmar verde)

- **Etapa 2.3 (opcional)**: Cards grid no content (4 cards: Model / Constraints / Horizons&Weights / Performance) como no preview HTML — atualmente o content usa `WorkspaceView` existente. Posso refatorar para cards.
- **Fase 3**: converter mockup HTML dos controllers individuais (FuzzyEditorView, MpcEditorView, etc) para o novo tema dark.
