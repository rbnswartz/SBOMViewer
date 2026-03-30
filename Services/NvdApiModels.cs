using System.Text.Json.Serialization;

namespace SBOMViewer.Services;

public class NvdApiResponse
{
    [JsonPropertyName("resultsPerPage")]
    public int ResultsPerPage { get; set; }

    [JsonPropertyName("startIndex")]
    public int StartIndex { get; set; }

    [JsonPropertyName("totalResults")]
    public int TotalResults { get; set; }

    [JsonPropertyName("vulnerabilities")]
    public List<NvdVulnerabilityItem> Vulnerabilities { get; set; } = new();
}

public class NvdVulnerabilityItem
{
    [JsonPropertyName("cve")]
    public NvdCve Cve { get; set; } = new();
}

public class NvdCve
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("published")]
    public DateTime? Published { get; set; }

    [JsonPropertyName("lastModified")]
    public DateTime? LastModified { get; set; }

    [JsonPropertyName("descriptions")]
    public List<NvdDescription> Descriptions { get; set; } = new();

    [JsonPropertyName("metrics")]
    public NvdMetrics? Metrics { get; set; }
}

public class NvdDescription
{
    [JsonPropertyName("lang")]
    public string Lang { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;
}

public class NvdMetrics
{
    [JsonPropertyName("cvssMetricV40")]
    public List<NvdCvssMetric>? CvssMetricV40 { get; set; }

    [JsonPropertyName("cvssMetricV31")]
    public List<NvdCvssMetric>? CvssMetricV31 { get; set; }

    [JsonPropertyName("cvssMetricV30")]
    public List<NvdCvssMetric>? CvssMetricV30 { get; set; }

    [JsonPropertyName("cvssMetricV2")]
    public List<NvdCvssMetricV2>? CvssMetricV2 { get; set; }
}

public class NvdCvssMetric
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("cvssData")]
    public NvdCvssData CvssData { get; set; } = new();
}

public class NvdCvssMetricV2
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("cvssData")]
    public NvdCvssDataV2 CvssData { get; set; } = new();

    [JsonPropertyName("baseSeverity")]
    public string? BaseSeverity { get; set; }
}

public class NvdCvssData
{
    [JsonPropertyName("baseScore")]
    public decimal BaseScore { get; set; }

    [JsonPropertyName("baseSeverity")]
    public string? BaseSeverity { get; set; }

    [JsonPropertyName("vectorString")]
    public string? VectorString { get; set; }
}

public class NvdCvssDataV2
{
    [JsonPropertyName("baseScore")]
    public decimal BaseScore { get; set; }

    [JsonPropertyName("vectorString")]
    public string? VectorString { get; set; }
}
