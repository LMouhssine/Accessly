namespace Accessly.Api;

/// <summary>Role names used in authorization attributes (match <c>UserRole</c> values).</summary>
public static class Roles
{
    public const string Admin = "Admin";
    public const string Organizer = "Organizer";
    public const string Staff = "Staff";
    public const string Attendee = "Attendee";

    public const string OrganizerOrAdmin = $"{Organizer},{Admin}";
    public const string StaffOrAbove = $"{Staff},{Organizer},{Admin}";
}
