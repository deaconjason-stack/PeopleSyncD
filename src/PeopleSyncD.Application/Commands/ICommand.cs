namespace PeopleSyncD.Application.Commands;

/// <summary>
/// Marker contract for commands that change system state.
/// </summary>
/// <typeparam name="TResponse">Command response type.</typeparam>
public interface ICommand<out TResponse>;
