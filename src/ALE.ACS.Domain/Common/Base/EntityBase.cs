namespace ALE.ACS.Domain.Common.Base;

/// <summary>
/// Representa a classe base para todas as entidades do domínio do
/// ALE Advanced Control Studio.
/// </summary>
public abstract class EntityBase
{
    /// <summary>
    /// Inicializa uma nova instância da entidade.
    /// </summary>
    protected EntityBase()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }

    #region Identity

    /// <summary>
    /// Identificador único da entidade.
    /// </summary>
    public Guid Id { get; }

    #endregion

    #region Properties

    /// <summary>
    /// Nome técnico da entidade.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Nome amigável exibido ao usuário.
    /// </summary>
    public string DisplayName { get; private set; } = string.Empty;

    /// <summary>
    /// Descrição da entidade.
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    #endregion

    #region Audit

    /// <summary>
    /// Data de criação da entidade (UTC).
    /// </summary>
    public DateTime CreatedAt { get; }

    /// <summary>
    /// Data da última atualização da entidade (UTC).
    /// </summary>
    public DateTime UpdatedAt { get; private set; }

    #endregion

    #region Methods

    /// <summary>
    /// Altera o nome técnico da entidade.
    /// </summary>
    /// <param name="name">Novo nome.</param>
    /// <exception cref="ArgumentException">
    /// Lançada quando o nome é nulo ou vazio.
    /// </exception>
    public virtual void Rename(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name.Trim();

        Touch();
    }

    /// <summary>
    /// Define o nome de exibição.
    /// </summary>
    /// <param name="displayName">Nome amigável.</param>
    public virtual void SetDisplayName(string displayName)
    {
        DisplayName = displayName?.Trim() ?? string.Empty;

        Touch();
    }

    /// <summary>
    /// Define a descrição da entidade.
    /// </summary>
    /// <param name="description">Descrição.</param>
    public virtual void SetDescription(string description)
    {
        Description = description?.Trim() ?? string.Empty;

        Touch();
    }

    /// <summary>
    /// Atualiza a data da última modificação.
    /// </summary>
    protected void Touch()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    #endregion
}