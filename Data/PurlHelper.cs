namespace SBOMViewer.Data;

/// <summary>
/// Helpers for working with Package URLs (purls).
/// purl format: pkg:TYPE/[NAMESPACE/]NAME[@VERSION][?QUALIFIERS][#SUBPATH]
/// </summary>
public static class PurlHelper
{
    /// <summary>
    /// Converts a purl to the package's page on its native registry
    /// (e.g. nuget.org, npmjs.com, pypi.org).  Returns null when the
    /// ecosystem is not recognised or the purl is malformed.
    /// </summary>
    public static string? GetRegistryUrl(string? purl)
    {
        if (!TryParse(purl, out var type, out var ns, out var name))
            return null;

        return type switch
        {
            "nuget"         => $"https://www.nuget.org/packages/{name}",
            "npm"           => ns != null
                                   ? $"https://www.npmjs.com/package/@{ns}/{name}"
                                   : $"https://www.npmjs.com/package/{name}",
            "pypi"          => $"https://pypi.org/project/{name}",
            "maven"         => ns != null
                                   ? $"https://central.sonatype.com/artifact/{ns}/{name}"
                                   : null,
            "cargo"         => $"https://crates.io/crates/{name}",
            "gem"           => $"https://rubygems.org/gems/{name}",
            "composer"      => ns != null
                                   ? $"https://packagist.org/packages/{ns}/{name}"
                                   : null,
            "golang"        => ns != null
                                   ? $"https://pkg.go.dev/{ns}/{name}"
                                   : $"https://pkg.go.dev/{name}",
            "githubactions" => ns != null
                                   ? $"https://github.com/{ns}/{name}"
                                   : $"https://github.com/{name}",
            "docker"        => ns != null
                                   ? $"https://hub.docker.com/r/{ns}/{name}"
                                   : $"https://hub.docker.com/_/{name}",
            _               => null
        };
    }

    /// <summary>
    /// Returns a short human-readable registry name for a purl
    /// (e.g. "NuGet", "npm", "PyPI").  Falls back to "Registry".
    /// </summary>
    public static string GetRegistryLabel(string? purl)
    {
        if (!TryParse(purl, out var type, out _, out _))
            return "Registry";

        return type switch
        {
            "nuget"         => "NuGet",
            "npm"           => "npm",
            "pypi"          => "PyPI",
            "maven"         => "Maven Central",
            "cargo"         => "crates.io",
            "gem"           => "RubyGems",
            "composer"      => "Packagist",
            "golang"        => "pkg.go.dev",
            "githubactions" => "GitHub",
            "docker"        => "Docker Hub",
            _               => "Registry"
        };
    }

    // Parses a purl into its type, optional namespace, and name.
    // Returns false if the string is null/empty or not a valid purl.
    private static bool TryParse(string? purl, out string type, out string? ns, out string name)
    {
        type = string.Empty;
        ns   = null;
        name = string.Empty;

        if (string.IsNullOrEmpty(purl))
            return false;

        if (!purl.StartsWith("pkg:", StringComparison.OrdinalIgnoreCase))
            return false;

        var remainder = purl[4..]; // strip "pkg:"

        // Extract type (everything before the first '/')
        var typeEnd = remainder.IndexOf('/');
        if (typeEnd < 0)
            return false;

        type      = remainder[..typeEnd].ToLowerInvariant();
        remainder = remainder[(typeEnd + 1)..];

        // URL-decode the path segment before further parsing so that encoded
        // characters (e.g. %40 in a version) don't interfere with delimiter
        // detection.
        remainder = Uri.UnescapeDataString(remainder);

        // Strip qualifiers (?...) and subpath (#...)
        var qIdx = remainder.IndexOf('?');
        if (qIdx >= 0) remainder = remainder[..qIdx];

        var hIdx = remainder.IndexOf('#');
        if (hIdx >= 0) remainder = remainder[..hIdx];

        // Strip version (@...)
        var vIdx = remainder.IndexOf('@');
        if (vIdx >= 0) remainder = remainder[..vIdx];

        // Split into optional namespace and name
        var lastSlash = remainder.LastIndexOf('/');
        if (lastSlash >= 0)
        {
            ns   = remainder[..lastSlash];
            name = remainder[(lastSlash + 1)..];
        }
        else
        {
            name = remainder;
        }

        return !string.IsNullOrEmpty(name);
    }
}
