# ALE ACS — Implementação Completa (Blocos A–E)

**Data:** 2026-11-17
**Patch:** [ale-acs-full-implementation.patch](./ale-acs-full-implementation.patch) (6318 linhas, +5384/−245)
**Base:** commit `23d9308` (último do GitHub — já inclui Fase 1 Mamdani fix + Fase 2 shell dark + Fase 3.1 Fuzzy editor dark)

---

## TL;DR — caminho recomendado (working tree com arquivos pré-existentes)

O `git apply` falha com "already exists" quando seu working tree já tem arquivos das fases anteriores. Use o **ZIP completo** que sobrescreve tudo, depois remova manualmente os órfãos:

```powershell
cd D:\Projetos\ALE-Advanced-Control-Studio

# 1. Baixa o ZIP do src/ completo (estado final limpo)
#    https://[seu-host]/download/ale-acs-src-final.zip

# 2. Extrai por cima (sobrescreve arquivos modificados + adiciona novos)
Expand-Archive -Path D:\Projetos\ale-acs-src-final.zip -DestinationPath . -Force

# 3. Remove os 8 órfãos (o ZIP não deleta arquivos que não estão nele)
git rm -r --cached src/ALE.ACS.Application/Controllers/ControllerFactory.cs
Remove-Item -Force src/ALE.ACS.Application/Controllers/ControllerFactory.cs
git rm -r --cached src/ALE.ACS.Application/Projects/ProjectManager.cs
Remove-Item -Force src/ALE.ACS.Application/Projects/ProjectManager.cs
git rm -r --cached src/ALE.ACS.Domain/Models/ModelVariableType.cs
Remove-Item -Force src/ALE.ACS.Domain/Models/ModelVariableType.cs
git rm -r --cached src/ALE.ACS.Presentation/Views/Fuzzy
Remove-Item -Recurse -Force src/ALE.ACS.Presentation/Views/Fuzzy
git rm -r --cached src/ALE.ACS.Presentation/ViewModels/Fuzzy
Remove-Item -Recurse -Force src/ALE.ACS.Presentation/ViewModels/Fuzzy
git rm -r --cached src/ALE.ACS.Presentation/Models/Fuzzy
Remove-Item -Recurse -Force src/ALE.ACS.Presentation/Models/Fuzzy
git rm -r --cached src/ALE.ACS.Presentation/Views/SolutionExplorerView.xaml
Remove-Item -Force src/ALE.ACS.Presentation/Views/SolutionExplorerView.xaml
git rm -r --cached src/ALE.ACS.Presentation/Views/SolutionExplorerView.xaml.cs
Remove-Item -Force src/ALE.ACS.Presentation/Views/SolutionExplorerView.xaml.cs

# 4. Build
dotnet build ALE.AdvancedControlStudio.slnx
```

Se tudo verde:

```powershell
git add -A
git commit -m "feat: Blocos A-E — Domain+App aggregates, Infrastructure+Export, 6 dark editors, Engine/Core destravado, Digital Twin"
git push
```

### Alternativa: resetar working tree e usar o patch (mais limpo)

Se você não se importa em descartar mudanças locais não committed (faça backup antes), pode resetar o working tree pro HEAD do GitHub e aplicar o patch v2 (que inclui as 9 deleções):

```powershell
cd D:\Projetos\ALE-Advanced-Control-Studio
git reset --hard origin/main
git clean -fd src/   # remove arquivos não tracked (órfãos das fases anteriores)
git apply D:\Projetos\ale-acs-full-implementation.patch
dotnet build ALE.AdvancedControlStudio.slnx
```

> ⚠️ `git reset --hard` descarta QUALQUER mudança não committed. Só use se você tiver certeza de que não há trabalho local a preservar (ou após commitar/stash).

---

## O que foi implementado (5 blocos, 25 unidades)

### Bloco A — Domain + Application (fundação de dados)

| Unidade | Arquivo | Mudança |
|---|---|---|
| A1 | `Domain/Controllers/Controller.cs` (NOVO) | Aggregate root com Type/Lifecycle/Setpoint/Manipulated/Controlled/ScanTime/SerializedSpec/IsDesignValid + factory `Create(name, type)` |
| A2 | `Domain/Models/ModelVariable.cs` | Vira `EntityBase` com factory `Create` + `SetRange`/`SetUnit`; enum `ModelVariableType` consolidado (Controlled/Manipulated/Disturbance/Parameter) |
| A2 | `Domain/Models/ControlModel.cs` | Vira `AggregateRoot` com `_variables` collection + `AddVariable`/`RemoveVariable` + `SetDynamics` (τ, θ) + `MarkValidated` |
| A3 | `Domain/Projects/Project.cs` | Adiciona `Models` + `Controllers` children com operações Add/Remove/Find/OfType |
| A3 | `Domain/Common/Results/Result.cs` | Adiciona `Result` não-genérico para operações sem retorno |
| A4 | `Application/Interfaces/IControllerService.cs` (NOVO) | CRUD completo de controllers (Create/Rename/SetVariables/SetScanTime/SetSerializedSpec/MarkDesignValid/Remove) |
| A4 | `Application/Controllers/ControllerService.cs` (NOVO) | Implementação que opera sobre o projeto ativo |
| A5 | `Application/Interfaces/IModelService.cs` (NOVO) | CRUD de modelos + variables + dynamics |
| A5 | `Application/Models/ModelService.cs` (NOVO) | Implementação |
| A5 | `Application/Interfaces/IProjectService.cs` | Estendido: Open/Save/SaveAs/Close/GetRecentProjects/AddModel/AddController |
| A5 | `Application/Projects/ProjectService.cs` | Refatorado: injeta `IEventBus` + `IProjectRepository`; Open/Save delegam ao repo |
| A5 | `Application/Interfaces/IProjectRepository.cs` (NOVO) | Abstração de persistência (inversão de dependência) |
| A5 | `Application/Projects/ProjectManager.cs` | **REMOVIDO** (era duplicata do ProjectService) |
| A5 | `Application/Controllers/ControllerFactory.cs` | **REMOVIDO** (stub que confundia com o Engine/Factories/ControllerFactory real) |
| A5 | `Application/DependencyInjection/ApplicationServiceCollectionExtensions.cs` | Registra IControllerService + IModelService (+ EventBus do Core no Bloco E) |

### Bloco B — Infrastructure + Export (encanamento)

| Unidade | Arquivo | Mudança |
|---|---|---|
| B1 | `Infrastructure/FileSystem/IFileSystem.cs` (NOVO) | `IFileSystem` + `PhysicalFileSystem` (abstração real sobre System.IO) |
| B1 | `Infrastructure/Persistence/FilePersistenceProvider.cs` (NOVO) | Implementa `IPersistenceProvider` (Core/Projects) gravando snapshots como JSON em disco |
| B1 | `Infrastructure/Persistence/JsonProjectRepository.cs` (NOVO) | Implementa `IProjectRepository` (Application) — serializa `Domain.Project` com filhos via `System.Text.Json` source-gen |
| B2 | `Infrastructure/DependencyInjection/InfrastructureServiceCollectionExtensions.cs` | Registra IFileSystem + IPersistenceProvider + IProjectRepository; diretório base em `%AppData%\ALE.ACS\Projects` |
| B3 | `Export/Providers/JsonExportProvider.cs` (NOVO) | `IExportProvider` para formato JSON |
| B3 | `Export/Providers/PlcCodeExportProvider.cs` (NOVO) | `IExportProvider` que delega ao `PlcCodeGeneratorRegistry` do Engine |
| B3 | `Export/DependencyInjection/ExportServiceCollectionExtensions.cs` (NOVO) | Registra providers + `ExportManager` |
| B4 | `Engine/PlcCodeGen/StructuredTextCascadeGenerator.cs` (NOVO) | FUNCTION_BLOCK Cascade em ST: master+slave PID inline com anti-windup por back-calculation |
| B4 | `Engine/PlcCodeGen/StructuredTextSmithGenerator.cs` (NOVO) | FUNCTION_BLOCK Smith Predictor em ST: PID + preditor FOPDT + buffer circular de MV atrasada |
| B4 | `Engine/PlcCodeGen/StructuredTextLinearizationGenerator.cs` (NOVO) | FUNCTION_BLOCK Linearização em ST: lei u=(v-f(x))/g(x) com placeholders para f/g (delegates não serializáveis) |
| B3 | `Presentation/App.xaml.cs` | Chama `AddInfrastructure()` + `AddExport()` no host |
| B3 | `Presentation/ALE.ACS.Presentation.csproj` | Adiciona ProjectReference a Infrastructure + Export |
| B1 | `Infrastructure/ALE.ACS.Infrastructure.csproj` | Adiciona ProjectReference a Engine |
| B3 | `Export/ALE.ACS.Export.csproj` | Adiciona ProjectReference a Engine |

### Bloco C — Presentation dark

| Unidade | Arquivo | Mudança |
|---|---|---|
| C1 | `Presentation/Views/Fuzzy/` (3 arquivos) | **REMOVIDO** (órfão — cópia morta do `Controllers/Fuzzy/` vivo) |
| C1 | `Presentation/ViewModels/Fuzzy/` | **REMOVIDO** (órfão) |
| C1 | `Presentation/Models/Fuzzy/` | **REMOVIDO** (órfão) |
| C1 | `Presentation/Views/SolutionExplorerView.xaml` + `.cs` | **REMOVIDO** (legacy + sem referência) |
| C2 | `Presentation/Views/WorkspaceView.xaml` | Reescrito: dark (DynamicResource em tudo), layout simplificado (TabControl direto, sem painel lateral duplicado), 6 DataTemplates novos para PID/MPC/Adaptive/Identification/Neural/Optimization |
| C3 | `Presentation/Services/Navigation/NavigationService.cs` | Refatorado com **registry pattern**: `EditorFactories` dict (nome → factory) para os 6 novos tipos; mantém path Fuzzy especializado |
| C4 | `Presentation/Controllers/Common/Model/ControllerSection.cs` (NOVO) | Modelo genérico de seção (title/icon/hint/items) |
| C4 | `Presentation/Controllers/Common/ViewModels/ControllerEditorViewModelBase.cs` (NOVO) | Base com commands `AddItem`/`RemoveItem` (CanExecute vinculado a texto/seleção) |
| C4 | `Presentation/Controllers/Common/Resources/ControllerEditorResources.xaml` (NOVO) | DataTemplates reutilizáveis (`ControllerSectionItemTemplate`, `ControllerSectionCardTemplate`) |
| C5 | `Presentation/Controllers/PID/ViewModels/PidEditorViewModel.cs` | 4 seções: Variáveis do laço / Sintonia / Modos de operação / Limites de saída |
| C5 | `Presentation/Controllers/PID/Views/PidEditorView.xaml` | XAML dark com header + seletor de seção + card ativo + Add/Remove |
| C6 | `Presentation/Controllers/MPC/ViewModels/MpcEditorViewModel.cs` | 5 seções: Variáveis CV/MV/DV / Horizontes / Pesos / Restrições / Modelo dinâmico |
| C6 | `Presentation/Controllers/Adaptive/ViewModels/AdaptiveEditorViewModel.cs` | 5 seções: Variáveis / Identificação RLS / Detecção de mudança / Redesign QP / Limites |
| C6 | `Presentation/Controllers/Identification/ViewModels/IdentificationEditorViewModel.cs` | 5 seções: Método / Dados de entrada / Dados de saída / Modelo estimado / Validação |
| C6 | `Presentation/Controllers/Neural/ViewModels/NeuralEditorViewModel.cs` | 5 seções: Topologia / Camadas / Funções de ativação / Treino / Aplicação |
| C6 | `Presentation/Controllers/Optimization/ViewModels/OptimizationEditorViewModel.cs` | 5 seções: Variáveis de decisão / Função objetivo / Restrições / Solver / Resultado |
| C5-C7 | 5 XAMLs (Mpc/Adaptive/Identification/Neural/Optimization) | Mesmo template dark do PID (108 linhas cada), só muda o `x:Class` |

### Bloco D — Engine/Core destravado

| Unidade | Arquivo | Mudança |
|---|---|---|
| D3 | `Engine/Core/Execution/ControllerExecutionHost.cs` (NOVO) | Registro `blockId → IController` + `VariableStore` compartilhado (variáveis nomeadas double) |
| D3 | `Engine/Core/Execution/ExecutionPipeline.cs` | Novo método `ExecuteWithControllers(context, host)` — **invoca cada controller** em ordem topológica, lê inputs do host, grava outputs de volta, coleta diagnostics de `ControllerMessage`s. O `Execute` original é mantido para compat. |
| D1 | `Engine/Core/Simulation/SimulationSession.cs` (NOVO) | `SimulationClock` (step + endTime + Advance/Reset) + `SimulationState` enum + `ISimulationComponent` (Initialize/Step/Shutdown) + `SimulationStatistics` |
| D1 | `Engine/Core/Simulation/SimulationEngine.cs` | Refatorado: `Run(SimulationClock)` com **loop de tempo real** + lifecycle (Created/Running/Paused/Completed/Aborted) + `StepOnce` (manual) + `Pause` + `Reset`. Mantém `Run(SimulationContext)` legado. |
| D2 | `Engine/Core/Runtime/RuntimeSession.cs` (NOVO) | `RuntimeState` + `RuntimeConfiguration` (scanTime + maxScans + autoStart) + `IRuntimeComponent` + `RuntimeScheduler` + `RuntimeStatistics` + `RuntimeException` + `RuntimeSession` |
| D2 | `Engine/Core/Runtime/RuntimeEngine.cs` | `Execute(RuntimeSession)` com **scan loop wall-clock** (Thread.Sleep respeitando ScanTime) + `Stop` + `ExecuteOnce`. Mantém `Execute(RuntimeContext)` legado. |

### Bloco E — Integração CORE + Digital Twin

| Unidade | Arquivo | Mudança |
|---|---|---|
| E1/E2 | `Application/DependencyInjection/ApplicationServiceCollectionExtensions.cs` | Registra `EventBus` do `Core.Common.Events` como singleton (CORE-008 não-destrutivo: o `IEventBus` thread-safe da Application permanece como barramento primário) |
| E4 | `Application/DigitalTwin/IDigitalTwinService.cs` (NOVO) | Contrato `SimulatePid(pidSpec, processModel, setpoint, initialPv, scanCount)` retornando série temporal |
| E4 | `Application/DigitalTwin/DigitalTwinService.cs` (NOVO) | Implementação que constrói `SimulationEngine` + `PidControllerComponent` (ISimulationComponent que envolve `PidBlock`) + `FopdtProcessComponent` (propaga FOPDT com buffer circular de MV atrasada) |
| E4 | `Presentation/ViewModels/MainWindowViewModel.cs` | Injeta `IDigitalTwinService`; `RunSimulation` detecta `Kind="PID"` e roda Digital Twin com PidSpec padrão (Kp=2.0, Ti=5s, Td=0.5s) contra FopdtModel padrão (K=1.0, τ=10s, θ=2s), 100 scans — resultado vai para o Output panel |

---

## Estatísticas

| Métrica | Valor |
|---|---|
| Arquivos modificados | 30 |
| Arquivos novos | 23 |
| Arquivos removidos | 7 (órfãos + stubs) |
| Linhas adicionadas | +5384 |
| Linhas removidas | −245 |
| Patch total | 6318 linhas |
| Blocos | 5 (A/B/C/D/E) |
| Commits incrementais no sandbox | 5 (um por bloco) |

---

## Validação que fiz (sandbox Linux, sem `dotnet`)

| Verificação | Resultado |
|---|---|
| `git apply --check` em clone fresco do GitHub | ✅ Passou |
| Sintaxe C# balanceada (chaves/parênteses/colchetes = 0) em todos os arquivos | ✅ 33 arquivos verificados |
| XAML balanceado (opens − closes − self_close = 0) | ✅ 11 arquivos XAML verificados |
| 14 `DynamicResource` keys usadas nos editores existem no `DarkTheme.xaml` | ✅ Confirmado |
| 26 bindings do Fuzzy editor batem com o ViewModel | ✅ 0 sem match |
| 10 bindings de cada editor novo batem com `ControllerEditorViewModelBase` | ✅ Confirmado |
| Sem dependência circular entre projetos (Application→Engine/Domain; Infrastructure→Application/Engine; Export→Application/Engine; Presentation→todos) | ✅ Confirmado |
| `dotnet build` | ⏳ Você roda localmente |

---

## Como testar na UI

1. Abra o app (deve abrir em dark se Windows estiver em dark).
2. No Project Explorer, clique em **PID Controller** → deve aparecer o editor dark com 4 cards (Variáveis / Sintonia / Modos / Limites).
3. Selecione uma seção no seletor horizontal → o card ativo muda.
4. Digite no TextBox → botão `+ Add` habilita → clique → item aparece na lista.
5. Selecione um item → botão `− Remove` habilita → clique → item some.
6. Clique em **▶ Simulate** na titlebar → se o documento ativo for PID, o Digital Twin roda 100 scans e o resultado vai pro Output panel (log: "Digital Twin OK: 100 scans, 10.0s simulados. Amostras: 3.").
7. Teste os outros 5 editores (MPC/Adaptive/Identification/Neural/Optimization) — mesmo comportamento.
8. Clique em **🎨 Theme** → todos os editores reagem em tempo real.

---

## Arquitetura final

```
┌─────────────────────────────────────────────────────────────┐
│                    Presentation (WPF)                        │
│  Shell dark + 7 editores dark + WorkspaceView dark           │
│  NavigationService (registry pattern) ←→ IDigitalTwinService │
└─────────────────────────────────────────────────────────────┘
                              │
┌─────────────────────────────┴───────────────────────────────┐
│                    Application                              │
│  IProjectService / IControllerService / IModelService       │
│  IProjectRepository (abstração) ← IDigitalTwinService        │
│  EventBus (thread-safe, Action<T>)                          │
└─────────────────────────────────────────────────────────────┘
         │                                  │
┌────────┴──────────┐          ┌───────────┴────────────────┐
│   Infrastructure  │          │         Export              │
│  FilePersistence  │          │  JsonExportProvider          │
│  Provider (JSON)  │          │  PlcCodeExportProvider       │
│  JsonProjectRepo  │          │  ExportManager               │
│  IFileSystem      │          └──────────────────────────────┘
└───────────────────┘
         │
┌────────┴─────────────────────────────────────────────────────┐
│                       Engine                                │
│  Algorithms: Fuzzy/MPC/AdaptiveMpc/Cascade/Smith/            │
│              Linearization/Optimization/PID (todos reais)   │
│  Core: ExecutionPipeline.ExecuteWithControllers (REAL)      │
│        SimulationEngine (loop de tempo real + lifecycle)     │
│        RuntimeEngine (scan loop wall-clock)                 │
│        Core.Common.Events.EventBus (registrado no DI)       │
│  PlcCodeGen: PID/MPC/Fuzzy + Cascade/Smith/Linearization    │
│              (3 geradores novos — total 6/9 kinds)           │
└─────────────────────────────────────────────────────────────┘
         │
┌────────┴─────────────────────────────────────────────────────┐
│                       Domain                                │
│  Project (aggregate com children Models + Controllers)      │
│  ControlModel (aggregate com Variables + Dynamics)           │
│  Controller (aggregate com Type/Lifecycle/Spec/DesignValid)   │
│  ModelVariable (EntityBase com Range/Unit)                   │
└─────────────────────────────────────────────────────────────┘
```

---

## Próximos passos (não incluídos)

- **PlcCodeGen para AdaptiveMpc e Optimization** (2 kinds restantes — complexidade alta, sem caso de uso imediato)
- **AdaptiveMpc ARX ordem ≥4** (marcado "extensão futura" no código original)
- **MIMO MPC** no PlcCodeGen (hoje só avisa "apenas SISO")
- **Plugin framework via reflection** (CORE-005 — sem consumidor)
- **Serialization concrete serializers** no Core (já existem no Fuzzy layer)
- **Unificação completa do EventBus** (migrar todos os publishers/subscribers de `Application.IEventBus` para `Core.Common.Events.EventBus` — refactor destrutivo, alto risco)
- **Wire CORE frameworks em consumers reais** (hoje Validation/Diagnostics/Plugins estão registrados no DI mas sem callers — precisam de features que os usem)

---

## Aplicação + commit

```powershell
cd D:\Projetos\ALE-Advanced-Control-Studio
git apply D:\Projetos\ale-acs-full-implementation.patch
dotnet build ALE.AdvancedControlStudio.slnx
```

Se verde:

```powershell
git add -A
git commit -m "feat: Blocos A-E — full implementation

- Domain: Controller/ControlModel/ModelVariable aggregates + Project com children
- Application: IControllerService/IModelService/IProjectRepository + handlers
- Infrastructure: FilePersistenceProvider + JsonProjectRepository + IFileSystem
- Export: JsonExportProvider + PlcCodeExportProvider + ExportManager
- Engine: 3 ST generators (Cascade/Smith/Linearization)
- Engine: ExecutionPipeline.ExecuteWithControllers (invoca controllers reais)
- Engine: SimulationEngine (loop de tempo real + lifecycle)
- Engine: RuntimeEngine (scan loop wall-clock)
- Presentation: 6 dark editors (PID/MPC/Adaptive/Identification/Neural/Optimization)
- Presentation: WorkspaceView dark + NavigationService registry pattern
- Presentation: limpeza de órfãos (Views/Fuzzy, ViewModels/Fuzzy, SolutionExplorerView)
- Digital Twin: IDigitalTwinService + PidControllerComponent + FopdtProcessComponent
- MainWindowViewModel.RunSimulation detecta PID e roda Digital Twin (100 scans)
- CORE-008: EventBus do Core.Common.Events registrado no DI (não-destrutivo)"
git push
```

Se o build reclamar de algo, me cola o erro. Casos comuns:

| Erro | Causa | Fix |
|---|---|---|
| `CS0246: 'IDigitalTwinService' não encontrado` | Presentation não referencia Application | Já está no csproj — verifique `dotnet restore` |
| `CS0234: 'Application' é ambíguo` | Conflito de namespace | Já tratado no commit `cd2239e` (qualify `System.Windows.Application`) |
| `XDG0007: Resource 'X' not found` | Key faltando no tema | Todas as 14 keys usadas existem no DarkTheme.xaml |
| `MC3002: DynamicResource` | Style referenciado antes da definição | App.xaml tem MergedDictionaries antes do MainWindow |

---

## Documentação auxiliar

- [ROADMAP-COMPLETO.md](./ROADMAP-COMPLETO.md) — inventário real + plano original
- [APLICACAO-FASE3-FUZZY-EDITOR.md](./APLICACAO-FASE3-FUZZY-EDITOR.md) — Fase 3.1 (Fuzzy dark editor, já merged)
- [APLICACAO-FASE2-UI.md](./APLICACAO-FASE2-UI.md) — Fase 2 (shell dark, já merged)
- [FIX-CS0234.md](./FIX-CS0234.md) — Fix CS0234 (já merged)
- [ale-acs-full-implementation.patch](./ale-acs-full-implementation.patch) — **este patch**
