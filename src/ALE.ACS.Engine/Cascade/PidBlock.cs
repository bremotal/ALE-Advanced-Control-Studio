using ALE.ACS.Engine.ProcessVariables;

namespace ALE.ACS.Engine.Cascade;

/// <summary>
/// Bloco PID ISA standard com anti-windup por tracking e derivação sobre PV.
/// Implementa a forma paralela com ganhos independentes (Kp, Ti, Td)
/// mas computa na forma dependente (P+I+D), padrão industrial.
/// </summary>
/// <remarks>
/// Lei de controle (por scan, com Ts):
///   e(t)  := SP - PV
///   P     := Kp * e
///   I    := I_prev + Kp * Ts / Ti * e            (integração com clamping anti-windup)
///   D     := Kp * Td / (Td/N + Ts) * (D_prev - (PV - PV_prev))   // filtro do derivativo
///   u_raw := P + I + D
///   u     := clamp(u_raw, OutMin, OutMax)
///   se u != u_raw: I := I - (u_raw - u)          // back-calculation anti-windup
/// </remarks>
public sealed class PidBlock
{
    public string Name { get; }
    public double Kp { get; set; }
    public double Ti { get; set; }  // segundos; Ti = 0 desliga a integral
    public double Td { get; set; }  // segundos; Td = 0 desliga a derivativa
    public double N { get; set; } = 8.0;  // coeficiente de filtro do derivativo (5-10 típico)
    public double OutMin { get; set; } = 0.0;
    public double OutMax { get; set; } = 100.0;
    public bool DirectAction { get; set; } = false;  // true: u aumenta quando PV aumenta (ex.: refrigeração)

    // Estado interno
    private double _integral = 0.0;
    private double _derivState = 0.0;
    private double _pvPrev = double.NaN;
    private bool _initialized = false;

    public PidBlock(string name, double kp, double ti, double td)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
        Kp = kp;
        Ti = ti;
        Td = td;
    }

    /// <summary>Zera o estado interno (integral + derivativo).</summary>
    public void Reset()
    {
        _integral = 0.0;
        _derivState = 0.0;
        _pvPrev = double.NaN;
        _initialized = false;
    }

    /// <summary>Configura a condição inicial para bumpless transfer de manual -> auto.</summary>
    public void BumplessTransfer(double currentOutput, double currentPv, double sp, double scanTimeSec)
    {
        // Para bumpless: a integral deve resultar em currentOutput quando e = SP - PV
        // => I = currentOutput - Kp * (SP - PV) - D
        var e = DirectAction ? (currentPv - sp) : (sp - currentPv);
        _integral = currentOutput - Kp * e;
        _pvPrev = currentPv;
        _initialized = true;
    }

    /// <summary>Executa um passo do PID. Retorna a saída saturada.</summary>
    public double Step(double sp, double pv, double scanTimeSec)
    {
        if (scanTimeSec <= 0)
            throw new ArgumentException("ScanTime deve ser positivo.", nameof(scanTimeSec));

        // Erro (com convenção de ação)
        var e = DirectAction ? (pv - sp) : (sp - pv);

        // Termo proporcional
        var p = Kp * e;

        // Termo integral (com clamping se Ti = 0)
        double i = _integral;
        if (Ti > 1e-9)
        {
            i = _integral + Kp * scanTimeSec / Ti * e;
        }

        // Termo derivativo (sobre PV, não sobre erro — evita kick de SP)
        double d = 0.0;
        if (Td > 1e-9)
        {
            if (!_initialized)
            {
                _pvPrev = pv;
                _initialized = true;
            }
            var dPv = pv - _pvPrev;
            // Filtro passa-baixa de primeiro ordem no termo derivativo
            var alpha = Td / N;
            var coeff = Td / (alpha + scanTimeSec);
            d = coeff * (-dPv) / scanTimeSec;  // -dPv porque derivamos -PV (para ação reversa padrão)
            // Suavização do estado do filtro
            _derivState = (_derivState + coeff * (-dPv)) * (alpha / (alpha + scanTimeSec));
            d = _derivState;
        }
        else if (!_initialized)
        {
            _pvPrev = pv;
            _initialized = true;
        }

        // Saída bruta
        var uRaw = p + i + d;

        // Saturação
        var u = Math.Max(OutMin, Math.Min(OutMax, uRaw));

        // Anti-windup por back-calculation
        if (u != uRaw && Ti > 1e-9)
        {
            i = i - (uRaw - u);
        }

        _integral = i;
        _pvPrev = pv;

        return u;
    }

    /// <summary>Snapshot do estado interno (para serialização/hot-swap).</summary>
    public PidState CaptureState() => new(_integral, _derivState, _pvPrev, _initialized);

    /// <summary>Restaura estado previamente capturado.</summary>
    public void RestoreState(PidState state)
    {
        _integral = state.Integral;
        _derivState = state.DerivState;
        _pvPrev = state.PvPrev;
        _initialized = state.Initialized;
    }
}

/// <summary>Estado interno serializável de um <see cref="PidBlock"/>.</summary>
public sealed record PidState(double Integral, double DerivState, double PvPrev, bool Initialized);
