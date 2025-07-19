namespace CompanyDirectory.API.Infrastructure.Orleans.Types;

public interface IEvent
{
    /// <summary>
    /// When the event was recorded in the system
    /// </summary>
    DateTime EventTimestamp { get; init; }

    /// <summary>
    /// The actual time of the event
    /// </summary>
    DateTime ActualTimestamp { get; init; }
}