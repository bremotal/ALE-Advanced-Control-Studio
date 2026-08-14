FOUNDATION-CORE.md - ALE Advanced Control Studio
01 INTRODUÇÃO
O ALE ACS é um IDE (Integrated Development Environment) para Engenharia de Controle Avançado (APC). A arquitetura é Engine First e Domain First, baseada em Clean Architecture, DDD, CQRS e MVVM.

02 ARQUITETURA
Domínio: Entidades ricas, AggregateRoot, Result. Não depende de ninguém.
Aplicação: Casos de uso, Commands e Handlers. Depende de Domínio.
Motor: Motores matemáticos (Fuzzy, MPC, PID). Depende de Domínio e Compartilhado.
Apresentação: WPF + MVVM (CommunityToolkit). Depende de Application, infrastructure e Engine.
03 PADRÕES DE PROJETO
CQRS (Commands e Handlers separados no Application)
Design Orientado por Domínio (Entidades com construtores privados e Factory Methods)
Padrão de Resultado (Tratamento de erros sem lançar Exceptions)
REGRAS DE IMPLEMENTAÇÃO
NÃO

Criar nova solução ou projetos
criar métodos de marcador
mudar as classes Foundation (AggregateRoot, Result, Project)
Renomear namespaces existentes
Use dinâmica ou reflexão
criar comentários TODO
SEMPRE

implementar classe completa com comentários XML
usar o padrão CQRS para serviços de aplicação (Command -> Handler)
use Resultado para o tratamento de erros
implementar testes unitários para classes de motor
Execute a build e os testes