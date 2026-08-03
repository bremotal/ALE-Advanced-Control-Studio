using ALE.ACS.Engine.Core;

namespace ALE.ACS.Engine.Abstractions;

public interface IController
{
    ControllerResult Execute(ControllerInput input);
}