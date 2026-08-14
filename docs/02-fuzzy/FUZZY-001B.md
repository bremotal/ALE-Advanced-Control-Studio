# FUZZY-001B

## Release

0.5

## Milestone

FUZZY-001B

## Feature

Membership Functions

---

## Objetivo

Implementar exclusivamente o núcleo de *Membership Functions* do módulo Fuzzy.

Esta implementação deve construir uma hierarquia matemática reutilizável para funções de pertinência do ALE Advanced Control Studio. Cada função deve ser tratada como um objeto de engenharia, e não apenas como uma expressão matemática: deve possuir identidade, metadados, tipo, parâmetros fortemente tipados, domínio e operações matemáticas reutilizáveis.

Não implementar Rule Engine.

Não implementar Inference Engine.

Não implementar Defuzzification.

Não implementar Fuzzy Variables.

Não implementar Surface Generator.

Não implementar Lookup Tables.

---

## Dependências

- Release 0.4 Foundation
- FUZZY-001A

---

## Pode criar arquivos somente em

```text
src/ALE.ACS.Engine/Fuzzy/MembershipFunctions
src/ALE.ACS.Engine/Fuzzy/Types
src/ALE.ACS.Engine/Fuzzy/Validation
src/ALE.ACS.Engine/Fuzzy/Serialization
tests/ALE.ACS.Engine.Tests/Fuzzy
```

## Pode modificar

Somente arquivos criados nesta implementação.

## Não pode modificar

- Core
- Application
- Presentation
- Infrastructure
- Export
- Domain
- Shared
- Engine Foundation
- Arquivos `.csproj`
- Solution
- `Directory.Build.*`
- Testes existentes

---

## Arquitetura

Implementar a seguinte hierarquia:

```text
IMembershipFunction
        |
MembershipFunctionBase
        |
        +-- TriangularMembershipFunction
        +-- TrapezoidalMembershipFunction
        +-- GaussianMembershipFunction
        +-- GeneralizedBellMembershipFunction
        +-- SigmoidMembershipFunction
        +-- SShapeMembershipFunction
        +-- ZShapeMembershipFunction
        +-- PiShapeMembershipFunction
        +-- SingletonMembershipFunction
        +-- PiecewiseLinearMembershipFunction
```

Também criar:

- `IMembershipFunctionFactory`
- `MembershipFunctionType`
- `MembershipFunctionMetadata`
- `MembershipFunctionParameters`

Toda Membership Function deve ser 100% parametrizada e fortemente tipada. Não utilizar arrays genéricos, `object[]`, `object`, `dynamic` ou `Dictionary` para representar parâmetros matemáticos.

---

## Tipos

### MembershipFunctionType

Criar enum contendo:

```text
Triangular
Trapezoidal
Gaussian
GeneralizedBell
Sigmoid
SShape
ZShape
PiShape
Singleton
PiecewiseLinear
Custom
```

Esta enumeração é a identificação única do tipo de função para futuras integrações de interface gráfica, serialização, exportação, PLC, XML, JSON e biblioteca.

### MembershipFunctionMetadata

Criar tipo contendo os campos:

```text
Id
Name
Description
Version
Author
Category
Created
LastModified
```

### MembershipFunctionParameters

Criar classe imutável responsável por armazenar parâmetros matemáticos.

Os parâmetros de cada função devem ser expostos por propriedades com nomes semânticos e tipos fortes. Não utilizar `Dictionary`, `object` ou `dynamic`.

---

## IMembershipFunction e MembershipFunctionBase

`MembershipFunctionBase` deve implementar o comportamento comum e servir como base obrigatória de todas as funções.

### Propriedades obrigatórias

```text
Name
Description
Type
Metadata
Parameters
DomainMinimum
DomainMaximum
```

### Operações obrigatórias

```text
Evaluate(double)
Evaluate(ReadOnlySpan<double>)
Derivative(double)
Integral(double)
Area()
Centroid()
Normalize()
Clone()
Translate(double)
Scale(double)
Mirror()
Validate()
```

Quando uma operação puder adotar uma implementação comum, ela deve ser fornecida pela classe base. As transformações devem devolver uma nova função; nenhuma instância pode ser alterada após a construção.

### Regras gerais

Todas as Membership Functions devem:

- ser imutáveis;
- ser thread-safe;
- possuir documentação XML;
- validar parâmetros no construtor;
- rejeitar `NaN`;
- rejeitar `Infinity`;
- rejeitar parâmetros inválidos;
- manter o resultado de `Evaluate` no intervalo `[0, 1]`;
- suportar serialização sem depender da camada Presentation.

O suporte, o pico, a área e o centroide devem poder ser obtidos da função ou de seu comportamento matemático, para permitir futuras extensões de auto tuning, MPC, otimização, geração de superfícies e geração automática de lookup tables.

---

## Funções a implementar

### TriangularMembershipFunction

Parâmetros fortemente tipados:

```text
A
B
C
```

Validar `A <= B <= C` e implementar `Evaluate()`.

### TrapezoidalMembershipFunction

Parâmetros fortemente tipados:

```text
A
B
C
D
```

Validar `A <= B <= C <= D` e implementar `Evaluate()`.

### GaussianMembershipFunction

Parâmetros fortemente tipados:

```text
Mean
Sigma
```

Validar `Sigma > 0` e implementar `Evaluate()`.

### GeneralizedBellMembershipFunction

Parâmetros fortemente tipados:

```text
A
B
C
```

Validar parâmetros que inviabilizem a função, incluindo largura diferente de zero, e implementar `Evaluate()`.

### SigmoidMembershipFunction

Parâmetros fortemente tipados:

```text
Slope
Center
```

Implementar `Evaluate()`.

### SShapeMembershipFunction

Receber parâmetros fortemente tipados para os limites inferior e superior da transição. Validar sua ordenação e implementar `Evaluate()`.

### ZShapeMembershipFunction

Receber parâmetros fortemente tipados para os limites inferior e superior da transição. Validar sua ordenação e implementar `Evaluate()`.

### PiShapeMembershipFunction

Receber parâmetros fortemente tipados para os quatro pontos que formam as transições ascendente e descendente. Validar a ordenação e implementar `Evaluate()`.

### SingletonMembershipFunction

Parâmetro fortemente tipado:

```text
Center
```

Implementar `Evaluate()`.

### PiecewiseLinearMembershipFunction

Receber uma coleção ordenada e tipada de pontos. Validar que a coleção possui pontos suficientes, coordenadas finitas, posições ordenadas e graus de pertinência válidos. Implementar interpolação linear em `Evaluate()`.

---

## Serialização

Todas as Membership Functions devem ser serializáveis independentemente da Presentation. A representação deve preservar o tipo e os parâmetros fortemente tipados.

Exemplo esperado:

```json
{
  "type": "Triangular",
  "parameters": {
    "a": 0,
    "b": 25,
    "c": 50
  }
}
```

---

## Testes

Criar testes unitários novos para todas as Membership Functions, sem modificar testes existentes.

Cobrir no mínimo:

- `Evaluate()` e valores de fronteira;
- domínio;
- `Clone()`;
- `Normalize()`;
- `Translate()`;
- `Scale()`;
- `Mirror()`;
- `Derivative()`;
- `Integral()`;
- área;
- centroide;
- serialização;
- validação;
- parâmetros inválidos;
- rejeição de `NaN` e `Infinity`.

Cobertura mínima: **95%** para os arquivos criados nesta implementação.

---

## Critérios de aceite

Executar:

```text
dotnet build
```

Caso existam erros provenientes desta implementação, corrigi-los automaticamente e executar novamente até obter:

```text
BUILD SUCCEEDED
```

Em seguida, executar:

```text
dotnet test
```

Caso existam testes quebrados apenas desta implementação, corrigi-los automaticamente e executar novamente até que todos os testes sejam aprovados.

---

## Resultado esperado

Ao concluir, apresentar apenas:

- Arquivos criados;
- Arquivos modificados;
- Resumo técnico;
- Resultado do Build;
- Resultado do Test.

Não implementar nenhuma funcionalidade adicional.

---

## Proibido

Não criar:

- `FuzzyVariable`;
- `FuzzyTerm`;
- `FuzzyRule`;
- `InferenceEngine`;
- `KnowledgeBase`;
- aggregation;
- composition;
- defuzzification;
- surface generator;
- lookup tables;
- PLC export;
- parser de regras;
- strings que representem regras fuzzy;
- qualquer funcionalidade fora desta especificação.

Caso este documento não seja encontrado, parar imediatamente. Não inferir arquitetura e não criar arquivos.
