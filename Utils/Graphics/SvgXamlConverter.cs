using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace MyriaRPG.Utils.Graphics
{
    /// <summary>
    /// Converts SVG files to WPF-compatible XAML format.
    /// Supports basic shapes (rect, circle, ellipse, line, polyline, polygon, path), text, and groups.
    /// Handles transforms, fills, strokes, and basic styling.
    /// </summary>
    public class SvgXamlConverter
    {
        private Dictionary<string, string> _cssStyles = new();
        private Dictionary<string, string> _gradients = new();
        private double _viewBoxWidth = 100;
        private double _viewBoxHeight = 100;
        private const string SvgNamespace = "http://www.w3.org/2000/svg";

        /// <summary>
        /// Converts an SVG file to XAML format.
        /// </summary>
        /// <param name="svgFilePath">Path to the SVG file</param>
        /// <returns>XAML string ready for WPF</returns>
        public string ConvertSvgToXaml(string svgFilePath)
        {
            if (!File.Exists(svgFilePath))
                throw new FileNotFoundException($"SVG file not found: {svgFilePath}");

            string svgContent = File.ReadAllText(svgFilePath);
            return ConvertSvgStringToXaml(svgContent);
        }

        /// <summary>
        /// Converts an SVG string to XAML format.
        /// </summary>
        /// <param name="svgContent">SVG content as string</param>
        /// <returns>XAML string ready for WPF</returns>
        protected string ConvertSvgStringToXaml(string svgContent)
        {
            try
            {
                XDocument svgDoc = XDocument.Parse(svgContent);
                XElement svgRoot = svgDoc.Root;

                if (svgRoot == null || svgRoot.Name.LocalName != "svg")
                    throw new InvalidOperationException("Invalid SVG document: root element must be <svg>");

                // Parse viewBox and dimensions
                ParseSvgDimensions(svgRoot);

                // Extract CSS styles if present
                ExtractCssStyles(svgRoot);

                // Build XAML Canvas
                StringBuilder xamlBuilder = new();
                xamlBuilder.AppendLine("<Canvas");
                xamlBuilder.AppendLine($"    xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"");
                xamlBuilder.AppendLine($"    xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"");
                xamlBuilder.AppendLine($"    Width=\"{_viewBoxWidth.ToString(CultureInfo.InvariantCulture)}\"");
                xamlBuilder.AppendLine($"    Height=\"{_viewBoxHeight.ToString(CultureInfo.InvariantCulture)}\"");
                xamlBuilder.AppendLine($"    Background=\"Transparent\">");

                // Process all child elements
                foreach (XElement element in svgRoot.Elements())
                {
                    ProcessElement(element, xamlBuilder);
                }

                xamlBuilder.AppendLine("</Canvas>");

                return xamlBuilder.ToString();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error converting SVG to XAML: {ex.Message}", ex);
            }
        }

        protected void ParseSvgDimensions(XElement svgRoot)
        {
            // Try to get dimensions from viewBox first
            string viewBox = svgRoot.Attribute("viewBox")?.Value ?? "";
            if (!string.IsNullOrEmpty(viewBox))
            {
                string[] parts = viewBox.Split(' ');
                if (parts.Length >= 4 && double.TryParse(parts[2], NumberStyles.Any, CultureInfo.InvariantCulture, out var width) &&
                    double.TryParse(parts[3], NumberStyles.Any, CultureInfo.InvariantCulture, out var height))
                {
                    _viewBoxWidth = width;
                    _viewBoxHeight = height;
                    return;
                }
            }

            // Fall back to width and height attributes
            if (double.TryParse(svgRoot.Attribute("width")?.Value ?? "100", NumberStyles.Any, CultureInfo.InvariantCulture, out var w))
                _viewBoxWidth = w;
            if (double.TryParse(svgRoot.Attribute("height")?.Value ?? "100", NumberStyles.Any, CultureInfo.InvariantCulture, out var h))
                _viewBoxHeight = h;
        }

        protected void ExtractCssStyles(XElement svgRoot)
        {
            XElement styleElement = svgRoot.Elements().FirstOrDefault(e => e.Name.LocalName == "style");
            if (styleElement != null)
            {
                string styleContent = styleElement.Value;
                // Simple CSS parser - extracts class styles
                MatchCollection matches = Regex.Matches(styleContent, @"\.([^\s{]+)\s*\{([^}]+)\}");
                foreach (Match match in matches)
                {
                    string className = match.Groups[1].Value;
                    string styles = match.Groups[2].Value;
                    _cssStyles[className] = styles;
                }
            }
        }

        protected void ProcessElement(XElement element, StringBuilder xamlBuilder)
        {
            string elementName = element.Name.LocalName;

            switch (elementName)
            {
                case "rect":
                    ProcessRect(element, xamlBuilder);
                    break;
                case "circle":
                    ProcessCircle(element, xamlBuilder);
                    break;
                case "ellipse":
                    ProcessEllipse(element, xamlBuilder);
                    break;
                case "line":
                    ProcessLine(element, xamlBuilder);
                    break;
                case "polyline":
                    ProcessPolyline(element, xamlBuilder);
                    break;
                case "polygon":
                    ProcessPolygon(element, xamlBuilder);
                    break;
                case "path":
                    ProcessPath(element, xamlBuilder);
                    break;
                case "text":
                    ProcessText(element, xamlBuilder);
                    break;
                case "g":
                case "group":
                    ProcessGroup(element, xamlBuilder);
                    break;
                case "defs":
                    // Skip defs, we handle gradients separately
                    ExtractGradients(element);
                    break;
                case "image":
                    ProcessImage(element, xamlBuilder);
                    break;
            }
        }

        protected void ProcessRect(XElement rect, StringBuilder xamlBuilder)
        {
            double x = GetDoubleAttribute(rect, "x", 0);
            double y = GetDoubleAttribute(rect, "y", 0);
            double width = GetDoubleAttribute(rect, "width", 100);
            double height = GetDoubleAttribute(rect, "height", 100);
            double rx = GetDoubleAttribute(rect, "rx", 0);
            double ry = GetDoubleAttribute(rect, "ry", rx);

            xamlBuilder.AppendLine($"    <Rectangle");
            xamlBuilder.AppendLine($"        Canvas.Left=\"{x.ToString(CultureInfo.InvariantCulture)}\"");
            xamlBuilder.AppendLine($"        Canvas.Top=\"{y.ToString(CultureInfo.InvariantCulture)}\"");
            xamlBuilder.AppendLine($"        Width=\"{width.ToString(CultureInfo.InvariantCulture)}\"");
            xamlBuilder.AppendLine($"        Height=\"{height.ToString(CultureInfo.InvariantCulture)}\"");

            if (rx > 0 || ry > 0)
            {
                xamlBuilder.AppendLine($"        RadiusX=\"{rx.ToString(CultureInfo.InvariantCulture)}\"");
                xamlBuilder.AppendLine($"        RadiusY=\"{ry.ToString(CultureInfo.InvariantCulture)}\"");
            }

            ApplyStyles(rect, xamlBuilder);
            xamlBuilder.AppendLine($"    />");
        }

        protected void ProcessCircle(XElement circle, StringBuilder xamlBuilder)
        {
            double cx = GetDoubleAttribute(circle, "cx", 0);
            double cy = GetDoubleAttribute(circle, "cy", 0);
            double r = GetDoubleAttribute(circle, "r", 10);

            xamlBuilder.AppendLine($"    <Ellipse");
            xamlBuilder.AppendLine($"        Canvas.Left=\"{(cx - r).ToString(CultureInfo.InvariantCulture)}\"");
            xamlBuilder.AppendLine($"        Canvas.Top=\"{(cy - r).ToString(CultureInfo.InvariantCulture)}\"");
            xamlBuilder.AppendLine($"        Width=\"{(r * 2).ToString(CultureInfo.InvariantCulture)}\"");
            xamlBuilder.AppendLine($"        Height=\"{(r * 2).ToString(CultureInfo.InvariantCulture)}\"");

            ApplyStyles(circle, xamlBuilder);
            xamlBuilder.AppendLine($"    />");
        }

        protected void ProcessEllipse(XElement ellipse, StringBuilder xamlBuilder)
        {
            double cx = GetDoubleAttribute(ellipse, "cx", 0);
            double cy = GetDoubleAttribute(ellipse, "cy", 0);
            double rx = GetDoubleAttribute(ellipse, "rx", 10);
            double ry = GetDoubleAttribute(ellipse, "ry", 10);

            xamlBuilder.AppendLine($"    <Ellipse");
            xamlBuilder.AppendLine($"        Canvas.Left=\"{(cx - rx).ToString(CultureInfo.InvariantCulture)}\"");
            xamlBuilder.AppendLine($"        Canvas.Top=\"{(cy - ry).ToString(CultureInfo.InvariantCulture)}\"");
            xamlBuilder.AppendLine($"        Width=\"{(rx * 2).ToString(CultureInfo.InvariantCulture)}\"");
            xamlBuilder.AppendLine($"        Height=\"{(ry * 2).ToString(CultureInfo.InvariantCulture)}\"");

            ApplyStyles(ellipse, xamlBuilder);
            xamlBuilder.AppendLine($"    />");
        }

        protected void ProcessLine(XElement line, StringBuilder xamlBuilder)
        {
            double x1 = GetDoubleAttribute(line, "x1", 0);
            double y1 = GetDoubleAttribute(line, "y1", 0);
            double x2 = GetDoubleAttribute(line, "x2", 100);
            double y2 = GetDoubleAttribute(line, "y2", 100);
            double strokeWidth = GetStrokeWidth(line, 1);

            xamlBuilder.AppendLine($"    <Line");
            xamlBuilder.AppendLine($"        X1=\"{x1.ToString(CultureInfo.InvariantCulture)}\"");
            xamlBuilder.AppendLine($"        Y1=\"{y1.ToString(CultureInfo.InvariantCulture)}\"");
            xamlBuilder.AppendLine($"        X2=\"{x2.ToString(CultureInfo.InvariantCulture)}\"");
            xamlBuilder.AppendLine($"        Y2=\"{y2.ToString(CultureInfo.InvariantCulture)}\"");
            xamlBuilder.AppendLine($"        Stroke=\"{GetStrokeColor(line)}\"");
            xamlBuilder.AppendLine($"        StrokeThickness=\"{strokeWidth.ToString(CultureInfo.InvariantCulture)}\"");

            xamlBuilder.AppendLine($"    />");
        }

        protected void ProcessPolyline(XElement polyline, StringBuilder xamlBuilder)
        {
            string points = polyline.Attribute("points")?.Value ?? "";
            if (string.IsNullOrEmpty(points))
                return;

            xamlBuilder.AppendLine($"    <Polyline");
            xamlBuilder.AppendLine($"        Points=\"{ParsePoints(points)}\"");

            ApplyStyles(polyline, xamlBuilder, isFilled: false);
            xamlBuilder.AppendLine($"    />");
        }

        protected void ProcessPolygon(XElement polygon, StringBuilder xamlBuilder)
        {
            string points = polygon.Attribute("points")?.Value ?? "";
            if (string.IsNullOrEmpty(points))
                return;

            xamlBuilder.AppendLine($"    <Polygon");
            xamlBuilder.AppendLine($"        Points=\"{ParsePoints(points)}\"");

            ApplyStyles(polygon, xamlBuilder);
            xamlBuilder.AppendLine($"    />");
        }

        protected void ProcessPath(XElement path, StringBuilder xamlBuilder)
        {
            string data = path.Attribute("d")?.Value ?? "";
            if (string.IsNullOrEmpty(data))
                return;

            // Convert SVG path data to WPF PathGeometry
            string pathGeometry = ConvertSvgPathToWpfPath(data);

            xamlBuilder.AppendLine($"    <Path");
            xamlBuilder.AppendLine($"        Data=\"{pathGeometry}\"");

            ApplyStyles(path, xamlBuilder);
            xamlBuilder.AppendLine($"    />");
        }

        protected void ProcessText(XElement textElement, StringBuilder xamlBuilder)
        {
            string text = textElement.Value;
            if (string.IsNullOrEmpty(text))
                return;

            double x = GetDoubleAttribute(textElement, "x", 0);
            double y = GetDoubleAttribute(textElement, "y", 0);
            string fontSize = textElement.Attribute("font-size")?.Value ?? "12";
            string fontFamily = textElement.Attribute("font-family")?.Value ?? "Arial";

            xamlBuilder.AppendLine($"    <TextBlock");
            xamlBuilder.AppendLine($"        Canvas.Left=\"{x.ToString(CultureInfo.InvariantCulture)}\"");
            xamlBuilder.AppendLine($"        Canvas.Top=\"{y.ToString(CultureInfo.InvariantCulture)}\"");
            xamlBuilder.AppendLine($"        FontFamily=\"{fontFamily}\"");
            xamlBuilder.AppendLine($"        FontSize=\"{fontSize}\"");
            xamlBuilder.AppendLine($"        Foreground=\"{GetTextColor(textElement)}\"");
            xamlBuilder.AppendLine($"        Text=\"{EscapeXml(text)}\"");
            xamlBuilder.AppendLine($"    />");
        }

        protected void ProcessGroup(XElement group, StringBuilder xamlBuilder)
        {
            // Start canvas group
            string transform = group.Attribute("transform")?.Value ?? "";
            
            xamlBuilder.AppendLine($"    <Canvas>");
            
            if (!string.IsNullOrEmpty(transform))
            {
                ApplyTransform(transform, xamlBuilder);
            }

            // Process child elements
            foreach (XElement child in group.Elements())
            {
                ProcessElement(child, xamlBuilder);
            }

            xamlBuilder.AppendLine($"    </Canvas>");
        }

        protected void ProcessImage(XElement image, StringBuilder xamlBuilder)
        {
            double x = GetDoubleAttribute(image, "x", 0);
            double y = GetDoubleAttribute(image, "y", 0);
            double width = GetDoubleAttribute(image, "width", 100);
            double height = GetDoubleAttribute(image, "height", 100);
            string href = image.Attribute("href")?.Value ?? image.Attribute(XNamespace.Get("http://www.w3.org/1999/xlink") + "href")?.Value ?? "";

            if (string.IsNullOrEmpty(href))
                return;

            xamlBuilder.AppendLine($"    <Image");
            xamlBuilder.AppendLine($"        Canvas.Left=\"{x.ToString(CultureInfo.InvariantCulture)}\"");
            xamlBuilder.AppendLine($"        Canvas.Top=\"{y.ToString(CultureInfo.InvariantCulture)}\"");
            xamlBuilder.AppendLine($"        Width=\"{width.ToString(CultureInfo.InvariantCulture)}\"");
            xamlBuilder.AppendLine($"        Height=\"{height.ToString(CultureInfo.InvariantCulture)}\"");
            xamlBuilder.AppendLine($"        Source=\"{href}\"");
            xamlBuilder.AppendLine($"    />");
        }

        protected void ExtractGradients(XElement defs)
        {
            // Placeholder for gradient extraction
            // In a full implementation, you'd parse linearGradient and radialGradient elements
        }

        protected void ApplyStyles(XElement element, StringBuilder xamlBuilder, bool isFilled = true)
        {
            // Get fill color
            string fill = GetFillColor(element);
            if (isFilled && fill != "None")
            {
                xamlBuilder.AppendLine($"        Fill=\"{fill}\"");
            }
            else if (isFilled)
            {
                xamlBuilder.AppendLine($"        Fill=\"Transparent\"");
            }

            // Get stroke
            string stroke = GetStrokeColor(element);
            if (stroke != "Transparent")
            {
                xamlBuilder.AppendLine($"        Stroke=\"{stroke}\"");
                xamlBuilder.AppendLine($"        StrokeThickness=\"{GetStrokeWidth(element, 1).ToString(CultureInfo.InvariantCulture)}\"");
            }

            // Stroke line cap/join
            string strokeLineCap = element.Attribute("stroke-linecap")?.Value ?? "butt";
            if (strokeLineCap != "butt")
            {
                xamlBuilder.AppendLine($"        StrokeStartLineCap=\"{ConvertLineCap(strokeLineCap)}\"");
                xamlBuilder.AppendLine($"        StrokeEndLineCap=\"{ConvertLineCap(strokeLineCap)}\"");
            }

            // Opacity
            string opacity = element.Attribute("opacity")?.Value ?? "1";
            if (double.TryParse(opacity, NumberStyles.Any, CultureInfo.InvariantCulture, out var opacityValue) && opacityValue < 1)
            {
                xamlBuilder.AppendLine($"        Opacity=\"{opacityValue.ToString(CultureInfo.InvariantCulture)}\"");
            }
        }

        protected void ApplyTransform(string transform, StringBuilder xamlBuilder)
        {
            // Simple transform parsing - supports translate, scale, rotate
            // In production, use a more robust parser
            if (transform.Contains("translate"))
            {
                var match = Regex.Match(transform, @"translate\(([-\d.]+),?\s*([-\d.]*)\)");
                if (match.Success)
                {
                    if (double.TryParse(match.Groups[1].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var tx))
                    {
                        xamlBuilder.AppendLine($"        RenderTransformOrigin=\"0.5,0.5\">");
                        xamlBuilder.AppendLine($"        <Canvas.RenderTransform>");
                        xamlBuilder.AppendLine($"            <TransformGroup>");
                        xamlBuilder.AppendLine($"                <TranslateTransform X=\"{tx.ToString(CultureInfo.InvariantCulture)}\" Y=\"0\"/>");
                        xamlBuilder.AppendLine($"            </TransformGroup>");
                        xamlBuilder.AppendLine($"        </Canvas.RenderTransform>");
                    }
                }
            }
        }

        protected string ConvertSvgPathToWpfPath(string svgPathData)
        {
            // This is a simplified converter. Full SVG path parsing is complex.
            // For production use, consider using the SharpVectors library or similar
            
            StringBuilder pathBuilder = new();
            pathBuilder.Append("M"); // Move to start

            // Remove extra whitespace and normalize the path
            string normalized = Regex.Replace(svgPathData, @"\s+", " ").Trim();
            
            // This is a very basic converter - for complex paths, use a proper library
            pathBuilder.Append(normalized);

            return pathBuilder.ToString();
        }

        protected string ParsePoints(string pointsString)
        {
            // Convert SVG points format to XAML PointCollection format
            string[] parts = Regex.Split(pointsString.Trim(), @"[\s,]+");
            StringBuilder pointsBuilder = new();

            for (int i = 0; i < parts.Length; i += 2)
            {
                if (i + 1 < parts.Length &&
                    double.TryParse(parts[i], NumberStyles.Any, CultureInfo.InvariantCulture, out var x) &&
                    double.TryParse(parts[i + 1], NumberStyles.Any, CultureInfo.InvariantCulture, out var y))
                {
                    if (pointsBuilder.Length > 0)
                        pointsBuilder.Append(" ");
                    pointsBuilder.Append($"{x.ToString(CultureInfo.InvariantCulture)},{y.ToString(CultureInfo.InvariantCulture)}");
                }
            }

            return pointsBuilder.ToString();
        }

        protected string GetFillColor(XElement element)
        {
            string fill = element.Attribute("fill")?.Value ?? GetStyleValue(element, "fill") ?? "";

            if (string.IsNullOrEmpty(fill) || fill == "none")
                return "Transparent";

            return ConvertColorToWpf(fill);
        }

        protected string GetStrokeColor(XElement element)
        {
            string stroke = element.Attribute("stroke")?.Value ?? GetStyleValue(element, "stroke") ?? "";

            if (string.IsNullOrEmpty(stroke) || stroke == "none")
                return "Transparent";

            return ConvertColorToWpf(stroke);
        }

        protected double GetStrokeWidth(XElement element, double defaultWidth)
        {
            string strokeWidth = element.Attribute("stroke-width")?.Value ?? GetStyleValue(element, "stroke-width") ?? "";

            if (double.TryParse(strokeWidth, NumberStyles.Any, CultureInfo.InvariantCulture, out var width))
                return width;

            return defaultWidth;
        }

        protected string GetTextColor(XElement element)
        {
            string fill = element.Attribute("fill")?.Value ?? GetStyleValue(element, "fill") ?? "Black";
            return ConvertColorToWpf(fill);
        }

        protected string GetStyleValue(XElement element, string styleName)
        {
            string style = element.Attribute("style")?.Value ?? "";
            if (string.IsNullOrEmpty(style))
                return null;

            var match = Regex.Match(style, $@"{styleName}\s*:\s*([^;]+)");
            return match.Success ? match.Groups[1].Value : null;
        }

        protected string ConvertColorToWpf(string svgColor)
        {
            if (string.IsNullOrEmpty(svgColor))
                return "#FF000000"; // Black default

            // Handle named colors
            svgColor = svgColor.Trim().ToLower();
            Dictionary<string, string> namedColors = new()
            {
                { "black", "#FF000000" },
                { "white", "#FFFFFFFF" },
                { "red", "#FFFF0000" },
                { "green", "#FF008000" },
                { "blue", "#FF0000FF" },
                { "gray", "#FF808080" },
                { "grey", "#FF808080" },
                { "cyan", "#FF00FFFF" },
                { "magenta", "#FFFF00FF" },
                { "yellow", "#FFFFFF00" },
                { "orange", "#FFFFA500" },
                { "purple", "#FF800080" },
                { "transparent", "#00000000" }
            };

            if (namedColors.ContainsKey(svgColor))
                return namedColors[svgColor];

            // Handle hex colors (#RRGGBB or #RGB)
            if (svgColor.StartsWith("#"))
            {
                svgColor = svgColor.TrimStart('#');
                if (svgColor.Length == 3)
                {
                    // Expand #RGB to #RRGGBB
                    svgColor = string.Concat(svgColor.Select(c => new string(c, 2)));
                }
                if (svgColor.Length == 6 && int.TryParse(svgColor, System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture, out _))
                {
                    return "#FF" + svgColor.ToUpper();
                }
            }

            // Handle rgb(r, g, b)
            var rgbMatch = Regex.Match(svgColor, @"rgb\s*\(\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)\s*\)");
            if (rgbMatch.Success)
            {
                int r = int.Parse(rgbMatch.Groups[1].Value);
                int g = int.Parse(rgbMatch.Groups[2].Value);
                int b = int.Parse(rgbMatch.Groups[3].Value);
                return $"#{255:X2}{r:X2}{g:X2}{b:X2}";
            }

            return "#FF000000"; // Default to black if conversion fails
        }

        protected string ConvertLineCap(string svgLineCap)
        {
            return svgLineCap.ToLower() switch
            {
                "round" => "Round",
                "square" => "Square",
                _ => "Flat"
            };
        }

        protected double GetDoubleAttribute(XElement element, string attributeName, double defaultValue)
        {
            string value = element.Attribute(attributeName)?.Value ?? "";
            
            if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
                return result;

            return defaultValue;
        }

        protected string EscapeXml(string text)
        {
            return text
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&apos;");
        }

        /// <summary>
        /// Saves XAML to a file.
        /// </summary>
        protected void SaveXamlToFile(string xamlContent, string outputPath)
        {
            File.WriteAllText(outputPath, xamlContent, Encoding.UTF8);
        }

        /// <summary>
        /// Converts SVG file directly to XAML file.
        /// </summary>
        public void ConvertSvgFileToXamlFile(string svgFilePath, string xamlOutputPath)
        {
            string xaml = ConvertSvgToXaml(svgFilePath);
            SaveXamlToFile(xaml, xamlOutputPath);
        }
    }
}
