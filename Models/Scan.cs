using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
namespace Saitynai.Models;

public partial class Scan
{
    public int Id { get; set; }

    public int PointId { get; set; }

    public DateTime ScannedAt { get; set; } = DateTime.UtcNow;

    public string? Filters { get; set; }

    public int ApCount { get; set; }

    [JsonIgnore]
    public virtual ICollection<AccessPoint> AccessPoints { get; set; } = new List<AccessPoint>();
    [JsonIgnore]
    public virtual Point? Point { get; set; } = null!;
}
