namespace ALE.ACS.Engine.Abstractions;

public interface IControllerFactory
{
    IController Create(string controllerType);

    bool CanCreate(string controllerType);
}