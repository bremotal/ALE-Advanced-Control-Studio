# ROADMAP COMPLETO — ALE Advanced Control Studio

**Data:** 2026-11-18 (atualizado pós-push commit `f45c68b`)
**Base:** auditoria real do código no GitHub (HEAD = `f45c68b`, includes Blocos A-E + Fase 2/3 UI + 6 fixes de compilação).

---

## 0. Estado atual (pós-push `f45c68b`)

| Métrica | Valor |
|---|---|
| Commit no `main` | `f45c68b` |
| Arquivos alterados no último commit | 53 |
| Linhas adicionadas | +5377 |
| Linhas removidas | −249 |
| Build | **9/9 projetos verdes em 18.2s** |
| Repositório | https://github.com/bremotal/ALE-Advanced-Control-Studio |

### Projetos compilando

| Projeto | Plataforma | Status |
|---|---|---|
| ALE.ACS.Domain | net10.0 | ✅ |
| ALE.ACS.Shared | net10.0 | ✅ |
| ALE.ACS.Engine | net10.0 | ✅ |
| ALE.ACS.Application | net10.0 | ✅ |
| ALE.ACS.Infrastructure | net10.0 | ✅ |
| ALE.ACS.Export | net10.0 | ✅ |
| ALE.ACS.Presentation | net10.0-windows | ✅ |
| ALE.ACS.Engine.Tests | net10.0 | ✅ |
| ALE.ACS.Presentation.Tests | net10.0-windows | ✅ |

### Fixes de compilação aplicados nesta sessão

| # | Erro | Arquivo | Fix |
|---|---|---|---|
| 1 | CS0101 `ModelVariableType` duplicado | `Domain/Models/ModelVariable.cs` | Removida definição inline |
| 2 | CS0117 `DiagnosticSeverity.Info` | `Engine/Core/Execution/ExecutionPipeline.cs:186` | `.Info` → `.Information` |
| 3 | Diretório fantasma `src/src/` (329 arquivos) | Estrutural | `Remove-Item -Recurse -Force` |
| 4 | CS0118 `EventBus` namespace-vs-classe | `Application/DI/ApplicationServiceCollectionExtensions.cs:37` | Fully-qualify `ALE.ACS.Engine.Core.Common.Events.EventBus` |
| 5 | CS0266 `IReadOnlyDictionary` → `Dictionary` | `Infrastructure/Persistence/FilePersistenceProvider.cs:45` | `new Dictionary<string, string>(snapshot.Metadata)` |
| 6 | CS1729 + 2× CS0200 `ControlModel` imutável | `Presentation/ViewModels/MainWindowViewModel.cs:331` | `ControlModel.Create(name, description)` + desembrulhar `Result<T>` |

---

## 1. Inventário real do código (estado atualizado)

### 1.1 Engine — produção-grade ✅

| Módulo | Status | Notas |
|---|---|---|
| **Fuzzy** | ✅ COMPLETO | 66 arquivos. Mamdani/Sugeno, 12 MF types, centroid/MoM defuzz, parser→AST→compiler, JSON/XML/binary serializers, SIMD, plugins, consistency checker. |
| **MPC** | ✅ COMPLETO | QP solve per scan (gradient-projection), prediction matrix S, gain K offline, FOPDT/SOPDT. |
| **AdaptiveMpc** | ⚠️ PARCIAL | RLS real + change-detection + QP redesign. **Lacuna:** rebuild só ARX ordem 2; ordens ≥4 marcadas "extensão futura". |
| **Cascade** | ✅ COMPLETO | ISA-PID + anti-windup + bumpless transfer; master→slave handoff. |
| **SmithPredictor** | ✅ COMPLETO | Delay buffer circular + no-delay/with-delay predictors + correção. |
| **Linearization** | ✅ COMPLETO | Feedback `u=(v-f(x))/g(x)` + exact I/O via Lie derivatives (Lf h, Lg h). |
| **Optimization** | ✅ COMPLETO | Matrix algebra + QP (Levenberg-Marquardt) + LP (Simplex com slack). |
| **PlcCodeGen** | ✅ COMPLETO (Bloco B4 entregue) | 3 novos geradores ST adicionados: `StructuredTextCascadeGenerator`, `StructuredTextLinearizationGenerator`, `StructuredTextSmithGenerator`. |
| **ProcessVariables** | ✅ COMPLETO | Tipado com units/ranges/nominal/kind. |
| **Factories** | ✅ COMPLETO | Dispatch table para as 9 kinds + adapter. |

### 1.2 Engine/Core — frameworks conectados ✅ (Bloco D + E entregues)

| Framework | CORE spec | Status pós-`f45c68b` |
|---|---|---|
| Validation | 003 | ✅ IMPLEMENTED |
| Diagnostics | 004 | ✅ IMPLEMENTED |
| Plugin | 005 | ⚠️ PARCIAL (5/8 tipos) |
| Persistence | 006 | ✅ IMPLEMENTED + `FilePersistenceProvider` concreto |
| Serialization | 007 | ⚠️ PARCIAL |
| EventBus | 008 | ✅ Thread-safe, registrado no DI da Application (Bloco E2 entregue) |
| DI | 009 | ⚠️ PARCIAL (Microsoft.Extensions.DI usado em paralelo) |
| Simulation | 010 | ✅ `SimulationEngine` real + `SimulationSession` (Bloco D1 entregue) |
| Runtime | 011 | ✅ `RuntimeEngine` real + `RuntimeSession` (Bloco D2 entregue) |
| Export | 012 | ⚠️ PARCIAL |

**Problema estrutural resolvido:** `ExecutionPipeline.ExecuteWithControllers` (Bloco D3) agora invoca controllers reais, não apenas ordena e grava IDs. CORE-008 EventBus registrado no DI da Application.

### 1.3 Domain — com aggregates ✅ (Bloco A entregue)

| Módulo | Status | Notas |
|---|---|---|
| Common/Base (EntityBase, AggregateRoot, ValueObject) | ✅ | DDD correto. |
| Common/Results (Result&lt;T&gt;) | ✅ | |
| **Controllers/Controller** | ✅ NOVO | Aggregate root com Id, Name, Type, Lifecycle. |
| Models (ControlModel, ModelVariable) | ✅ | ControlModel com factory `Create()` imutável + variables/dynamics. |
| Projects (Project) | ✅ | Aggregate com children: Controllers, Models, Strategies. |

### 1.4 Application — completa ✅ (Bloco A4/A5 + E4 entregues)

| Módulo | Status | Notas |
|---|---|---|
| Fuzzy (EvaluateFuzzyControllerHandler) | ✅ | CQRS real. |
| EventBus (InMemoryEventBus) | ✅ | Thread-safe + CORE EventBus registrado. |
| Projects (ProjectService, CreateProjectHandler) | ✅ | Create/Open/Save/Delete/List. |
| **Controllers/ControllerService** | ✅ NOVO | Handlers CRUD completos. |
| **Models/ModelService** | ✅ NOVO | Handlers CRUD completos. |
| **DigitalTwin/DigitalTwinService** | ✅ NOVO (Bloco E4) | `SimulatePid()` roda PID contra FOPDT por N scans. |
| Interfaces | ✅ | IControllerService, IModelService, IProjectRepository. |
| DI | ✅ | Registra todos os serviços + CORE EventBus. |

### 1.5 Infrastructure — povoada ✅ (Bloco B1/B2 entregues)

| Módulo | Status |
|---|---|
| `FilePersistenceProvider` (JSON) | ✅ IMPLEMENTED |
| `JsonProjectRepository` | ✅ IMPLEMENTED |
| `FileSystem/IFileSystem` | ✅ IMPLEMENTED |
| DI wiring | ✅ |

### 1.6 Export — povoada ✅ (Bloco B3 entregue)

| Módulo | Status |
|---|---|
| `JsonExportProvider` | ✅ IMPLEMENTED |
| `PlcCodeExportProvider` (delegando ao Engine/PlcCodeGen) | ✅ IMPLEMENTED |
| `ExportServiceCollectionExtensions` | ✅ IMPLEMENTED |

### 1.7 Presentation — dark + 6 editores ✅ (Bloco C entregue)

| Item | Status | Notas |
|---|---|---|
| Shell (MainWindow, Titlebar, Sidebar, Tabs, Output) | ✅ DARK | Fase 2 + fix CS0234. |
| Tema (DarkTheme/LightTheme/ThemeManager) | ✅ | 39+ keys, Auto/Light/Dark. |
| **6 editores dark** (PID, MPC, Adaptive, Identification, Neural, Optimization) | ✅ NOVO | Cards específicos por kind. |
| `NavigationService` | ✅ | Registry pattern (Kind → ViewModel). |
| `WorkspaceView.xaml` | ✅ DARK | Cores hardcoded removidas. |
| `MainWindowViewModel.RunSimulation` | ✅ | Detecta PID e roda Digital Twin (100 scans). |

---

## 2. Resumo executivo — onde estamos

| Camada | Maturidade (antes) | Maturidade (agora) |
|---|---|---|
| Engine (algoritmos) | Produção | **Produção** |
| Engine/Core (frameworks) | Inexistente na prática | **Conectado e funcional** |
| Domain | Anêmico | **Com aggregates** |
| Application | Parcial | **Completa** |
| Infrastructure | Vazio | **Povoada** |
| Export | Vazio | **Povoada** |
| Presentation shell | Dark pronto | **Dark pronto** |
| Presentation editores | 6/7 stub | **6 editores dark completos** |
| Build | Não compilava | **9/9 verde em 18.2s** |

**Conclusão:** o motor e a casa agora estão conectados. Falta a normalização de encoding (LF/CRLF já tratado pelo git) e os Pontos Futuros abaixo.

---

## 3. Pontos futuros (fora do escopo entregue)

- **AdaptiveMpc ARX ordem ≥4**: marca como "extensão futura" no código original — não incluído.
- **MIMO MPC** no PlcCodeGen: só avisa hoje; não incluído.
- **CORE-005 Plugin framework completo** (Loader via reflection): não incluído — sem caso de uso consumindo plugins.
- **CORE-007 Serialization concrete serializers**: não incluído — JSON/XML serializers do Fuzzy layer são suficientes.
- **CORE-009 DI unification**: Microsoft.Extensions.DI ainda em paralelo com o Core/DI; refactor arquitetural não executado (risco vs benefício baixo).

---

## 4. Próximos passos sugeridos

1. **Testes funcionais** — rodar a suíte completa:
   ```powershell
   dotnet test ALE.AdvancedControlStudio.slnx
   ```
2. **Smoke test runtime** — abrir a IDE, criar um Project, adicionar um Controller PID, clicar em Run Simulation, confirmar que o Digital Twin executa 100 scans e loga no Output panel.
3. **Encoding mojibake** — ver comando PowerShell abaixo para normalizar BOM/UTF-8 nos arquivos com `Ã§`/`Ã£` nos comentários (não bloqueia build, mas fica feio no GitHub).
4. **Tag de release** — quando o smoke test passar:
   ```powershell
   git tag -a v0.4.0-blocos-AE -m "Blocos A-E completos: Domain+App+Infra+Export+6 dark editors+Engine/Core destravado+Digital Twin"
   git push origin v0.4.0-blocos-AE
   ```
