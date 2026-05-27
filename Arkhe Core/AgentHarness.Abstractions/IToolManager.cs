using System.Collections.Generic;

namespace AgentHarness.Abstractions;

public interface IToolManager
{
    void RegisterTool(object tool);
    IEnumerable<object> GetRegisteredTools();
}
