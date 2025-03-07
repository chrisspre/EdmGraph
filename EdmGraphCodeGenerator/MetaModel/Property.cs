namespace omm.generator.Model;

public record class Property(string Name, PropertyType Type, bool IsOptional = false, string? DefaultValue = null)
{
}

