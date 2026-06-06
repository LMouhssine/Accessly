namespace Accessly.Domain.Enums;

public enum CheckInResult
{
    Accepted = 0,
    AlreadyCheckedIn = 1,
    InvalidTicket = 2,
    TicketCancelled = 3,
    EventMismatch = 4
}
