namespace PowerQuality.Application.DTOs;
public sealed record SiteDto(Guid Id, string Name, string? Location, DateTime CreatedAtUtc);
public sealed record CreateSiteRequest(string Name, string? Location);
