using System.Text.RegularExpressions;
using AiPlatform.Core.Domain;

namespace AiPlatform.Agents;

public interface IPolicyEvaluator
{
    Task<bool> CanInvokeToolAsync(string toolId, string? tenantId, CancellationToken ct = default);
}

public class PolicyEvaluator : IPolicyEvaluator
{
    private readonly Core.Repositories.IPolicyRepository _policyRepository;

    public PolicyEvaluator(Core.Repositories.IPolicyRepository policyRepository)
    {
        _policyRepository = policyRepository;
    }

    public async Task<bool> CanInvokeToolAsync(string toolId, string? tenantId, CancellationToken ct = default)
    {
        var policies = await _policyRepository.ListAsync(tenantId, ct).ConfigureAwait(false);
        foreach (var policy in policies)
        {
            foreach (var rule in policy.Rules)
            {
                if (rule.Action.Equals("deny", StringComparison.OrdinalIgnoreCase))
                {
                    if (MatchesTool(rule.ToolPattern, toolId))
                        return false;
                }
                else if (rule.Action.Equals("allow", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrEmpty(rule.ToolPattern) || MatchesTool(rule.ToolPattern, toolId))
                        return true;
                }
            }
        }
        return true;
    }

    private static bool MatchesTool(string? pattern, string toolId)
    {
        if (string.IsNullOrEmpty(pattern)) return false;
        try
        {
            return Regex.IsMatch(toolId, "^" + Regex.Escape(pattern).Replace("\\*", ".*") + "$");
        }
        catch
        {
            return string.Equals(pattern, toolId, StringComparison.OrdinalIgnoreCase);
        }
    }
}
