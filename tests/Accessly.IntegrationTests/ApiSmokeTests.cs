using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace Accessly.IntegrationTests;

public class ApiSmokeTests(AccesslyApiFactory factory) : IClassFixture<AccesslyApiFactory>
{
    private sealed record LoginResult(string AccessToken);
    private sealed record IdResult(Guid Id);
    private sealed record EventsPage(int TotalCount);
    private sealed record BookingResult(Guid BookingId, string TicketCode, string Status);
    private sealed record CheckInResult(bool Accepted, string Result);

    [Fact]
    public async Task Health_endpoint_reports_healthy()
    {
        var client = factory.CreateClient();
        var response = await client.GetAsync("/api/health");
        response.EnsureSuccessStatusCode();
        (await response.Content.ReadAsStringAsync()).Should().Contain("Healthy");
    }

    [Fact]
    public async Task Catalog_returns_seeded_published_events()
    {
        var client = factory.CreateClient();
        var page = await client.GetFromJsonAsync<EventsPage>("/api/events");
        page!.TotalCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Full_flow_create_publish_book_and_check_in()
    {
        var client = factory.CreateClient();

        SetBearer(client, await LoginAsync(client, "organizer@accessly.local"));
        var create = await client.PostAsJsonAsync("/api/events", new
        {
            title = "Integration Test Event",
            description = "Created by an integration test.",
            startAt = DateTimeOffset.UtcNow.AddDays(5),
            endAt = DateTimeOffset.UtcNow.AddDays(5).AddHours(2),
            venueName = "Test Hall",
            capacity = 10,
            priceAmount = 0,
            currency = "EUR",
        });
        create.EnsureSuccessStatusCode();
        var eventId = (await create.Content.ReadFromJsonAsync<IdResult>())!.Id;

        (await client.PostAsync($"/api/events/{eventId}/publish", null)).EnsureSuccessStatusCode();

        SetBearer(client, await LoginAsync(client, "attendee@accessly.local"));
        var booking = await client.PostAsync($"/api/events/{eventId}/bookings", null);
        booking.EnsureSuccessStatusCode();
        var bookingResult = await booking.Content.ReadFromJsonAsync<BookingResult>();
        bookingResult!.Status.Should().Be("Confirmed");
        bookingResult.TicketCode.Should().NotBeNullOrEmpty();

        SetBearer(client, await LoginAsync(client, "staff@accessly.local"));
        var checkIn = await client.PostAsJsonAsync("/api/check-ins", new { code = bookingResult.TicketCode, eventId });
        checkIn.EnsureSuccessStatusCode();
        var checkInResult = await checkIn.Content.ReadFromJsonAsync<CheckInResult>();
        checkInResult!.Accepted.Should().BeTrue();
        checkInResult.Result.Should().Be("Accepted");
    }

    private static async Task<string> LoginAsync(HttpClient client, string email)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new { email, password = "Password123!" });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<LoginResult>())!.AccessToken;
    }

    private static void SetBearer(HttpClient client, string token)
        => client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
}
