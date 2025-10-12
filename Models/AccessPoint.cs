using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text.Json.Serialization;
namespace Saitynai.Models;

public partial class AccessPoint
{
    public int Id { get; set; }

    public int ScanId { get; set; }

    public string? Ssid { get; set; }

    public string Bssid { get; set; } = null!;

    public string? Capabilities { get; set; }

    public int? Centerfreq0 { get; set; }

    public int? Centerfreq1 { get; set; }

    public int? Frequency { get; set; }

    public short Level { get; set; }
    
    [JsonIgnore]
    public virtual Scan? Scan { get; set; } = null!;
}


public sealed class AccessPointCreateDto
{
    public int ScanId { get; set; }
    public string? Ssid { get; set; }
    public string Bssid { get; set; } = null!;
    public string? Capabilities { get; set; }
    public int? Centerfreq0 { get; set; }
    public int? Centerfreq1 { get; set; }
    public int? Frequency { get; set; }
    public short Level { get; set; }
}
