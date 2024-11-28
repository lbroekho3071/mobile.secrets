using System;
using System.Collections.Generic;
using System.Text;
using Mobile.Secrets.Utils;

namespace Mobile.Secrets
{
    public class CodeBuilder
{
    private readonly string _className;
    private readonly string _namespaceName;

    private List<string> _propertyDefinitions;

    public CodeBuilder(string className, string namespaceName)
    {
        _className = className;
        _namespaceName = namespaceName;

        _propertyDefinitions = new List<string>();
    }

    public CodeBuilder AddProperty(string propertyName, object value)
    {
        var property = MapPrimitiveProperty(value);
        if (property == null)
            return this;
        
        // TODO: validate and replace a character when it is not allowed by C#
        var sanitizedPropertyName = propertyName.Replace(".", "_");

        var indentationStringBuilder = new IndentationStringBuilder(1);
        
        indentationStringBuilder.AppendLine($"public static {property.Item1} {sanitizedPropertyName} ")
            .AppendLine("{")
            .IncrementIndentation()
            .AppendLine($"get => {property.Item2};")
            .DecrementIndentation()
            .AppendLine("}");

        var definition = indentationStringBuilder.ToString();
        _propertyDefinitions.Add(definition);

        return this;
    }

    private Tuple<string, string>? MapPrimitiveProperty(object value)
    {
        if (value is string stringValue)
            return new Tuple<string, string>("string", $"\"{stringValue}\"");
        if (value is long longValue)
            return new Tuple<string, string>("long", longValue.ToString());
        if (value is bool boolValue)
            return new Tuple<string, string>("bool", boolValue.ToString().ToLower());

        return null;
    }

    public string BuildString()
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine($"namespace {_namespaceName};")
            .AppendLine()
            .AppendLine($"public partial class {_className}")
            .AppendLine("{");

        foreach (var definition in _propertyDefinitions)
            stringBuilder.Append(definition);

        stringBuilder.AppendLine("}");

        return stringBuilder.ToString();
    }
}
}
