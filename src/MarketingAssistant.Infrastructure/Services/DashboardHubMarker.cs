using Microsoft.AspNetCore.SignalR;

namespace MarketingAssistant.Infrastructure.Services;

/// <summary>
/// Marker class to allow Infrastructure to reference the SignalR hub type without depending on the Api project.
/// The actual DashboardHub in Api extends this marker and is mapped in Program.cs.
/// </summary>
public class DashboardHubMarker : Hub { }
