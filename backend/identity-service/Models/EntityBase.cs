using System;

namespace identity_service.Models;

public class EntityBase
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? UserCreate { get; set; }
    public DateTimeOffset? DateCreate { get; set; }
    public string? UserUpdate { get; set; }
    public DateTimeOffset? DateUpdate { get; set; }
}
