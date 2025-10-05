using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
namespace Saitynai.Models;

public partial class Point
{
    public int Id { get; set; }

    public int FloorId { get; set; }

    public decimal Latitude { get; set; }

    public decimal Longitude { get; set; }

    public int ApCount { get; set; }
    [JsonIgnore]
    public virtual Floor? Floor { get; set; } = null!;

    [JsonIgnore]
    public virtual ICollection<Scan> Scans { get; set; } = new List<Scan>();
}
