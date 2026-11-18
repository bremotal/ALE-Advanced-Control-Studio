# Fase 3.1 — Fuzzy Editor dark funcional

## TL;DR — 2 comandos

```powershell
cd D:\Projetos\ALE-Advanced-Control-Studio
git apply D:\Projetos\fase3-fuzzy-editor.patch
dotnet build src\ALE.ACS.Presentation\ALE.ACS.Presentation.csproj
```

Se tudo verde:

```powershell
git add -A
git commit -m "feat(fuzzy): Fase 3.1 — dark editor with 4 cards + Add/Remove commands"
git push
```

## O que está incluído (3 arquivos, +582/-38 linhas)

| Arquivo | Ação | Mudança |
|---|---|---|
| `Controllers/Fuzzy/ViewModels/FuzzyEditorViewModel.cs` | ESTENDIDO | +9 ObservableProperty (4 `_new*` + 4 `_selected*` + `_controller`) + 8 RelayCommand (Add/Remove para cada coleção) com `CanExecute` + 8 `partial On*Changed` para reavaliar `CanExecute` em tempo real |
| `Controllers/Fuzzy/Views/FuzzyEditorView.xaml` | REESCRITO | Layout dark com ScrollViewer → header (nome + descrição + 3 badges de contagem) → grid 2×2 de cards (Inputs / Outputs / Membership Functions / Rule Base). Cada card: header com ícone + ListBox + linha Add/Remove (TextBox + PrimaryButton + MiniButton) + hint |
| `Controllers/Fuzzy/Resources/FuzzyResources.xaml` | PREENCHIDO | 2 DataTemplates: `FuzzyItemTemplate` (bullet + texto) para Inputs/Outputs/MFs e `FuzzyRuleTemplate` (mono font + wrap) para regras |

## Funcionalidades novas

### 1. Layout dark coerente com a shell
- Tudo via `DynamicResource` (14 keys: `BackgroundBrush`, `PanelBackgroundBrush`, `PanelAltBackgroundBrush`, `TextForegroundBrush`, `MutedForegroundBrush`, `AccentBrush`, `GoodBrush`, `WarnBrush`, `BadBrush`, `BorderBrush`, `CardBorder`, `PrimaryButton`, `MiniButton`, `ThemedTextBox`).
- Reage ao tema ativo (Auto/Light/Dark) em tempo real.

### 2. 4 cards de edição
Cada um com:
- **Header** com ícone colorido (▶ Inputs / ◀ Outputs / ∿ MFs / ⇒ Rules), título e contagem `(N)` ao lado.
- **ListBox** com `SelectedItem` bidirecional ligado à propriedade `Selected*` do VM.
- **Linha Add/Remove**: TextBox (`UpdateSourceTrigger=PropertyChanged`) + botão `+ Add` (`PrimaryButton`, habilitado só se texto não vazio) + botão `− Remove` (`MiniButton`, habilitado só se item selecionado).
- **Hint** abaixo explicando o propósito da seção.

### 3. Header do editor com badges de contagem
- Nome (`Controller.Name`) + descrição (`Controller.Description`)
- 3 pills de contagem: **Inputs** (accent), **Outputs** (good/green), **Rules** (warn/amber) — atualizam ao adicionar/remover.

### 4. Comportamento CanExecute em tempo real
- Botão `+ Add` fica desabilitado quando o TextBox está vazio.
- Botão `− Remove` fica desabilitado quando nenhum item está selecionado.
- Ao adicionar, o TextBox é limpo automaticamente.
- Ao remover, o `Selected*` é limpo (volta a `null`).

## Bindings preservados (NENHUMA quebra)

O `Controller` (já existente) é mantido, então todos os bindings legados continuam funcionando. Apenas **adicionamos** propriedades/commands novos:

- `Controller.Name`, `Controller.Description`, `Controller.Inputs`, `Controller.Outputs`, `Controller.MembershipFunctions`, `Controller.RuleBase` (existentes)
- `Controller.Inputs.Count`, `Controller.Outputs.Count`, `Controller.MembershipFunctions.Count`, `Controller.RuleBase.Count` (existentes)
- `NewInput`, `NewOutput`, `NewMembershipFunction`, `NewRule` (NOVOS)
- `SelectedInput`, `SelectedOutput`, `SelectedMembershipFunction`, `SelectedRule` (NOVOS)
- `AddInputCommand`, `RemoveInputCommand`, `AddOutputCommand`, `RemoveOutputCommand`, `AddMembershipFunctionCommand`, `RemoveMembershipFunctionCommand`, `AddRuleCommand`, `RemoveRuleCommand` (NOVOS)

## Validação que fiz (sandbox Linux, sem `dotnet`)

| Verificação | Resultado |
|---|---|
| Patch aplica limpo (`git apply --check`) em clone fresco do GitHub | ✅ Passou |
| XAML balanceado (opens − closes − self_close = 0) | ✅ 0 nos 2 arquivos XAML |
| Todas as 14 `DynamicResource` keys existem no `DarkTheme.xaml` | ✅ Confirmado |
| `StaticResource` `FuzzyItemTemplate` + `FuzzyRuleTemplate` definidos em `FuzzyResources.xaml` | ✅ Confirmado |
| `MonoFontFamily` existe no tema | ✅ Confirmado |
| 26 bindings no XAML, todos com match no VM | ✅ 0 sem match |
| 9 `[ObservableProperty]` + 8 `[RelayCommand]` (cada um com `CanExecute`) | ✅ Confirmado |
| 8 `partial On*Changed` chamam `NotifyCanExecuteChanged` | ✅ Confirmado |
| 8 `private bool Can*()` (Add/Remove para cada coleção) | ✅ Confirmado |
| C# sintaxe balanceada (chaves/parênteses/colchetes = 0) | ✅ Confirmado |
| ⚠️ `dotnet build` não rodou (sandbox sem SDK) | ⏳ Você roda localmente |

## Como testar na UI

1. Abra o app (deve abrir em dark se seu Windows estiver em dark).
2. No Project Explorer (sidebar esquerda), clique em **Fuzzy Controller**.
3. O content central deve mostrar o editor dark com:
   - Header com nome "Fuzzy Controller" + descrição + 3 badges (Inputs: 2, Outputs: 1, Rules: 1)
   - 4 cards: Inputs, Outputs, Membership Functions, Rule Base
4. Teste interações:
   - Digite algo no TextBox "Add" de Inputs → botão `+ Add` habilita → clique → item aparece na lista, badge atualiza, TextBox limpa
   - Selecione um item na lista → botão `− Remove` habilita → clique → item some, badge atualiza
   - Mesmo fluxo para os 4 cards
5. Clique no 🎨 Theme na titlebar → editor reage em tempo real (Light/Dark/Auto)

## Observação sobre o `WorkspaceView.xaml`

O `WorkspaceView.xaml` (que hospeda o editor) ainda tem o layout antigo com cores hardcoded (`#F7F7F7`, `#D0D0D0`, `White`) e um painel lateral de propriedades. Isso significa que o editor dark aparece **dentro de um quadro claro**. 

Isso é intencional para esta etapa (escopo contido, baixo risco). A **Etapa 3.2** vai refazer o `WorkspaceView.xaml` para herdar tema dark e limpar o layout (remover o painel lateral de propriedades duplicado, já que o PropertiesPane da sidebar já mostra isso).

## Estado dos entregáveis

| Item | Status |
|---|---|
| Fase 1 (Mamdani fix) | ✅ pushed |
| Fase 2 (Shell dark) | ✅ pushed (commit `cd2239e`) |
| Fix CS0234 | ✅ pushed |
| Patch Fase 3.1 (este) | ✅ `download/fase3-fuzzy-editor.patch` (666 linhas) |
| Aplicação + build | ⏳ Pendente de você aplicar localmente |

## Próximos passos (após você confirmar verde)

- **Etapa 3.2**: Refazer `WorkspaceView.xaml` para dark (remover cores hardcoded, painel lateral duplicado).
- **Etapa 3.3**: Aplicar o mesmo padrão de editor dark aos outros 6 controllers (MPC, PID, Neural, Identification, Optimization, Adaptive) — cada um com suas seções específicas.
- **Etapa 3.4 (opcional)**: Conectar o editor ao `ALE.ACS.Engine.Fuzzy` real (FuzzyController, MembershipFunctions, Rules, Inference, Defuzzification) para que Add/Remove reflitam num controlador executável, não só no modelo de UI.
