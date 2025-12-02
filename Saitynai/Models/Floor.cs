using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
namespace Saitynai.Models;

public partial class Floor
{
    public int Id { get; set; }

    public int BuildingId { get; set; }

    public int FloorNumber { get; set; }

    public string FloorPlanPath { get; set; } = null!;

    [JsonIgnore]
    public virtual Building? Building { get; set; } = null!;

    [JsonIgnore]
    public virtual ICollection<Point> Points { get; set; } = new List<Point>();
}
