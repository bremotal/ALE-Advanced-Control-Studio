using ALE.ACS.Engine.Abstractions;
using ALE.ACS.Engine.Core;
using ALE.ACS.Engine.Core.Simulation;
using ALE.ACS.Engine.SmithPredictor;

namespace ALE.ACS.Application.DigitalTwin;

/// <summary>
/// Ponto de uma série temporal da simulação (tempo + valor de uma variável).
/// </summary>
public sealed class DigitalTwinSample
{
    public DigitalTwinSample(TimeSpan time, string variableName, double value)
    {
        Time = time;
        VariableName = variableName;
        Value = value;
    }

    public TimeSpan Time { get; }
    public string VariableName { get; }
    public double Value { get; }
}

/// <summary>
/// Resultado de uma execução do Digital Twin.
/// </summary>
public sealed class DigitalTwinResult
{
    public DigitalTwinResult(
        bool success,
        IReadOnlyList<DigitalTwinSample> samples,
        TimeSpan simulatedDuration,
        int stepCount,
        string? errorMessage = null)
    {
        Success = success;
        Samples = samples;
        SimulatedDuration = simulatedDuration;
        StepCount = stepCount;
        ErrorMessage = errorMessage;
    }

    public bool Success { get; }
    public IReadOnlyList<DigitalTwinSample> Samples { get; }
    public TimeSpan SimulatedDuration { get; }
    public int StepCount { get; }
    public string? ErrorMessage { get; }
}

/// <summary>
/// Serviço de Digital Twin — simula um controlador contra um modelo de processo.
/// </summary>
/// <remarks>
/// Constrói uma <see cref="SimulationEngine"/> com dois componentes:
/// <list type="number">
///   <item><description>O controlador (via factory do Engine) que lê PV/SP e produz MV.</description></item>
///   <item><description>Um modelo FOPDT do processo que lê MV e produz PV.</description></item>
/// </list>
/// Executa por N passos e retorna a série temporal de MV e PV para visualização.
/// </remarks>
public interface IDigitalTwinService
{
    /// <summary>Simula um controlador PID contra um processo FOPDT.</summary>
    /// <param name="pidSpec">Spec do PID (Kp, Ti, Td, etc.).</param>
    /// <param name="processModel">Modelo FOPDT do processo (K, τ, θ, Ts).</param>
    /// <param name="setpoint">Setpoint desejado.</param>
    /// <param name="initialPv">Valor inicial da PV.</param>
    /// <param name="scanCount">Número de scans a simular.</param>
    /// <returns>Série temporal de MV e PV.</returns>
    DigitalTwinResult SimulatePid(
        object pidSpec,
        object processModel,
        double setpoint,
        double initialPv,
        int scanCount);
}
