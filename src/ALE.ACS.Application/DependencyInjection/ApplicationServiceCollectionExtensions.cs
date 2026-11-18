using ALE.ACS.Application.Controllers;
using ALE.ACS.Application.DigitalTwin;
using ALE.ACS.Application.EventBus;
using ALE.ACS.Application.Interfaces;
using ALE.ACS.Application.Models;
using ALE.ACS.Application.Projects;
using ALE.ACS.Engine.Core.Common.Events;
using Microsoft.Extensions.DependencyInjection;

namespace ALE.ACS.Application.DependencyInjection;

/// <summary>
/// Registra os serviços da camada Application.
/// </summary>
/// <remarks>
/// Integração CORE (Bloco E):
/// <list type="bullet">
///   <item><description>Registra o <see cref="EventBus"/> do Core.Common.Events como
///     singleton, tornando-o disponível para módulos que queiram programar contra
///     a interface Core (IEvent/IEventHandler&lt;T&gt;). O <see cref="IEventBus"/>
///     da Application (thread-safe, baseado em Action&lt;T&gt;) permanece como o
///     barramento primário para os serviços existentes.</description></item>
///   <item><description>Registra o <see cref="IDigitalTwinService"/> que usa a
///     <c>SimulationEngine</c> real (Bloco D) para simular controladores contra
///     modelos de processo.</description></item>
/// </list>
/// </remarks>
public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // EventBus da Application (thread-safe, baseado em Action<T>)
        services.AddSingleton<IEventBus, InMemoryEventBus>();

        // EventBus do Core.Common.Events (interface IEvent/IEventHandler<T>)
        // — disponível para módulos CORE, cumprindo CORE-008 de forma não-destrutiva.
        services.AddSingleton<ALE.ACS.Engine.Core.Common.Events.EventBus>();
        

        // Serviços de domínio (todos operam sobre o projeto ativo)
        services.AddSingleton<IProjectService, ProjectService>();
        services.AddSingleton<IControllerService, ControllerService>();
        services.AddSingleton<IModelService, ModelService>();

        // Digital Twin (Bloco E4)
        services.AddSingleton<IDigitalTwinService, DigitalTwinService>();

        return services;
    }
}
