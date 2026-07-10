namespace ALE.ACS.Application.Projects.CreateProject;

/// <summary>
/// Representa a solicitação para criação de um novo projeto.
/// </summary>
/// <param name="Name">
/// Nome informado pelo usuário.
/// </param>
public sealed record CreateProjectCommand(string Name);