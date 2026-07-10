# ADR-0001 — Architecture Baseline

- **Status:** Accepted
- **Date:** 2026-07-09
- **Decision Makers:** Projeto ALE Advanced Control Studio

---

# Context

O ALE Advanced Control Studio será uma plataforma de engenharia para
desenvolvimento, simulação, sintonia e monitoramento de sistemas de controle.

Como o projeto terá crescimento contínuo, foi necessária a definição de uma
arquitetura capaz de suportar evolução sem grandes refatorações.

---

# Decision

A solução será organizada em camadas independentes.

```
Presentation
        │
Application
        │
Domain
        │
Engine
        │
Infrastructure
```

Cada camada possui responsabilidade única.

## Presentation

Responsável pela interface WPF.

Contém:

- Views
- ViewModels
- Controls
- Themes
- Dependency Injection

Não deve conter regras de negócio.

---

## Application

Responsável pelos casos de uso.

Contém:

- Commands
- Queries
- Services
- Interfaces

Coordena o fluxo da aplicação.

---

## Domain

Contém exclusivamente as regras de negócio.

Não conhece:

- Interface
- Banco de dados
- Frameworks

---

## Engine

Responsável pelos algoritmos de engenharia.

Exemplos:

- PID
- Cascade
- MPC
- FeedForward
- Fuzzy
- Simulation
- Auto Tune

O Engine deve ser independente da interface gráfica.

---

## Infrastructure

Responsável pelas integrações externas.

Exemplos:

- Logging
- Configuration
- SQLite
- SQL Server
- OPC UA
- Modbus
- Exportação

---

# Consequences

Benefícios esperados:

- baixo acoplamento
- alta coesão
- facilidade de testes
- facilidade de manutenção
- escalabilidade

---

# Status

Esta decisão passa a ser a arquitetura oficial do projeto.