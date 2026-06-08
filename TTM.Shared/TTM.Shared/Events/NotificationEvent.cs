using TTM.Shared.Constants;

namespace TTM.Shared.Events;

public class NotificationEvent
{
    /// <summary>
    /// If there is a user attached to the event
    /// </summary>
    public Guid? UserId { get; set; }
    public EventOrigin Origin { get; set; }
    public NotificationTarget Target { get; set; }
    public string Payload { get; set; }
}