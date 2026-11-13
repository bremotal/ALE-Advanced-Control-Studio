namespace ALE.ACS.Engine.Abstractions;

/// <summary>
/// Resultado do design offline de um controlador. Parâmetros pré-computados
/// que serão usados pelo PLC em runtime.
/// </summary>
/// <remarks>
/// Marker interface — cada engine tem sua própria implementação concreta.
/// Por exemplo, <c>MpcDesign</c> carrega a matriz de ganho K pré-resolvida,
/// <c>CascadeDesign</c> carrega os ganhos master/slave ajustados.
/// </remarks>
public interface IControllerDesign
{
    /// <summary>Indica se o design está completo e válido para implantação.</summary>
    bool IsValid { get; }

    /// <summary>Resumo legível dos parâmetros computados.</summary>
    string Summary { get; }
}
