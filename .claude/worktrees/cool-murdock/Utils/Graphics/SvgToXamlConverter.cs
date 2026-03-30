
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace MyriaRPG.Utils.Graphics
{
    /// <summary>
    /// Extended SVG to XAML converter with gradient support and utility methods
    /// </summary>
    public partial class SvgToXamlConverter : SvgXamlConverter
    {
        /// <summary>
        /// Represents a parsed SVG gradient
        /// </summary>
        public class GradientDefinition
        {
            public string Id { get; set; }
            public string Type { get; set; } // "linear" or "radial"
            public List<(double Offset, string Color)> Stops { get; set; } = new();
            public double X1 { get; set; }
            public double Y1 { get; set; }
            public double X2 { get; set; }
            public double Y2 { get; set; }
            public double CX { get; set; }
            public double CY { get; set; }
            public double R { get; set; }
        }

        private Dictionary<string, GradientDefinition> _parsedGradients = new();

        /// <summary>
        /// Advanced color converter supporting gradients via url(#id) references
        /// </summary>
        private string ConvertAdvancedColor(XElement element, string colorAttribute, bool isFill = true)
        {
            string colorValue = element.Attribute(colorAttribute)?.Value ?? "";
            
            if (colorValue.StartsWith("url(#"))
            {
                string gradientId = colorValue.Substring(5, colorValue.Length - 6);
                if (_parsedGradients.TryGetValue(gradientId, out var gradient))
                {
                    return GenerateGradientXaml(gradient);
                }
            }

            return ConvertColorToWpf(colorValue);
        }

        /// <summary>
        /// Generates XAML brush code for a gradient
        /// </summary>
        private string GenerateGradientXaml(GradientDefinition gradient)
        {
            if (gradient.Type == "linear")
            {
                return $@"
                <LinearGradientBrush StartPoint=""{gradient.X1},{gradient.Y1}"" EndPoint=""{gradient.X2},{gradient.Y2}"">
                    {string.Join(Environment.NewLine, gradient.Stops.Select(s => 
                        $@"<GradientStop Offset=""{s.Offset.ToString(CultureInfo.InvariantCulture)}"" Color=""{s.Color}""/>"))}
                </LinearGradientBrush>";
            }
            else if (gradient.Type == "radial")
            {
                return $@"
                <RadialGradientBrush Center=""{gradient.CX},{gradient.CY}"" RadiusX=""{gradient.R}"" RadiusY=""{gradient.R}"">
                    {string.Join(Environment.NewLine, gradient.Stops.Select(s => 
                        $@"<GradientStop Offset=""{s.Offset.ToString(CultureInfo.InvariantCulture)}"" Color=""{s.Color}""/>"))}
                </RadialGradientBrush>";
            }

            return "#FF000000";
        }

        /// <summary>
        /// Parses SVG gradient definitions
        /// </summary>
        public void ParseGradientsFromSvg(XDocument svgDoc)
        {
            XElement defs = svgDoc.Root?.Elements()
                .FirstOrDefault(e => e.Name.LocalName == "defs");

            if (defs == null)
                return;

            // Parse linear gradients
            foreach (XElement linearGrad in defs.Elements().Where(e => e.Name.LocalName == "linearGradient"))
            {
                var gradient = new GradientDefinition
                {
                    Id = linearGrad.Attribute("id")?.Value ?? "",
                    Type = "linear",
                    X1 = GetDoubleAttribute(linearGrad, "x1", 0),
                    Y1 = GetDoubleAttribute(linearGrad, "y1", 0),
                    X2 = GetDoubleAttribute(linearGrad, "x2", 1),
                    Y2 = GetDoubleAttribute(linearGrad, "y2", 0)
                };

                foreach (XElement stop in linearGrad.Elements().Where(e => e.Name.LocalName == "stop"))
                {
                    double offset = GetDoubleAttribute(stop, "offset", 0);
                    string color = stop.Attribute("stop-color")?.Value ?? "black";
                    gradient.Stops.Add((offset, ConvertColorToWpf(color)));
                }

                if (!string.IsNullOrEmpty(gradient.Id))
                    _parsedGradients[gradient.Id] = gradient;
            }

            // Parse radial gradients
            foreach (XElement radialGrad in defs.Elements().Where(e => e.Name.LocalName == "radialGradient"))
            {
                var gradient = new GradientDefinition
                {
                    Id = radialGrad.Attribute("id")?.Value ?? "",
                    Type = "radial",
                    CX = GetDoubleAttribute(radialGrad, "cx", 0.5),
                    CY = GetDoubleAttribute(radialGrad, "cy", 0.5),
                    R = GetDoubleAttribute(radialGrad, "r", 0.5)
                };

                foreach (XElement stop in radialGrad.Elements().Where(e => e.Name.LocalName == "stop"))
                {
                    double offset = GetDoubleAttribute(stop, "offset", 0);
                    string color = stop.Attribute("stop-color")?.Value ?? "black";
                    gradient.Stops.Add((offset, ConvertColorToWpf(color)));
                }

                if (!string.IsNullOrEmpty(gradient.Id))
                    _parsedGradients[gradient.Id] = gradient;
            }
        }
    }

    /// <summary>
    /// Utility class for working with SVG icons in WPF
    /// </summary>
    public class SvgIconManager : IDisposable
    {
        private readonly SvgToXamlConverter _converter;
        private readonly Dictionary<string, string> _xamlCache;
        private readonly string _cacheDirectory;

        public SvgIconManager(string cacheDirectory = null)
        {
            _converter = new SvgToXamlConverter();
            _xamlCache = new Dictionary<string, string>();
            _cacheDirectory = cacheDirectory ?? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "MyriaRPG", "IconCache");
        }

        /// <summary>
        /// Gets or converts an SVG icon to XAML
        /// </summary>
        public string GetIconXaml(string iconId, string svgPath)
        {
            if (_xamlCache.TryGetValue(iconId, out var cachedXaml))
                return cachedXaml;

            string xaml = _converter.ConvertSvgToXaml(svgPath);
            _xamlCache[iconId] = xaml;

            // Optionally save to disk cache
            if (_cacheDirectory != null)
                SaveToCache(iconId, xaml);

            return xaml;
        }

        /// <summary>
        /// Batch converts multiple SVG files
        /// </summary>
        public Dictionary<string, string> ConvertMultipleSvgs(Dictionary<string, string> iconMap)
        {
            var results = new Dictionary<string, string>();

            foreach (var kvp in iconMap)
            {
                try
                {
                    results[kvp.Key] = GetIconXaml(kvp.Key, kvp.Value);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to convert {kvp.Key}: {ex.Message}");
                }
            }

            return results;
        }

        /// <summary>
        /// Saves XAML to disk cache
        /// </summary>
        private void SaveToCache(string iconId, string xamlContent)
        {
            try
            {
                Directory.CreateDirectory(_cacheDirectory);
                string cachePath = Path.Combine(_cacheDirectory, $"{iconId}.xaml");
                File.WriteAllText(cachePath, xamlContent, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save to cache: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads cached XAML if it exists
        /// </summary>
        public bool TryLoadFromCache(string iconId, out string xamlContent)
        {
            xamlContent = null;

            if (_cacheDirectory == null || !Directory.Exists(_cacheDirectory))
                return false;

            string cachePath = Path.Combine(_cacheDirectory, $"{iconId}.xaml");
            if (!File.Exists(cachePath))
                return false;

            try
            {
                xamlContent = File.ReadAllText(cachePath, Encoding.UTF8);
                _xamlCache[iconId] = xamlContent;
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load from cache: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Clears memory cache
        /// </summary>
        public void ClearMemoryCache()
        {
            _xamlCache.Clear();
        }

        /// <summary>
        /// Clears disk cache
        /// </summary>
        public void ClearDiskCache()
        {
            try
            {
                if (Directory.Exists(_cacheDirectory))
                    Directory.Delete(_cacheDirectory, true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to clear disk cache: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets cache statistics
        /// </summary>
        public (int MemoryCacheCount, int DiskCacheCount) GetCacheStats()
        {
            int diskCount = 0;
            if (Directory.Exists(_cacheDirectory))
                diskCount = Directory.GetFiles(_cacheDirectory, "*.xaml").Length;

            return (_xamlCache.Count, diskCount);
        }

        public void Dispose()
        {
            _xamlCache?.Clear();
        }
    }

    /// <summary>
    /// Batch processor for converting entire SVG icon sets
    /// </summary>
    public class SvgBatchProcessor
    {
        private readonly SvgToXamlConverter _converter;
        private int _successCount;
        private int _failureCount;

        public event EventHandler<string> OnProgress;
        public event EventHandler<string> OnError;

        public SvgBatchProcessor()
        {
            _converter = new SvgToXamlConverter();
        }

        /// <summary>
        /// Processes all SVG files in a directory
        /// </summary>
        public void ProcessDirectory(string inputDirectory, string outputDirectory, bool overwrite = false)
        {
            if (!Directory.Exists(inputDirectory))
                throw new DirectoryNotFoundException($"Input directory not found: {inputDirectory}");

            Directory.CreateDirectory(outputDirectory);

            var svgFiles = Directory.GetFiles(inputDirectory, "*.svg", SearchOption.TopDirectoryOnly);
            _successCount = 0;
            _failureCount = 0;

            OnProgress?.Invoke(this, $"Starting conversion of {svgFiles.Length} files...");

            foreach (var svgFile in svgFiles)
            {
                try
                {
                    string fileName = Path.GetFileNameWithoutExtension(svgFile);
                    string outputPath = Path.Combine(outputDirectory, $"{fileName}.xaml");

                    if (File.Exists(outputPath) && !overwrite)
                    {
                        OnProgress?.Invoke(this, $"Skipping (already exists): {fileName}");
                        continue;
                    }

                    _converter.ConvertSvgFileToXamlFile(svgFile, outputPath);
                    _successCount++;
                    OnProgress?.Invoke(this, $"✓ Converted: {fileName}");
                }
                catch (Exception ex)
                {
                    _failureCount++;
                    OnError?.Invoke(this, $"✗ Failed to convert {Path.GetFileName(svgFile)}: {ex.Message}");
                }
            }

            OnProgress?.Invoke(this, $"Completed: {_successCount} successful, {_failureCount} failed");
        }

        /// <summary>
        /// Processes SVG files recursively
        /// </summary>
        public void ProcessDirectoryRecursive(string inputDirectory, string outputDirectory, bool overwrite = false)
        {
            if (!Directory.Exists(inputDirectory))
                throw new DirectoryNotFoundException($"Input directory not found: {inputDirectory}");

            Directory.CreateDirectory(outputDirectory);

            var svgFiles = Directory.GetFiles(inputDirectory, "*.svg", SearchOption.AllDirectories);
            _successCount = 0;
            _failureCount = 0;

            OnProgress?.Invoke(this, $"Starting recursive conversion of {svgFiles.Length} files...");

            foreach (var svgFile in svgFiles)
            {
                try
                {
                    // Create relative output directory structure
                    string relativePath = Path.GetRelativePath(inputDirectory, svgFile);
                    string relativeDir = Path.GetDirectoryName(relativePath);
                    string fileName = Path.GetFileNameWithoutExtension(svgFile);

                    string outputDir = Path.Combine(outputDirectory, relativeDir);
                    Directory.CreateDirectory(outputDir);

                    string outputPath = Path.Combine(outputDir, $"{fileName}.xaml");

                    if (File.Exists(outputPath) && !overwrite)
                    {
                        OnProgress?.Invoke(this, $"Skipping (already exists): {relativePath}");
                        continue;
                    }

                    _converter.ConvertSvgFileToXamlFile(svgFile, outputPath);
                    _successCount++;
                    OnProgress?.Invoke(this, $"✓ Converted: {relativePath}");
                }
                catch (Exception ex)
                {
                    _failureCount++;
                    OnError?.Invoke(this, $"✗ Failed to convert {svgFile}: {ex.Message}");
                }
            }

            OnProgress?.Invoke(this, $"Completed: {_successCount} successful, {_failureCount} failed");
        }

        /// <summary>
        /// Gets conversion statistics
        /// </summary>
        public (int Success, int Failure, int Total) GetStats()
        {
            return (_successCount, _failureCount, _successCount + _failureCount);
        }
    }

    /// <summary>
    /// Resource dictionary generator for XAML icons
    /// </summary>
    public class SvgResourceDictionaryGenerator
    {
        private readonly SvgToXamlConverter _converter;

        public SvgResourceDictionaryGenerator()
        {
            _converter = new SvgToXamlConverter();
        }

        /// <summary>
        /// Generates a WPF ResourceDictionary from SVG files
        /// </summary>
        public void GenerateResourceDictionary(Dictionary<string, string> iconMap, string outputPath)
        {
            StringBuilder resourceDict = new();

            resourceDict.AppendLine(@"<ResourceDictionary");
            resourceDict.AppendLine(@"    xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""");
            resourceDict.AppendLine(@"    xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">");

            foreach (var kvp in iconMap)
            {
                try
                {
                    string xaml = _converter.ConvertSvgToXaml(kvp.Value);
                    
                    // Extract just the Canvas content
                    resourceDict.AppendLine();
                    resourceDict.AppendLine($"    <!-- Icon: {kvp.Key} -->");
                    resourceDict.AppendLine($"    <Canvas x:Key=\"Icon.{kvp.Key}\">");
                    resourceDict.AppendLine(ExtractCanvasContent(xaml));
                    resourceDict.AppendLine($"    </Canvas>");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to generate resource for {kvp.Key}: {ex.Message}");
                }
            }

            resourceDict.AppendLine();
            resourceDict.AppendLine("</ResourceDictionary>");

            File.WriteAllText(outputPath, resourceDict.ToString(), Encoding.UTF8);
        }

        private string ExtractCanvasContent(string xaml)
        {
            // Simple extraction - finds content between <Canvas> tags
            int startIndex = xaml.IndexOf('>');
            int endIndex = xaml.LastIndexOf('<');

            if (startIndex > 0 && endIndex > startIndex)
                return xaml.Substring(startIndex + 1, endIndex - startIndex - 1).Trim();

            return "";
        }
    }
}
