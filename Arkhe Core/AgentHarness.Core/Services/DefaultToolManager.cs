using AgentHarness.Abstractions;

namespace AgentHarness.Core.Services;

public class DefaultToolManager : IToolManager
{
    private readonly List<object> _tools = new();

    public void RegisterTool(object tool)
    {
        _tools.Add(tool);
    }

    public IEnumerable<object> GetRegisteredTools()
    {
        return _tools;
    }
}
