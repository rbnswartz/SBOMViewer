using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SBOMViewer.Data;

namespace SBOMViewer.Services;

public class NvdService
{
    private const string NvdApiBaseUrl = "https://services.nvd.nist.gov/rest/json/cves/2.0";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<NvdService> _logger;
    private readonly IConfiguration _configuration;

    public NvdService(
        IHttpClientFactory httpClientFactory,
        IServiceScopeFactory scopeFactory,
        ILogger<NvdService> logger,
        IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _scopeFactory = scopeFactory;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Fetches vulnerabilities from NVD for all tracked dependencies and stores them in the database.
    /// </summary>
    public async Task UpdateVulnerabilitiesAsync(CancellationToken cancellationToken = default)
    {
        var apiKey = _configuration["NvdApi:ApiKey"];
        var delayMs = _configuration.GetValue<int>("NvdApi:SearchDelayMs", 6000);

        List<(int Id, string Name, string? Ecosystem)> dependencyInfos;

        using (var scope = _scopeFactory.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dependencyInfos = await dbContext.Dependencies
                .Where(d => !string.IsNullOrEmpty(d.Name))
                .Select(d => new { d.Id, d.Name, d.Ecosystem })
                .ToListAsync(cancellationToken)
                .ContinueWith(t => t.Result.Select(x => (x.Id, x.Name, x.Ecosystem)).ToList(), cancellationToken);
        }

        _logger.LogInformation("Starting NVD vulnerability update for {Count} dependencies", dependencyInfos.Count);

        foreach (var dep in dependencyInfos)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            try
            {
                await ProcessDependencyAsync(dep.Id, dep.Name, dep.Ecosystem, apiKey, cancellationToken);
                await Task.Delay(delayMs, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching NVD vulnerabilities for dependency '{Name}'", dep.Name);
            }
        }

        _logger.LogInformation("NVD vulnerability update completed");
    }

    private async Task ProcessDependencyAsync(
        int dependencyId,
        string packageName,
        string? ecosystem,
        string? apiKey,
        CancellationToken cancellationToken)
    {
        var keyword = BuildSearchKeyword(packageName, ecosystem);
        var cves = await SearchNvdAsync(keyword, apiKey, cancellationToken);

        if (cves.Count == 0)
            return;

        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        foreach (var cve in cves)
        {
            var englishDesc = cve.Descriptions.FirstOrDefault(d => d.Lang == "en")?.Value;

            // Determine best available CVSS score and severity
            GetBestCvssInfo(cve.Metrics, out var score, out var severity, out var vector);

            // Upsert the vulnerability record
            var vuln = await dbContext.Vulnerabilities
                .FirstOrDefaultAsync(v => v.CveId == cve.Id, cancellationToken);

            if (vuln == null)
            {
                vuln = new Vulnerability { CveId = cve.Id };
                dbContext.Vulnerabilities.Add(vuln);
            }

            vuln.Description = englishDesc;
            vuln.Severity = severity;
            vuln.CvssScore = score;
            vuln.CvssVector = vector;
            vuln.PublishedDate = cve.Published;
            vuln.LastModifiedDate = cve.LastModified;
            vuln.LastFetchedDate = DateTime.UtcNow;

            await dbContext.SaveChangesAsync(cancellationToken);

            // Link the vulnerability to this dependency (if not already linked)
            var alreadyLinked = await dbContext.DependencyVulnerabilities
                .AnyAsync(dv => dv.DependencyId == dependencyId && dv.VulnerabilityId == vuln.Id, cancellationToken);

            if (!alreadyLinked)
            {
                dbContext.DependencyVulnerabilities.Add(new DependencyVulnerability
                {
                    DependencyId = dependencyId,
                    VulnerabilityId = vuln.Id
                });
                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        _logger.LogDebug("Processed {Count} CVEs for dependency '{Name}'", cves.Count, packageName);
    }

    private static string BuildSearchKeyword(string packageName, string? ecosystem)
    {
        // Combine name with ecosystem for a more targeted search
        if (!string.IsNullOrEmpty(ecosystem))
            return $"{packageName} {ecosystem}";
        return packageName;
    }

    private async Task<List<NvdCve>> SearchNvdAsync(string keyword, string? apiKey, CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient("NVD");

        var url = $"{NvdApiBaseUrl}?keywordSearch={Uri.EscapeDataString(keyword)}&resultsPerPage=100";

        var request = new HttpRequestMessage(HttpMethod.Get, url);
        if (!string.IsNullOrEmpty(apiKey))
            request.Headers.Add("apiKey", apiKey);

        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        HttpResponseMessage response;
        try
        {
            response = await client.SendAsync(request, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "HTTP error contacting NVD API for keyword '{Keyword}'", keyword);
            return new List<NvdCve>();
        }

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("NVD API returned {StatusCode} for keyword '{Keyword}'", response.StatusCode, keyword);
            return new List<NvdCve>();
        }

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var apiResponse = JsonSerializer.Deserialize<NvdApiResponse>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return apiResponse?.Vulnerabilities.Select(v => v.Cve).ToList() ?? new List<NvdCve>();
    }

    private static void GetBestCvssInfo(
        NvdMetrics? metrics,
        out decimal? score,
        out string? severity,
        out string? vector)
    {
        score = null;
        severity = null;
        vector = null;

        if (metrics == null)
            return;

        // Prefer CVSSv4, then v3.1, then v3.0, then v2
        var v4Primary = metrics.CvssMetricV40?.FirstOrDefault(m => m.Type == "Primary")
                     ?? metrics.CvssMetricV40?.FirstOrDefault();
        if (v4Primary != null)
        {
            score = v4Primary.CvssData.BaseScore;
            severity = v4Primary.CvssData.BaseSeverity;
            vector = v4Primary.CvssData.VectorString;
            return;
        }

        var v31Primary = metrics.CvssMetricV31?.FirstOrDefault(m => m.Type == "Primary")
                      ?? metrics.CvssMetricV31?.FirstOrDefault();
        if (v31Primary != null)
        {
            score = v31Primary.CvssData.BaseScore;
            severity = v31Primary.CvssData.BaseSeverity;
            vector = v31Primary.CvssData.VectorString;
            return;
        }

        var v30Primary = metrics.CvssMetricV30?.FirstOrDefault(m => m.Type == "Primary")
                      ?? metrics.CvssMetricV30?.FirstOrDefault();
        if (v30Primary != null)
        {
            score = v30Primary.CvssData.BaseScore;
            severity = v30Primary.CvssData.BaseSeverity;
            vector = v30Primary.CvssData.VectorString;
            return;
        }

        var v2Primary = metrics.CvssMetricV2?.FirstOrDefault(m => m.Type == "Primary")
                     ?? metrics.CvssMetricV2?.FirstOrDefault();
        if (v2Primary != null)
        {
            score = v2Primary.CvssData.BaseScore;
            severity = v2Primary.BaseSeverity ?? ScoreToSeverityV2(v2Primary.CvssData.BaseScore);
            vector = v2Primary.CvssData.VectorString;
        }
    }

    private static string ScoreToSeverityV2(decimal score) => score switch
    {
        >= 7.0m => "HIGH",
        >= 4.0m => "MEDIUM",
        _ => "LOW"
    };
}
