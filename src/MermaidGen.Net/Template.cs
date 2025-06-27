namespace MermaidGen.Net;

using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Serilog;


public partial class Template() // Dictionary<string, TemplateValue> placeholders)
{
    public Dictionary<string, ReplacementValue> Placeholders { get; } = [];

    public ReplacementValue this[string key]
    {
        get => Placeholders[key];
        set => Placeholders[key] = value;
    }


    /// <summary>
    /// Expands the template by replacing simple placeholders and invoking methods for section placeholders.
    /// Accepts a file path for the template input and writes the output to another file.
    /// </summary>
    /// <param name="templatePath">Path to the template file or name of an embedded resource.</param>
    /// <param name="outputFilePath">Path to the output file where the expanded template will be written.</param>
    /// <param name="debug">If true, includes debug information in the output.</param>
    /// <exception cref="ArgumentException">Thrown if templatePath or outputFilePath is null or whitespace.</exception>
    /// <exception cref="FileNotFoundException">Thrown if the template file or embedded resource cannot be found.</exception>
    /// <remarks>
    /// If the template is found as a file, it reads from that file; 
    /// otherwise, it looks for an embedded resource with the specified name.    
    /// If the template is not found as either a file or an embedded resource, a FileNotFoundException is thrown.
    /// </remarks>
    public void Expand(string templatePath, TextWriter outputWriter, bool debug = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(templatePath, nameof(templatePath));
        ArgumentNullException.ThrowIfNull(outputWriter, nameof(outputWriter));

        using var templateReader = GetTemplateReader(templatePath);
        Expand(templateReader, outputWriter, debug);
    }

    /// <summary>
    /// Expands the template by replacing simple placeholders and invoking methods for section placeholders.
    /// </summary>
    public void Expand(TextReader templateReader, TextWriter outputWriter, bool debug = false)
    {
        ArgumentNullException.ThrowIfNull(templateReader, nameof(templateReader));
        ArgumentNullException.ThrowIfNull(outputWriter, nameof(outputWriter));

        string? line;
        int lineNumber = 0;
        Log.Debug("Starting template expansion...");
        while ((line = templateReader.ReadLine()) != null)
        {
            ProcessLine(line, lineNumber++, outputWriter, debug);
        }
        Log.Debug("Template expansion completed.");
    }


    private static StreamReader GetTemplateReader(string templatePath)
    {
        // Check if it's a file path
        if (File.Exists(templatePath))
        {
            return new StreamReader(templatePath);
        }

        // Try as embedded resource
        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        if (assembly is null)
        {
            throw new InvalidOperationException("Could not determine entry assembly");
        }
        if (TryGetResourceStream(assembly, templatePath, out var stream))
        {
            return new StreamReader(stream);
        }

        throw new FileNotFoundException($"No Template file or embedded resource found with name {templatePath} [available resources: {string.Join(", ", assembly.GetManifestResourceNames())}]");
    }

    private static bool TryGetResourceStream(System.Reflection.Assembly assembly, string templatePath, [MaybeNullWhen(false)] out Stream stream)
    {
        // Try direct match first
        stream = assembly.GetManifestResourceStream(templatePath);
        if (stream is not null)
        {
            return true;
        }
        // Find resources that end with the template path

        var availableResources = assembly.GetManifestResourceNames();
        var matchingResource = availableResources.FirstOrDefault(r => r.EndsWith($".{templatePath}") || r.EndsWith(templatePath));
        if (matchingResource is not null)
        {
            stream = assembly.GetManifestResourceStream(matchingResource);
            return stream is not null;
        }

        stream = null;
        return false;
    }

    /// <summary>
    /// write the result of expanding a single template line 
    /// simple placeholders have the form &lt;!-- name --&gt; and are replaced with the corresponding value from the dictionary in the line.
    /// section placeholders have the form &lt;!-- section:name --&gt; the corresponding action writes directly to the output.
    /// whitespace in front of the &lt;!-- will be written for every line the action writes to the output.
    /// </summary>
    private void ProcessLine(string line, int lineNumber, TextWriter outputWriter, bool debug)
    {
        if (!line.Contains("<!--"))
        {
            // quickly write the line without processing if it does not contain a placeholder
            outputWriter.WriteLine(line);
            return;
        }
        Log.Verbose("line {lineNumber}: processing: '{line}'", lineNumber, line);
        var sectionMatch = SectionPlaceholderRegex().Match(line);
        if (sectionMatch.Success)
        {
            // extract the section placeholder name
            var prefix = sectionMatch.Groups["prefix"].Value;
            var sectionName = sectionMatch.Groups["ident"].Value;
            var suffix = sectionMatch.Groups["suffix"].Value;
            // if there is a section placeholder with the right name and it is an action, run it
            if (Placeholders.TryGetValue(sectionName, out var value))
            {
                Log.Debug("line {lineNumber}: identified section '{sectionName}'", lineNumber, sectionName);
                if (value.TryGetAction(out var action))
                {
                    if (debug)
                    {
                        outputWriter.WriteLine($"{prefix}<!-- section:{sectionName} -->");
                    }

                    RunAction(line, prefix, sectionName, action, outputWriter, debug);
                    return; // Only run the first matching section placeholder and skip further processing for this line}
                }
            }
        }

        // Check for simple placeholders
        line = SimplePlaceholderRegex().Replace(line, SimplePlaceholderMatchEvaluator);
        outputWriter.WriteLine(line);


        // /////////////////////////
        // local methods

        // MatchEvaluator for the placeholder
        string SimplePlaceholderMatchEvaluator(Match match)
        {
            var placeholder = match.Groups[1].Value;
            if (Placeholders.TryGetValue(placeholder, out var value) && value.TryGetString(out var stringValue))
            {
                Log.Debug("line {lineNumber} replacing placeholder '{placeholder}' with '{stringValue}'", lineNumber, placeholder, stringValue);
                return stringValue;
            }
            else
            {
                Log.Warning("no value for placeholder '{placeholder}'", placeholder);
                return $"<!-- Warning: '{placeholder}' not provided or not a simple placeholder -->";
            }
        }
    }


    static void RunAction(string line, string prefix, string sectionName, Action<TextWriter> action, TextWriter outputWriter, bool debug)
    {
        if (debug)
        {
            outputWriter.WriteLine($"{prefix}<!-- section:{sectionName} -->");
        }

        if (prefix.Length == 0)
        {
            action(outputWriter);
        }
        else if (string.IsNullOrWhiteSpace(prefix))
        {
            // Use IndentedTextWriter to prefix each line with the whitespace
            using var indentedWriter = new System.CodeDom.Compiler.IndentedTextWriter(outputWriter, prefix) { Indent = 1 };
            action(indentedWriter);
        }
        else
        {
            // If there is non-whitespace before the placeholder, just write as is
            outputWriter.Write(prefix);
            outputWriter.Write("<!-- Warning: Non-whitespace before section placeholder detected. This may lead to unexpected formatting. -->");
            action(outputWriter);
        }
    }


    [GeneratedRegex(@"<!--\s*(\w+)\s*-->")]
    private static partial Regex SimplePlaceholderRegex();


    [GeneratedRegex(@"^(?<prefix>\s*)<!--\s*section\s*:\s*(?<ident>\w+)\s*-->(?<suffix>.*)$", RegexOptions.IgnoreCase)]
    private static partial Regex SectionPlaceholderRegex();

    internal static string[] AvailableResources() => System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceNames();
}
