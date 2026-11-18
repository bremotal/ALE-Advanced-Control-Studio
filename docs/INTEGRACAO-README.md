# Document Infrastructure — Entrega Completa

**Branch:** `feature/document-infrastructure`
**Commit:** `635925c feat(documents): Document Infrastructure complete`
**Testes:** 192/192 passando (164 Engine + 28 Application)
**Data:** 12/11/2026

---

## ⚠️ Antes de aplicar — AÇÃO SUA (não automática)

### 1. Corrigir a mensagem do commit "Sprint 1.0"

O último commit (`7947782`) tem uma mensagem copiada de outro projeto
("Sprint 1.0 - Execucao de treino, historico agrupado, login automatico
e edicao de series"). **Não apague esse commit** — ele contém TODO o
trabalho do ALE ACS (docs CORE-001..012, Engine Fuzzy completa,
Application, etc.). Apenas corrija a mensagem localmente:

```bash
# Não recomendado se você já publicou e tem colaboradores.
# Se for só seu repositório pessoal:
git commit --amend -m "feat: ALE ACS foundation (CORE-001..012, Fuzzy engine, IDE shell)"
git push --force-with-lease
```

### 2. Como aplicar este patch

```bash
# Na raiz do repositório local
git fetch origin
git checkout main   # ou a branch que você usa
git apply --check /caminho/para/0001-feat-documents-*.patch   # valida
git apply /caminho/para/0001-feat-documents-*.patch           # aplica

# OU, se preferir criar uma branch:
git checkout -b feature/document-infrastructure
git am /caminho/para/0001-feat-documents-*.patch
```

Após aplicar, abra a Solution no Visual Studio e faça **Rebuild Solution**
(`Ctrl+Shift+B`). A camada Presentation só compila no Windows (WPF).

---

## 🎯 O que foi entregue

### Problema original
A persistência `.aleacs` vivia **dentro** do `MainWindowViewModel`
(`JsonSerializer.Serialize` + `File.WriteAllText` diretos), violando a
Clean Architecture do próprio `AGENTS.md`. Além disso:
- A associação documento↔projeto era por **string** (`Kind == "Project"
  && Title == project.Name`), frágil.
- Não havia confirmação ao fechar documentos não salvos.
- Não havia "Save All".
- Arquivos órfãos na raiz do repo.

### Solução
Implementação completa da **Document Infrastructure** em 4 camadas,
seguindo a Clean Architecture e o padrão DDD existente.

---

## 📐 Arquitetura entregue

```
┌─────────────────────────────────────────────────────────────┐
│ Presentation (WPF)                                          │
│   MainWindowViewModel ── usa ──► IProjectPersistenceService │
│   (sem JsonSerializer/File direto)                ▲           │
└───────────────────────────────────────────────────┼───────────┘
                                                    │
┌───────────────────────────────────────────────────┼───────────┐
│ Application                                        ▼           │
│   IDocumentService ◄── DocumentService (CRUD)                 │
│   IProjectPersistenceService ◄── ProjectPersistenceService    │
│       ├─ IProjectRepository        (contrato do Domain)        │
│       ├─ IDocumentRepository       (contrato do Domain)        │
│       └─ IProjectFileStorage       (contrato em Application)  │
│   ProjectSnapshot / DocumentSnapshot  (DTOs de persistência)  │
└───────────────────────────────────────────────────┼───────────┘
                                                    │
┌───────────────────────────────────────────────────┼───────────┐
│ Infrastructure                                     ▼           │
│   InMemoryProjectRepository  ◄── IProjectRepository           │
│   InMemoryDocumentRepository  ◄── IDocumentRepository          │
│   JsonProjectFileStorage      ◄── IProjectFileStorage          │
│   FileSystemService           ◄── IFileSystemService           │
└───────────────────────────────────────────────────┼───────────┘
                                                    │
┌───────────────────────────────────────────────────┼───────────┐
│ Domain                                             ▼          │
│   Document (aggregate) — Create/Restore/SetContent/MarkSaved   │
│   DocumentType (enum): Generic/Project/Model/Controller/...    │
│   DocumentState (enum): Draft/Clean/Dirty/Closed               │
│   IDocumentRepository, IProjectRepository (contratos)          │
│   Project.RegisterDocument/UnregisterDocument (associação)    │
│   EntityBase.RestoreIdentity (reconstituição)                  │
│   Result (não-genérico) + Result<T> (existente)                │
└─────────────────────────────────────────────────────────────┘
```

### Fluxo de Save (exemplo)
1. Usuário clica em **Salvar** → `SaveProjectCommand`
2. `MainWindowViewModel.SaveProjectAsync` → `IProjectPersistenceService.SaveAsync`
3. `ProjectPersistenceService`:
   - Associa documentos ao agregado Project (`RegisterDocument`)
   - Persiste Project via `IProjectRepository`
   - Persiste documentos em lote via `IDocumentRepository.SaveAllAsync`
   - Serializa snapshot via `IProjectFileStorage.WriteAsync`
   - Marca Project como salvo

---

## 📁 Arquivos criados / modificados

### Domain (novos)
- `src/ALE.ACS.Domain/Documents/DocumentType.cs`
- `src/ALE.ACS.Domain/Documents/DocumentState.cs`
- `src/ALE.ACS.Domain/Documents/Document.cs`
- `src/ALE.ACS.Domain/Documents/IDocumentRepository.cs`
- `src/ALE.ACS.Domain/Projects/IProjectRepository.cs`

### Domain (modificados)
- `src/ALE.ACS.Domain/Projects/Project.cs` — `RegisterDocument`/`UnregisterDocument`
- `src/ALE.ACS.Domain/Common/Base/EntityBase.cs` — `RestoreIdentity`, setters protegidos
- `src/ALE.ACS.Domain/Common/Results/Result.cs` — adicionado `Result` não-genérico

### Application (novos)
- `src/ALE.ACS.Application/Documents/DocumentDto.cs`
- `src/ALE.ACS.Application/Documents/IDocumentService.cs` (comandos + DTOs)
- `src/ALE.ACS.Application/Documents/DocumentService.cs`
- `src/ALE.ACS.Application/Projects/ProjectSnapshot.cs`
- `src/ALE.ACS.Application/Projects/IProjectPersistenceService.cs`
  (inclui `IProjectFileStorage` e `ProjectPersistenceService`)

### Application (modificados)
- `src/ALE.ACS.Application/DependencyInjection/ApplicationServiceCollectionExtensions.cs`
  — registra `IDocumentService` e `IProjectPersistenceService`

### Infrastructure (novos)
- `src/ALE.ACS.Infrastructure/Documents/InMemoryDocumentRepository.cs`
- `src/ALE.ACS.Infrastructure/Projects/InMemoryProjectRepository.cs`
- `src/ALE.ACS.Infrastructure/FileSystem/IFileSystemService.cs` (+ `FileSystemService`)
- `src/ALE.ACS.Infrastructure/Projects/JsonProjectFileStorage.cs`

### Infrastructure (modificados)
- `src/ALE.ACS.Infrastructure/DependencyInjection/InfrastructureServiceCollectionExtensions.cs`
  — registra repositórios, file system e storage JSON

### Presentation (modificados)
- `src/ALE.ACS.Presentation/ALE.ACS.Presentation.csproj`
  — adicionada referência a `ALE.ACS.Infrastructure`; removidos excludes
  mortos de `Controls/` e `NovaPasta/`
- `src/ALE.ACS.Presentation/App.xaml.cs` — chama `AddInfrastructure()` antes de
  `AddApplication()`/`AddPresentation()`
- `src/ALE.ACS.Presentation/ViewModels/MainWindowViewModel.cs`
  — save/open via `IProjectPersistenceService`; `SaveAllCommand`;
  confirmação ao fechar com documentos não salvos; mapeamento entre
  `EngineeringDocument` (Presentation) e `Document` (Domain)
- `src/ALE.ACS.Presentation/Views/MainWindow.xaml`
  — novo item "Salvar Tudo" no menu Arquivo

### Presentation (movido)
- `src/ALE.ACS.Presentation/as.cs` →
  `src/ALE.ACS.Presentation/Properties/AssemblyInfo.cs`
  (mesmo conteúdo, localização convencional)

### Tests (novos)
- `tests/ALE.ACS.Application.Tests/Documents/DocumentTests.cs` — entidade Document + associação Project
- `tests/ALE.ACS.Application.Tests/Documents/DocumentServiceTests.cs` — use cases CRUD
- `tests/ALE.ACS.Application.Tests/Documents/ProjectPersistenceServiceTests.cs` — save/load round-trip do `.aleacs`

### Tests (modificados)
- `tests/ALE.ACS.Application.Tests/ALE.ACS.Application.Tests.csproj`
  — referência a `ALE.ACS.Infrastructure` (para `InMemoryDocumentRepository`)

### Higiene do repositório
- Removidos arquivos órfãos vazios `dotnet` e `git` da raiz
- `.gitignore` agora ignora `/dotnet` e `/git` (caso reapareçam)

---

## ✅ Validação executada (aqui no sandbox Linux)

```
Build (Linux, .NET 10.0.404):
  ALE.ACS.Domain         : OK ✅
  ALE.ACS.Application    : OK ✅
  ALE.ACS.Engine         : OK ✅
  ALE.ACS.Infrastructure : OK ✅
  ALE.ACS.Export         : OK ✅
  ALE.ACS.Shared         : OK ✅

Testes:
  Engine.Tests       : 164/164 ✅
  Application.Tests  :  28/28 ✅  (5 existentes + 23 novos)
  ─────────────────────────────────
  TOTAL              : 192/192 ✅
```

### O que NÃO pude validar aqui
- **Presentation (WPF)** — só compila no Windows (`net10.0-windows`).
  Você precisa validar visualmente no VS: abrir/criar projeto, salvar,
  carregar, fechar com documento dirty, "Salvar Tudo".

---

## 🔌 Compatibilidade com `.aleacs` existente

**Atenção:** o formato do `.aleacs` mudou. Os arquivos `.aleacs` antigos
(gerados pelo `MainWindowViewModel` anterior com `ProjectFile` record)
**não serão lidos** pelo novo `JsonProjectFileStorage` (esquema
`ProjectSnapshot` é diferente: usa `projectId` Guid, `documentIds`,
etc.).

Se você tiver projetos `.aleacs` antigos importantes, há duas opções:
1. **Migrar manualmente** — abra cada um num editor de texto e ajuste
   o JSON para o novo schema (ver `ProjectSnapshot` /
   `DocumentSnapshot` em `Application/Projects/ProjectSnapshot.cs`).
2. **Adicionar um migrador** — posso escrever um
   `LegacyAleacsMigrator` em Infrastructure que detecta o formato
   antigo e converte on the fly. **Diga se quer esta opção.**

---

## 🔄 Próximos passos sugeridos

1. **Você valida no VS** (Rebuild + abrir IDE + testar save/load).
2. Se houver erro de compilação no Presentation (não pude validar WPF
   aqui), me mande a mensagem de erro do compilador que eu corrijo.
3. Decidir sobre migração de `.aleacs` antigos (opção 2 acima).
4. Próximo feature: **Engineering Model editor** (item do Roadmap
   depois de Document Infrastructure).

---

## 📦 Arquivos nesta entrega

- `0001-feat-documents-Document-Infrastructure-complete-Doma.patch`
  — patch git pronto para `git apply` ou `git am`
- `INTEGRACAO-README.md` — este documento
