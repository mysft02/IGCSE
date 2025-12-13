using BusinessObject.Enums;

namespace Service.Assistant;

public class AssistantPersonaConfig
{
    public AssistantPersona Persona { get; set; }
    public string SystemPrompt { get; set; } = null!;
    public string Personality { get; set; } = null!;
    public string Tone { get; set; } = null!;
    public string[] AllowedTopics { get; set; } = Array.Empty<string>();
    public Dictionary<string, object> BehaviorRules { get; set; } = new();
}

public interface IPersonaService
{
    AssistantPersonaConfig GetPersonaConfig(string userRole);
    string BuildSystemPrompt(AssistantPersonaConfig persona, ChatContext context, string ragContext);
}

