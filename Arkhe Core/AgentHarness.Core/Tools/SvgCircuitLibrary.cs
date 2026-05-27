namespace AgentHarness.Core.Tools;

/// <summary>
/// Proporciona una biblioteca estática de primitivas SVG para dibujar circuitos electrónicos.
/// </summary>
public static class SvgCircuitLibrary
{
    public const string Defs = @"<defs>
    <!-- Resistencia -->
    <g id=""resistor"">
        <path d=""M 0 10 L 10 10 L 15 0 L 25 20 L 35 0 L 45 20 L 50 10 L 60 10"" stroke=""black"" fill=""none"" stroke-width=""2""/>
    </g>
    <!-- Capacitor -->
    <g id=""capacitor"">
        <path d=""M 0 10 L 25 10 M 35 10 L 60 10 M 25 0 L 25 20 M 35 0 L 35 20"" stroke=""black"" fill=""none"" stroke-width=""2""/>
    </g>
    <!-- Diodo -->
    <g id=""diode"">
        <path d=""M 0 10 L 20 10 M 20 0 L 20 20 L 40 10 Z M 40 0 L 40 20 M 40 10 L 60 10"" stroke=""black"" fill=""none"" stroke-width=""2""/>
    </g>
    <!-- Inductor / Bobina -->
    <g id=""inductor"">
        <path d=""M 0 10 L 15 10 C 15 -5, 25 -5, 25 10 C 25 -5, 35 -5, 35 10 C 35 -5, 45 -5, 45 10 L 60 10"" stroke=""black"" fill=""none"" stroke-width=""2""/>
    </g>
    <!-- GND (Tierra) -->
    <g id=""gnd"">
        <path d=""M 15 0 L 15 15 M 0 15 L 30 15 M 5 20 L 25 20 M 10 25 L 20 25"" stroke=""black"" fill=""none"" stroke-width=""2""/>
    </g>
    <!-- Fuente de Voltaje DC -->
    <g id=""dc_source"">
        <circle cx=""20"" cy=""20"" r=""15"" stroke=""black"" fill=""none"" stroke-width=""2""/>
        <path d=""M 15 10 L 25 10 M 20 5 L 20 15 M 15 30 L 25 30"" stroke=""black"" fill=""none"" stroke-width=""1""/>
        <path d=""M 20 5 L 20 0 M 20 35 L 20 40"" stroke=""black"" fill=""none"" stroke-width=""2""/>
    </g>
</defs>";
}
