IDE-FOUNDATION-CORE.md - Infraestrutura da IDE (Sprint 3)
01 - OBJETIVO
Conectar o Shell WPF (Presentation) aos motores matemáticos (Engine) através de serviços na camada Application. A IDE deve permitir criar projetos, adicionar controladores, salvar/carregar (persistência) e exibir a estrutura em árvore dinamicamente.

02 - ARQUITETURA E COMPONENTES
Os componentes residirão majoritariamente em e .ALE.ACS.ApplicationALE.ACS.Presentation.Services

2.1 Event Bus (Aplicativo)
Comunicação desacoplada entre serviços e ViewModels.

IEventBus: Interface com e .void Publish<T>(T event)void Subscribe<T>(Action<T> handler)
EventBus: Implementação thread-safe.
2.2 Fábrica de Controladores (Aplicação)
Cria instâncias de controladores matemáticos e as adiciona ao projeto atual.

IControllerFactory: Interface com .Controller Create(string name, ControllerType type)
ControllerFactory: Implementação. Para Release 0.6, cria mockados ou reais e os anexa ao .FuzzyControllerProject
2.3 Gerente de Projeto (Aplicação)
Gerencia o ciclo de vida do projeto na memória.

IProjectManager: Mantém o ativo. Métodos: , , .ProjectCreateProjectLoadProjectSaveProject
Deve publicar eventos no (ex: , ).IEventBusProjectOpenedEventControllerAddedEvent
2.4 Persistência (Aplicação/Infraestrutura)
Serialização do projeto em arquivo (JSON)..acs

IProjectSerializer: , .Project Load(string path)void Save(Project project, string path)
2.5 Serviço de Exploração (Apresentação)
Mantém a árvore visual () sincronizada com o .ObservableCollection<TreeNode>ProjectManager

Assina eventos do. Se um controlador for adicionado, adiciona um nó na árvore.IEventBus
2.6 Gerente de Documentos (Apresentação)
Gerencia as abas abertas no Workspace.

IDocumentManager: Mantém.ObservableCollection<EngineeringDocument>
Métodos: , .OpenDocument(Controller)CloseDocument(Guid id)
03 - REGRAS DE IMPLEMENTAÇÃO
Implementar exclusivamente o escopo descrito.
Não criar Solution ou csproj.
Não modificar as classes da Engine (Fuzzy, MPC, etc.).
Usar padrão CQRS/Result na Application.
Implementar primeiro os testes xUnit (TDD) para o ProjectManager, ControllerFactory e EventBus.
Ao finalizar, apresentar apenas: Arquivos criados, Arquivos modificados, Resumo técnico.
Parar imediatamente após concluir a implementação.
