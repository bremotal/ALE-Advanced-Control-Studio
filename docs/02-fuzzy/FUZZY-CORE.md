FUZZY-CORE.md - Módulo Fuzzy Engine (Versão 0.6)
01 INTRODUÇÃO
Este documento específico a engine Fuzzy do ALE ACS. A engine deve ser capaz de realizar inferências Mamdani e Sugeno, com funções de pertinência configuráveis e defuzzificação.

02 ARQUITETURA E ORGANIZAÇÃO
A engine residirá no projeto dentro da pasta . Estrutura de massas:ALE.ACS.EngineFuzzy/

Fuzzy/MembershipFunctions/ (Funções Triangulares, Trapezoidais, Gaussana, etc.)
Fuzzy/Variables/ (FuzzyVariable, FuzzyTerm)
Fuzzy/Rules/ (Regra Difusa)
Fuzzy/Inference/ (Mamdani, Sugeno)
Fuzzy/Defuzzification/ (Centroide, Bissetor, MÃE)
03 BASE DE INTERFACES
Todas as funções de pertinência devem implementar a interface .IMembershipFunction

namespace ALE.ACS.Engine.Fuzzy.MembershipFunctions;/// <summary>/// Define uma função de pertinência fuzzy./// </summary>public interface IMembershipFunction{    /// <summary>    /// Calcula o grau de pertinência para um valor de entrada crisp.    /// </summary>    /// <param name="x">Valor crisp de entrada.</param>    /// <returns>Grau de pertinência entre 0.0 e 1.0.</returns>    double Evaluate(double x);    /// <summary>    /// Retorna os limites do suporte da função (onde y > 0).    /// </summary>    (double Min, double Max) GetSupportBounds();}
REGRAS DE IMPLEMENTAÇÃO PARA COPILOT
NÃO

usar bibliotecas matemáticas externas (implementar matemática usando apenas System.Math)
criar modelos anêmicos
mudar Camada de domínio
SEMPRE

implementar IMembershipFunction para toda função de associação
garantir que Avalie retornos estritamente entre 0.0 e 1.0
criar testes xUnit em ALE. ACS. Engine.Tests para cada função
Coloque os testes em tests/ALE.ACS.Engine.Tests/Fuzzy/

05 CAPÍTULO 3 - REGRAS FUZZY (RULES ENGINE)
Neste capítulo, definimos como o conhecimento do engenheiro é estruturado em regras. Uma regra fuzzy é composta por antecedentes (condições) e um consequente (ação/resultado).

5.1 - Arquitetura e Organização
As classes residirão no projeto dentro da pasta .ALE.ACS.EngineFuzzy/Rules/

Fuzzy/Rules/FuzzyOperator.cs (Enum)
Fuzzy/Rules/FuzzyClause.cs (Uma condição: Variável IS Termo)
Fuzzy/Rules/FuzzyRule.cs (A regra completa: SE Cláusulas THEN Clause)
5.2 - Definição de FuzzyOperator
Define como múltiplos antecedentes em uma regra são combinados.

And (Interseção - geralmente calculado como Mínimo)
Or (União - geralmente calculado como Máximo)
5.3 - Definição de FuzzyClause
Uma cláusula representa a associação de uma a um específico. Ex: "Temperatura é Alta"FuzzyVariableFuzzyTerm

Deve conter a referência à .FuzzyVariable
Deve conter a referência ao .FuzzyTerm
Deve ter uma propriedade (ex: "Temperatura NÃO é Alta").IsNegated
5.4 - Definição de FuzzyRule
Representa a regra completa.

Deve ter uma lista de cláusulas antecedentes (condições).
Deve ter um operador lógico () definindo como unir os antecedentes.FuzzyOperator
Deve ter uma única cláusula consequente (o resultado).
Não pode ter antecedentes vazios.
O consequente não pode ser nulo.
5.5 - REGRAS DE IMPLEMENTAÇÃO (CAPÍTULO 3)
NÃO

criar lógica de inferência matemática ainda (apenas a modelagem das regras)
aceitar variáveis ou termos nulos nas cláusulas
usar dynamic ou reflection
SEMPRE

validar se o passado na cláusula realmente pertence à informada (usar validação de referência de memória ou Id se existir)FuzzyTermFuzzyVariable
lançar se a regra for criada sem antecedentes ou sem consequenteArgumentException
implementar testes xUnit para:
Criar regra válida com AND
Criar regra válida com OR
Falhar ao criar regra sem antecedentes
Falhar ao adicionar cláusula onde o termo não pertence à variável
colocar os testes em 'tests/ALE. ACS. Engine.Testes/Fuzzy/Regras/

06 CAPÍTULO 4 - MOTORES DE INFERÊNCIA E LINEARIZADORES
Neste capítulo, implementamos a máquina que executa o raciocínio fuzzy. Existem dois tipos principais de motores: Mamdani (o output é uma função fuzzy) e Sugeno (o output é matemático). No ALE ACS, o output do tipo Sugeno será estendido para suportar Linearizadores Piecewise (0 a N pontos).

6.1 - Arquitetura e Organização
As classes residirão no projeto dentro da pasta e .ALE.ACS.EngineFuzzy/Inference/Fuzzy/Defuzzification/

Fuzzy/Inference/InferenceEngineType.cs (Enum: Mamdani, Sugeno)
Fuzzy/Inference/IFuzzyInferenceEngine.cs (Interface)
Fuzzy/Inference/MamdaniInferenceEngine.cs
Fuzzy/Inference/SugenoInferenceEngine.cs
Fuzzy/Inference/SugenoOutputType.cs (Enum: Constante, Linear, Linear por Partes)
Fuzzy/Inference/SugenoOutputFunction.cs (Modelagem da função de saída)
6.2 - Definição de SugenoOutputFunction (O Linearizador)
No motor Sugeno, cada do output não tem uma , mas sim uma função matemática.FuzzyTermIMembershipFunction

Constant: Retorna um valor fixo (ex: 50.0).
Linear: Retorna .a*x + b
PiecewiseLinear (Linearizador): Retorna um valor baseado em uma lista de pontos (X, Y). Se a entrada for 0 a N pontos, o motor deve fazer a interpolação linear entre eles.
A classe deve conter:SugenoOutputFunction

O tipo da função ().SugenoOutputType
Um valor constante (se para Constant).
Coeficientes e (se de Linear).AB
Uma lista de (se for PiecewiseLinear), contendo de 0 a N pontos.Point
6.3 - Definição da Interface do Motor
public interface IFuzzyInferenceEngine{    InferenceEngineType Type { get; }        /// <summary>    /// Avalia as regras fuzzy e retorna o valor crisp de saída.    /// </summary>    /// <param name="inputs">Dicionário onde a chave é o nome da Variável e o valor é a entrada Crisp.</param>    /// <param name="rules">A base de regras.</param>    /// <returns>Resultado numérico da inferência.</returns>    double Evaluate(Dictionary<string, double> inputs, IEnumerable<FuzzyRule> rules);}
6.4 - Motores Mamdani e Sugeno
'MamdaniInMamdaniInferenceEngine: Avalia as regras combinando as pertinências (Min para AND, Max para OR). Saída O
SugenoInferenceEngine: Avalia as regras, calcula o grau de ativação da regra (Min para AND), aplica a função do consequente (Constant, Linear ou PiecewiseLinear) e faz a média ponderada: 'Sum(wi * zi) / SumSum(wi * zi) / Sum(wi).
6.5 - REGRAS DE IMPLEMENTAÇÃO
NÃO

implementar uma defuzzificação
usar dynamic ou reflection
pular validações de pontos nulos no linearizador
SEMPRE

garantir que o linearizador ('PedaçosLinearPiecewiseLinear) faça interpolação linear correta entre os pontos informados
garantir que a média ponderada do Sugeno trate divisão por zero (se a soma dos pesos for 0, retorna 0)
implementar testes xUnit para:
Criar motor Sugeno com output Constante e validar resultado
Criar motor Sugeno com saída Linear e
Criar motor Sugeno com saída PiecewiseLinear
colocar os testes em 'tests/ALE. Atests/ALE.ACS.Engine.Tests/Fuzzy/Inference/


07 CAPÍTULO 5 - DEFUZZIFICAÇÃO
Neste capítulo, implementamos os métodos para converter um conjunto fuzzy de saída (gerado pelo motor Mamdani) em um valor crisp numérico.

7.1 - Arquitetura e Organização
As classes residirão no projeto dentro da pasta .ALE.ACS.EngineFuzzy/Defuzzification/

Fuzzy/Defuzzification/DefuzzificationMethod.cs (Enum: Centroide, Bissetor, MédiaDeMáximo, MáximoEsquerda, MáximoDireito)
Fuzzy/Defuzzification/IDefuzzifier.cs (Interface)
Fuzzy/Defuzzification/CentroidDefuzzifier.cs
Fuzzy/Defuzzification/MeanOfMaximumDefuzzifier.cs
7.2 - Definição da Interface
O defuzzifier receberá uma lista de funções de pertinência ativadas e seus respectivos pesos (grau de ativação da regra), além dos limites da variável de saída.

public interface IDefuzzifier{    DefuzzificationMethod Method { get; }        /// <summary>    /// Calcula o valor crisp a partir das funções de pertinência ativadas.    /// </summary>    /// <param name="activatedFunctions">Lista de funções e seus pesos de ativação.</param>    /// <param name="minValue">Limite inferior da variável de saída.</param>    /// <param name="maxValue">Limite superior da variável de saída.</param>    /// <returns>Valor crisp.</returns>    double Defuzzify(List<(IMembershipFunction Function, double Weight)> activatedFunctions, double minValue, double maxValue);}
7.3 - Algoritmos
CentroidDefuzzifier: Calcula o centro de massa. Deve discretizar o intervalo [minValue, maxValue] em N passos (ex: 1000 passos), aplicar o operador de máxima união (Max) entre as funções cortadas (Min(peso, Evaluate(x))), e calcular a integral numérica: .Sum(x * y) / Sum(y)
MeanOfMaximumDefuzzifier: Encontra o valor máximo de Y na união das funções cortadas e retorna a média dos valores de X que atingem esse máximo.
7.4 - REGRAS DE IMPLEMENTAÇÃO (CAPÍTULO 5)
NÃO

usar bibliotecas externas de matemática (usar apenas System.Math)
retornar NaN em caso de conjunto fuzzy vazio (retornar 0)
alterar os motores de inferência ainda, apenas criar os defuzzificadores
SEMPRE

usar System.Math
implementar o Centroid com discretização fixa de 1000 pontos
tratar divisão por zero na defuzzificação (se a área total for 0, retornar 0)
implementar testes xUnit para:
Centroid de uma única função Triangular ativada com peso 1.0 (deve retornar o centro do triângulo)
Centroid de duas funções sobrepostas
MOM de uma função Trapezoidal ativada com peso 1.0
colocar os testes em tests/ALE.ACS.Engine.Tests/Fuzzy/Defuzzification/

08 CAPÍTULO 6 - CONTROLADOR FUZZY (SYSTEM WRAPPER)
Neste capítulo, criamos a classe de fachada (Fachada) que todas as partes do motor. O será a API pública do motor fuzzy para a camada de Application.FuzzyController

8.1 - Arquitetura e Organização
A classe residirá no projeto na raiz da pasta .ALE.ACS.EngineFuzzy/

Fuzzy/FuzzyController.cs
8.2 - Definição da Classe FuzzyController
O controlador deve ser imutável em sua configuração (variáveis e regras) após a construção, garantindo thread-safety durante a execução (avaliação).

Deve conter uma lista de variáveis de entrada ().Inputs
Deve conter uma variável de saída ().Output
Deve conter a base de regras ().Rules
Deve receber no construtor a estratégia de inferência () e, se for Mamdani, o .IFuzzyInferenceEngineIDefuzzifier
Deve expor um método que retorna o valor crisp final.Evaluate(Dictionary<string, double> inputs)
8.3 - Implementação Base (Referência para o Copilot)
namespace ALE.ACS.Engine.Fuzzy;public sealed class FuzzyController{    public string Name { get; }    public IReadOnlyList<FuzzyVariable> Inputs { get; }    public FuzzyVariable Output { get; }    public IReadOnlyList<FuzzyRule> Rules { get; }    public InferenceEngineType EngineType { get; }    private readonly IFuzzyInferenceEngine _inferenceEngine;    private readonly IDefuzzifier? _defuzzifier; // Null para Sugeno    public FuzzyController(        string name,        List<FuzzyVariable> inputs,        FuzzyVariable output,        List<FuzzyRule> rules,        IFuzzyInferenceEngine inferenceEngine,        IDefuzzifier? defuzzifier = null)    {        // Validar name, inputs, output, rules, inferenceEngine        // Se inferenceEngine for Mamdani, defuzzifier não pode ser null    }    public double Evaluate(Dictionary<string, double> inputs)    {        // 1. Validar se todas as entradas foram fornecidas        // 2. Chamar _inferenceEngine.Evaluate(inputs, Rules)        // 3. Se for Sugeno, retornar o resultado direto (média ponderada)        // 4. Se for Mamdani, o motor deve retornar as funções ativadas (ou o controlador as monta)         //    e passa para o _defuzzifier.Defuzzify(...)        // Nota: Para manter a compatibilidade com a interface do Capítulo 4,         // se o motor Mamdani retornar apenas um número (grau de ativação),         // o controlador pode retornar esse número ou 0 se não houver defuzzifier complexo.        // Vamos assumir que Evaluate retorna o double final calculado.    }}
8.4 - REGRAS DE IMPLEMENTAÇÃO (CAPÍTULO 6)
NÃO

permitir modificação das listas de Inputs ou Rules após a construção
Ace
usar dynamic ou reflection
SEMPRE

validar no construtor que o defuzzifier é obrigatório se o EngineType paraMamdani
validar não Evaluate se a quantidade de inputs no dicionário corresponde à quantidade de variáveis de entrada configuradas
implementar testes xUnit para:
Criar um controlador Mamdani completo e avaliar
Criar um controlador Sugeno completo e avaliar
Falhar ao criar Mamdani sem defuzzifier
Falhar ao avaliar com inputs faltantes
colocar os testes em tests/ALE.ACS.Engine.Tests/Fuzzy/FuzzyControllerTests.cs

09 CAPÍTULO 7 - INTEGRAÇÃO NA APPLICATION
Neste capítulo, expomos o motor fuzzy para a camada de Application usando CQRS. O usuário envia uma configuração fuzzy e recebe o valor calculado.

7.1 - Arquitetura e Organização
As classes residirão no projeto dentro da pasta .ALE.ACS.ApplicationFuzzy/

Fuzzy/EvaluateFuzzyControllerCommand.cs
Fuzzy/EvaluateFuzzyControllerHandler.cs
7.2 - Definição do Command
O Command carrega a configuração do controlador em formato DTO (Data Transfer Object) para não acoplar a Application à Engine.

Deve conter as variáveis de entrada, a variável de saída, as regras e os inputs atuais (crisp).
7.3 - Definição do Handler
O Handler recebe o Command, constrói os objetos da Engine (, , , ), chama o método e retorna um .FuzzyVariableFuzzyTermFuzzyRuleFuzzyControllerEvaluateResult<double>

7.4 - REGRAS DE IMPLEMENTAÇÃO (CAPÍTULO 7)
NÃO

retornar a instância da Engine para a UI (sempre retornar DTOs ou primitivos)
usar dynamic ou reflection
pular validações de domínio no Handler
SEMPRE

usar o padrão para retornar o valor ou erroResult<T>
mapear as exceções da Engine para no HandlerResult.Failure
implementar testes xUnit para:
Handler retornando sucesso com um controlador Sugeno simples
Handler retornando falha se as regras estiverem vazias
colocar os testes em (se o projeto de testes de Application existir, senão colocar no projeto de testes da Engine)tests/ALE.ACS.Application.Tests/Fuzzy/