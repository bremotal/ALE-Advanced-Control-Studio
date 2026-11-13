using ALE.ACS.Engine.Core;

namespace ALE.ACS.Engine.Abstractions;

/// <summary>
/// Gera código PLC a partir de uma <see cref="IControllerSpec"/> (não de uma instância runtime).
/// Cada engine registra seu próprio gerador; a <see cref="PlcCodeGeneratorRegistry"/>
/// despacha por <see cref="IControllerSpec.Kind"/>.
/// </summary>
public interface IPlcCodeGenerator
{
    /// <summary>Tipo de controlador que este gerador sabe traduzir.</summary>
    ControllerKind Supports { get; }

    /// <summary>Gera o código PLC a partir do pedido.</summary>
    PlcGenerationResult Generate(PlcGenerationRequest request);
}
