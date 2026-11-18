using ALE.ACS.Engine.Optimization;

namespace ALE.ACS.Engine.AdaptiveMpc;

/// <summary>
/// Recursive Least Squares com fator de esquecimento (forgetting factor).
/// Estima parâmetros θ de um modelo linear y(k) = φ(k)' θ, onde φ é o regressor.
/// </summary>
/// <remarks>
/// Para Adaptive MPC: identifica os parâmetros de um modelo ARX discreto
///   y(k) = -a1 y(k-1) - ... - ana y(k-na) + b1 u(k-1) + ... + bnb u(k-nb)
/// O regressor é φ = [-y(k-1), ..., -y(k-na), u(k-1), ..., u(k-nb)].
/// O RLS atualiza a estimativa a cada novo par (u, y) observado.
/// Quando o modelo muda (mudança de ponto de operação, envelhecimento), o
/// AdaptiveMpcController re-desenha o QP.
/// </remarks>
public sealed class RecursiveLeastSquares
{
    private readonly double _lambda;  // forgetting factor (0.95 - 0.999 típico)
    private double[] _theta;          // parâmetros estimados
    private Matrix _P;                // matriz de covariância
    private readonly int _order;

    /// <summary>Cria um RLS para estimar <paramref name="order"/> parâmetros.</summary>
    public RecursiveLeastSquares(int order, double lambda = 0.99, double initialCovariance = 1e3)
    {
        if (order <= 0) throw new ArgumentException("Ordem deve ser positiva.", nameof(order));
        if (lambda <= 0 || lambda > 1.0) throw new ArgumentException("λ deve estar em (0, 1].", nameof(lambda));

        _order = order;
        _lambda = lambda;
        _theta = new double[order];
        _P = Matrix.Identity(order).Scale(initialCovariance);
    }

    public double[] Theta => _theta;
    public int Order => _order;
    public double Lambda => _lambda;

    /// <summary>Atualiza a estimativa com um novo par (regressor, saída observada).</summary>
    public void Update(double[] phi, double y)
    {
        if (phi.Length != _order)
            throw new ArgumentException($"Regressor deve ter {_order} elementos.", nameof(phi));

        // Ganho de Kalman: K = P φ / (λ + φ' P φ)
        var Pphi = _P.MultiplyVector(phi);
        double denom = _lambda;
        for (int i = 0; i < _order; i++) denom += phi[i] * Pphi[i];
        if (Math.Abs(denom) < 1e-15) denom = 1e-15;

        var K = new double[_order];
        for (int i = 0; i < _order; i++) K[i] = Pphi[i] / denom;

        // Erro de predição
        double yhat = 0;
        for (int i = 0; i < _order; i++) yhat += phi[i] * _theta[i];
        double e = y - yhat;

        // Atualiza θ
        for (int i = 0; i < _order; i++) _theta[i] += K[i] * e;

        // Atualiza P = (P - K φ' P) / λ
        // Implementação simplificada: P_new = (P - K (φ' P)) / λ
        // Como K = P φ / denom, fica: P_new = (1/λ) * (P - P φ φ' P / denom)
        for (int i = 0; i < _order; i++)
        {
            for (int j = 0; j < _order; j++)
            {
                _P[i, j] = (_P[i, j] - Pphi[i] * Pphi[j] / denom) / _lambda;
            }
        }
    }

    /// <summary>Reseta o estimador para uso a partir de um novo conjunto.</summary>
    public void Reset(double initialCovariance = 1e3)
    {
        _theta = new double[_order];
        _P = Matrix.Identity(_order).Scale(initialCovariance);
    }

    /// <summary>Snapshot do estado (para serialização/hot-swap).</summary>
    public RlsState CaptureState() => new((double[])_theta.Clone(), _P);

    /// <summary>Restaura estado capturado.</summary>
    public void RestoreState(RlsState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        _theta = (double[])state.Theta.Clone();
        _P = state.P;
    }
}

public sealed record RlsState(double[] Theta, Matrix P);
