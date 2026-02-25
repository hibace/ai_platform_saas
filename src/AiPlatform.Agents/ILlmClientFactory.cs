namespace AiPlatform.Agents;

public interface ILlmClientFactory
{
    ILlmClient? GetClient(string? provider, string? model);
}
