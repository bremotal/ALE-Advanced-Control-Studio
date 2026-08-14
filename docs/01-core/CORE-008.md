# ALE Advanced Control Studio

# CORE-008 – Event System

**Document ID:** ALE-ACS-SDS-CORE-008
**Version:** 1.0.0
**Status:** FROZEN

## Objetivo

Definir o sistema interno de eventos do ALE ACS para desacoplar os componentes do Core, permitindo comunicação baseada em eventos sem dependências diretas.

## Estrutura

```
Core/Common/Events
├── IEvent
├── IEventHandler<TEvent>
├── EventBus
├── EventDispatcher
├── EventSubscription
├── EventContext
├── EventPriority
├── EventResult
└── EventException
```

## Responsabilidades

### IEvent

Contrato base para todos os eventos do sistema.

### IEventHandler<TEvent>

Processa um tipo específico de evento.

### EventBus

Responsável por:

- Publicar eventos
- Registrar assinantes
- Remover assinantes
- Encaminhar eventos ao Dispatcher

### EventDispatcher

Executa os handlers registrados respeitando prioridade.

### EventContext

Disponibiliza informações compartilhadas durante a publicação.

## Eventos previstos

- ProjectOpened
- ProjectClosed
- VariableChanged
- StrategyChanged
- BlockAdded
- BlockRemoved
- ValidationCompleted
- ExecutionStarted
- ExecutionFinished

A implementação destes eventos ocorrerá em módulos específicos.

## Regras

- Nenhum componente referencia diretamente outro componente.
- Toda comunicação entre módulos deve preferir eventos.
- O EventBus deve ser thread-safe e preparado para evolução assíncrona.

## Critérios de Aceitação

- Build sem erros.
- Build sem warnings.
- Testes unitários cobrindo publicação, assinatura, remoção e ordem de execução.

## Prompt Oficial para o Copilot

Implemente integralmente o Event System conforme esta especificação. Desenvolva apenas a infraestrutura reutilizável de eventos, documentação XML e testes unitários. Não implemente eventos específicos dos algoritmos. Não execute build nem testes.
