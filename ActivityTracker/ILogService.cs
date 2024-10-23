namespace ActivityTracker;

public interface ILogService
{
    ///<summary>
    /// Logs a message to a specific destination.
    ///</summary>
    ///<param name="message">The message to log.</param>
    void LogMessage(string message);
}
