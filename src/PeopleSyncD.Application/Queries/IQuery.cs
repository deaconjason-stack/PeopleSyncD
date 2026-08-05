namespace PeopleSyncD.Application.Queries;

/// <summary>
/// Marker contract for read-only queries.
/// </summary>
/// <typeparam name="TResponse">Query response type.</typeparam>
public interface IQuery<out TResponse>;
