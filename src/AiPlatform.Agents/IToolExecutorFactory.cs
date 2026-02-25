namespace AiPlatform.Agents;

public interface IToolExecutorFactory
{
    IToolExecutor? GetExecutor(string toolId);
}
