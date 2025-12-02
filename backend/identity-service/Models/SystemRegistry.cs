namespace identity_service.Models;

public class SystemRegistry : EntityBase
{    
    public string SystemCode { get; set; } = null!;
    public string SystemName { get; set; } = null!;
    public string? Description { get; set; }
    public string BaseUrl { get; set; } = null!;
    public string? IconUrl { get; set; }
    public bool IsEnabled { get; set; } = true;
    public string? Category { get; set; }
    public string? ContactEmail { get; set; }
    public bool? IsCentralAdmin { get; set; } = false;
    public DateTimeOffset? LastSync { get; set; } 
    public string? ApiKey { get; set; }

    public virtual ICollection<Menu>? Menus { get; set; }
}