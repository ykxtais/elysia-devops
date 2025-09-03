namespace ElysiaAPI.Domain.ValueObjects;

using System.Text.RegularExpressions;

public sealed record Placa
{
    public string Value { get; }

    private Placa(string value) => Value = value;

    public static Placa Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Placa é obrigatória.");

        var v = value.Trim().ToUpperInvariant();
        
        if (!Regex.IsMatch(v, "^[A-Z]{3}[0-9][A-Z0-9][0-9]{2}$"))
            throw new ArgumentException("Placa inválida.");

        return new Placa(v);
    }

    public override string ToString() => Value;
}
