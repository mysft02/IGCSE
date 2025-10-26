using System.Text.Json.Serialization;

namespace BusinessObject.Payload.Response.Trello;

public class TrelloAccountResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("aaId")]
    public string AaId { get; set; }

    [JsonPropertyName("activityBlocked")]
    public bool ActivityBlocked { get; set; }

    [JsonPropertyName("fullName")]
    public string FullName { get; set; }

    [JsonPropertyName("username")]
    public string Username { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; }

    [JsonPropertyName("initials")]
    public string Initials { get; set; }

    [JsonPropertyName("avatarUrl")]
    public string AvatarUrl { get; set; }

    [JsonPropertyName("avatarHash")]
    public string AvatarHash { get; set; }

    [JsonPropertyName("avatarSource")]
    public string AvatarSource { get; set; }

    [JsonPropertyName("confirmed")]
    public bool Confirmed { get; set; }

    [JsonPropertyName("memberType")]
    public string MemberType { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonPropertyName("bio")]
    public string Bio { get; set; }

    [JsonPropertyName("bioData")]
    public object BioData { get; set; }

    [JsonPropertyName("gravatarHash")]
    public string GravatarHash { get; set; }

    [JsonPropertyName("idBoards")]
    public List<string> IdBoards { get; set; }

    [JsonPropertyName("idOrganizations")]
    public List<string> IdOrganizations { get; set; }

    [JsonPropertyName("idEnterprise")]
    public string IdEnterprise { get; set; }

    [JsonPropertyName("idEnterprisesDeactivated")]
    public List<string> IdEnterprisesDeactivated { get; set; }

    [JsonPropertyName("idPremOrgsAdmin")]
    public List<string> IdPremOrgsAdmin { get; set; }

    [JsonPropertyName("products")]
    public List<string> Products { get; set; }

    [JsonPropertyName("limits")]
    public TrelloLimitsResponse Limits { get; set; }

    [JsonPropertyName("prefs")]
    public TrelloPrefsResponse Prefs { get; set; }

    [JsonPropertyName("dateLastImpression")]
    public string DateLastImpression { get; set; }

    [JsonPropertyName("dateLastActive")]
    public string DateLastActive { get; set; }

    [JsonPropertyName("marketingOptIn")]
    public TrelloMarketingOptInResponse MarketingOptIn { get; set; }

    [JsonPropertyName("trophies")]
    public List<string> Trophies { get; set; }

    [JsonPropertyName("oneTimeMessagesDismissed")]
    public List<string> OneTimeMessagesDismissed { get; set; }

    [JsonPropertyName("messagesDismissed")]
    public List<Dictionary<string, object>> MessagesDismissed { get; set; }

    [JsonPropertyName("nonPublic")]
    public Dictionary<string, object> NonPublic { get; set; }

    [JsonPropertyName("nonPublicAvailable")]
    public bool NonPublicAvailable { get; set; }

    [JsonPropertyName("loginTypes")]
    public List<string> LoginTypes { get; set; }

    [JsonPropertyName("nodeId")]
    public string NodeId { get; set; }

    [JsonPropertyName("sessionType")]
    public string SessionType { get; set; }

    [JsonPropertyName("domainClaimed")]
    public string DomainClaimed { get; set; }

    [JsonPropertyName("premiumFeatures")]
    public List<string> PremiumFeatures { get; set; }

    [JsonPropertyName("isAaMastered")]
    public bool IsAaMastered { get; set; }

    [JsonPropertyName("ixUpdate")]
    public string IxUpdate { get; set; }

    [JsonPropertyName("aaBlockSyncUntil")]
    public string AaBlockSyncUntil { get; set; }

    [JsonPropertyName("aaEmail")]
    public string AaEmail { get; set; }

    [JsonPropertyName("aaEnrolledDate")]
    public string AaEnrolledDate { get; set; }

    [JsonPropertyName("credentialsRemovedCount")]
    public int CredentialsRemovedCount { get; set; }

    [JsonPropertyName("idMemberReferrer")]
    public string IdMemberReferrer { get; set; }

    [JsonPropertyName("uploadedAvatarHash")]
    public string UploadedAvatarHash { get; set; }

    [JsonPropertyName("uploadedAvatarUrl")]
    public string UploadedAvatarUrl { get; set; }

    [JsonPropertyName("idEnterprisesAdmin")]
    public List<string> IdEnterprisesAdmin { get; set; }
}