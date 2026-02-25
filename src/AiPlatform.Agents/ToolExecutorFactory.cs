namespace AiPlatform.Agents;

public class ToolExecutorFactory : IToolExecutorFactory
{
    private readonly IReadOnlyDictionary<string, IToolExecutor> _executors;

    public ToolExecutorFactory(IEnumerable<IToolExecutor> executors)
    {
        _executors = executors.ToDictionary(e => e.ToolId, StringComparer.OrdinalIgnoreCase);
    }

    public IToolExecutor? GetExecutor(string toolId) =>
        _executors.TryGetValue(toolId, out var e) ? e : null;
}
