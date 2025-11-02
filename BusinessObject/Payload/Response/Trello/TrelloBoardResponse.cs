using System.Text.Json.Serialization;

namespace BusinessObject.Payload.Response.Trello
{
    public class TrelloBoardResponse
    {
        [JsonPropertyName("id")] public string Id { get; set; } = null!;
        [JsonPropertyName("nodeId")] public string? NodeId { get; set; }
        [JsonPropertyName("name")] public string Name { get; set; } = null!;
        [JsonPropertyName("desc")] public string? Desc { get; set; }
        [JsonPropertyName("descData")] public object? DescData { get; set; }
        [JsonPropertyName("closed")] public bool Closed { get; set; }
        [JsonPropertyName("dateClosed")] public string? DateClosed { get; set; }
        [JsonPropertyName("idOrganization")] public string? IdOrganization { get; set; }
        [JsonPropertyName("idEnterprise")] public string? IdEnterprise { get; set; }

        // Board limits are very extensive; keep as open object to be forward-compatible
        [JsonPropertyName("limits")] public Dictionary<string, object>? Limits { get; set; }

        [JsonPropertyName("pinned")] public bool Pinned { get; set; }
        [JsonPropertyName("starred")] public bool Starred { get; set; }
        [JsonPropertyName("url")] public string Url { get; set; } = null!;

        // Board-specific preferences (separate from member prefs)
        [JsonPropertyName("prefs")] public TrelloBoardPrefsResponse? Prefs { get; set; }

        [JsonPropertyName("shortLink")] public string? ShortLink { get; set; }
        [JsonPropertyName("subscribed")] public bool Subscribed { get; set; }
        [JsonPropertyName("labelNames")] public TrelloLabelNamesResponse? LabelNames { get; set; }
        [JsonPropertyName("powerUps")] public List<string>? PowerUps { get; set; }
        [JsonPropertyName("dateLastActivity")] public string? DateLastActivity { get; set; }
        [JsonPropertyName("dateLastView")] public string? DateLastView { get; set; }
        [JsonPropertyName("shortUrl")] public string? ShortUrl { get; set; }
        [JsonPropertyName("idTags")] public List<string>? IdTags { get; set; }
        [JsonPropertyName("datePluginDisable")] public string? DatePluginDisable { get; set; }
        [JsonPropertyName("creationMethod")] public string? CreationMethod { get; set; }
        [JsonPropertyName("ixUpdate")] public string? IxUpdate { get; set; }
        [JsonPropertyName("templateGallery")] public string? TemplateGallery { get; set; }
        [JsonPropertyName("enterpriseOwned")] public bool EnterpriseOwned { get; set; }
        [JsonPropertyName("idBoardSource")] public string? IdBoardSource { get; set; }
        [JsonPropertyName("premiumFeatures")] public List<string>? PremiumFeatures { get; set; }
        [JsonPropertyName("idMemberCreator")] public string? IdMemberCreator { get; set; }
        [JsonPropertyName("type")] public string? Type { get; set; }
        [JsonPropertyName("memberships")] public List<TrelloMembershipResponse>? Memberships { get; set; }
    }

    public class TrelloBoardPrefsResponse
    {
        [JsonPropertyName("permissionLevel")] public string? PermissionLevel { get; set; }
        [JsonPropertyName("hideVotes")] public bool? HideVotes { get; set; }
        [JsonPropertyName("voting")] public string? Voting { get; set; }
        [JsonPropertyName("comments")] public string? Comments { get; set; }
        [JsonPropertyName("invitations")] public string? Invitations { get; set; }
        [JsonPropertyName("selfJoin")] public bool? SelfJoin { get; set; }
        [JsonPropertyName("cardCovers")] public bool? CardCovers { get; set; }
        [JsonPropertyName("showCompleteStatus")] public bool? ShowCompleteStatus { get; set; }
        [JsonPropertyName("cardCounts")] public bool? CardCounts { get; set; }
        [JsonPropertyName("isTemplate")] public bool? IsTemplate { get; set; }
        [JsonPropertyName("cardAging")] public string? CardAging { get; set; }
        [JsonPropertyName("calendarFeedEnabled")] public bool? CalendarFeedEnabled { get; set; }
        [JsonPropertyName("hiddenPluginBoardButtons")] public List<string>? HiddenPluginBoardButtons { get; set; }
        [JsonPropertyName("switcherViews")] public List<TrelloSwitcherViewResponse>? SwitcherViews { get; set; }
        [JsonPropertyName("autoArchive")] public string? AutoArchive { get; set; }
        [JsonPropertyName("background")] public string? Background { get; set; }
        [JsonPropertyName("backgroundColor")] public string? BackgroundColor { get; set; }
        [JsonPropertyName("backgroundDarkColor")] public string? BackgroundDarkColor { get; set; }
        [JsonPropertyName("backgroundImage")] public string? BackgroundImage { get; set; }
        [JsonPropertyName("backgroundDarkImage")] public string? BackgroundDarkImage { get; set; }
        [JsonPropertyName("backgroundImageScaled")] public object? BackgroundImageScaled { get; set; }
        [JsonPropertyName("backgroundTile")] public bool? BackgroundTile { get; set; }
        [JsonPropertyName("backgroundBrightness")] public string? BackgroundBrightness { get; set; }
        [JsonPropertyName("sharedSourceUrl")] public string? SharedSourceUrl { get; set; }
        [JsonPropertyName("backgroundBottomColor")] public string? BackgroundBottomColor { get; set; }
        [JsonPropertyName("backgroundTopColor")] public string? BackgroundTopColor { get; set; }
        [JsonPropertyName("canBePublic")] public bool? CanBePublic { get; set; }
        [JsonPropertyName("canBeEnterprise")] public bool? CanBeEnterprise { get; set; }
        [JsonPropertyName("canBeOrg")] public bool? CanBeOrg { get; set; }
        [JsonPropertyName("canBePrivate")] public bool? CanBePrivate { get; set; }
        [JsonPropertyName("canInvite")] public bool? CanInvite { get; set; }
    }

    public class TrelloSwitcherViewResponse
    {
        [JsonPropertyName("viewType")] public string? ViewType { get; set; }
        [JsonPropertyName("enabled")] public bool? Enabled { get; set; }
    }

    public class TrelloLabelNamesResponse
    {
        [JsonPropertyName("green")] public string? Green { get; set; }
        [JsonPropertyName("yellow")] public string? Yellow { get; set; }
        [JsonPropertyName("orange")] public string? Orange { get; set; }
        [JsonPropertyName("red")] public string? Red { get; set; }
        [JsonPropertyName("purple")] public string? Purple { get; set; }
        [JsonPropertyName("blue")] public string? Blue { get; set; }
        [JsonPropertyName("sky")] public string? Sky { get; set; }
        [JsonPropertyName("lime")] public string? Lime { get; set; }
        [JsonPropertyName("pink")] public string? Pink { get; set; }
        [JsonPropertyName("black")] public string? Black { get; set; }
        [JsonPropertyName("green_dark")] public string? GreenDark { get; set; }
        [JsonPropertyName("yellow_dark")] public string? YellowDark { get; set; }
        [JsonPropertyName("orange_dark")] public string? OrangeDark { get; set; }
        [JsonPropertyName("red_dark")] public string? RedDark { get; set; }
        [JsonPropertyName("purple_dark")] public string? PurpleDark { get; set; }
        [JsonPropertyName("blue_dark")] public string? BlueDark { get; set; }
        [JsonPropertyName("sky_dark")] public string? SkyDark { get; set; }
        [JsonPropertyName("lime_dark")] public string? LimeDark { get; set; }
        [JsonPropertyName("pink_dark")] public string? PinkDark { get; set; }
        [JsonPropertyName("black_dark")] public string? BlackDark { get; set; }
        [JsonPropertyName("green_light")] public string? GreenLight { get; set; }
        [JsonPropertyName("yellow_light")] public string? YellowLight { get; set; }
        [JsonPropertyName("orange_light")] public string? OrangeLight { get; set; }
        [JsonPropertyName("red_light")] public string? RedLight { get; set; }
        [JsonPropertyName("purple_light")] public string? PurpleLight { get; set; }
        [JsonPropertyName("blue_light")] public string? BlueLight { get; set; }
        [JsonPropertyName("sky_light")] public string? SkyLight { get; set; }
        [JsonPropertyName("lime_light")] public string? LimeLight { get; set; }
        [JsonPropertyName("pink_light")] public string? PinkLight { get; set; }
        [JsonPropertyName("black_light")] public string? BlackLight { get; set; }
    }

    public class TrelloMembershipResponse
    {
        [JsonPropertyName("id")] public string? Id { get; set; }
        [JsonPropertyName("idMember")] public string? IdMember { get; set; }
        [JsonPropertyName("memberType")] public string? MemberType { get; set; }
        [JsonPropertyName("unconfirmed")] public bool? Unconfirmed { get; set; }
        [JsonPropertyName("deactivated")] public bool? Deactivated { get; set; }
    }
}
