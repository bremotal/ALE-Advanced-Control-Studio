using ALE.ACS.Engine.Core;

namespace ALE.ACS.Engine.Abstractions;

/// <summary>
/// Registry que despacha geração de código PLC por tipo de controlador.
/// </summary>
public sealed class PlcCodeGeneratorRegistry
{
    private readonly Dictionary<ControllerKind, IPlcCodeGenerator> _byKind = new();

    public PlcCodeGeneratorRegistry(IEnumerable<IPlcCodeGenerator> generators)
    {
        foreach (var g in generators)
        {
            _byKind[g.Supports] = g;
        }
    }

    /// <summary>Registra manualmente um gerador (útil para testes).</summary>
    public void Register(IPlcCodeGenerator generator)
        => _byKind[generator.Supports] = generator;

    /// <summary>Tenta obter o gerador para o tipo.</summary>
    public bool TryGet(ControllerKind kind, out IPlcCodeGenerator? generator)
        => _byKind.TryGetValue(kind, out generator);

    /// <summary>Atalho de geração com despacho automático.</summary>
    public PlcGenerationResult Generate(PlcGenerationRequest request)
    {
        if (!_byKind.TryGetValue(request.Spec.Kind, out var generator) || generator is null)
        {
            return PlcGenerationResult.Fail(
                $"Nenhum gerador PLC registrado para o tipo '{request.Spec.Kind}'.",
                request.Target);
        }

        return generator.Generate(request);
    }
}
