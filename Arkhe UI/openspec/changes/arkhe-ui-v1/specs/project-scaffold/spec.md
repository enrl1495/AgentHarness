# Project Scaffold Specification

## Purpose

Defines the solution and project structure, references, and bootstrapping files required to build and run the WinUI 3 app.

## Requirements

### Requirement: Solution File

The system MUST include an `AgentHarness.WinUI.slnx` solution file referencing the WinUI project and Core sibling projects.

#### Scenario: Solution load

- GIVEN the repository root contains `AgentHarness.WinUI.slnx`
- WHEN `dotnet sln list` is executed
- THEN `AgentHarness.WinUI.csproj` is listed
- AND Core sibling projects are listed as `ProjectReference` entries

### Requirement: Project File

The system MUST include an `AgentHarness.WinUI.csproj` targeting `net10.0-windows10.0.19041.0`, with WinUI self-contained deployment and `ProjectReference` to Core siblings.

#### Scenario: Build succeeds

- GIVEN the `.csproj` is present
- WHEN `dotnet build` runs
- THEN compilation succeeds with zero errors
- AND the output contains `AgentHarness.WinUI.exe`

#### Scenario: Missing Core reference

- GIVEN a required Core `ProjectReference` is removed
- WHEN `dotnet build` runs
- THEN build fails with unresolved type errors

### Requirement: App Bootstrap

The system MUST provide `App.xaml` and `App.xaml.cs` that merge resource dictionaries and call `AddAgentHarnessCore()` during startup.

#### Scenario: Resource dictionaries merged

- GIVEN `App.xaml` merges `ModernTheme.xaml` and `ViewTemplates.xaml`
- WHEN the app launches
- THEN all pages can resolve `{StaticResource}` references

#### Scenario: DI container configured

- GIVEN `App.xaml.cs` overrides `OnLaunched`
- WHEN the app starts
- THEN `AddAgentHarnessCore()` is called on the service collection
- AND `MainWindow` is instantiated from the service provider
