using ChatApp.Schemas;

namespace ChatApp.Tables;

public class PartialUserProfile : DictionaryPartialDataclass, IChatUserProfile
{
    public PartialUserProfile(Dictionary<string, object> anonymousObject) : base(anonymousObject)
    {
    }

    public string GetUserId() => Fetch<string>("UserId");
    public string GetUserName() => Fetch<string>("UserName");
    public string GetPfpUrl() => Fetch<string>("ProfilePictureUrl");
    public string GetDescription() => Fetch<string>("Description");
    public bool IsDisabled() => FetchNative<bool>("Disabled");
    public bool IsDeleted() => FetchNative<bool>("Deleted");
}

public class UserProfile : IChatUserProfile
{
    public string UserId { get; set; } = "";
    public string UserName { get; set; } = "";
    public string ProfilePictureUrl { get; set; } = "";
    public string Description { get; set; } = "";
    public bool Disabled { get; set; } = false;
    public bool Deleted { get; set; } = false;

    public string GetUserId() => UserId;
    public string GetUserName() => UserName;
    public string GetPfpUrl() => ProfilePictureUrl;
    public string GetDescription() => Description;
    public bool IsDisabled() => Disabled;
    public bool IsDeleted() => Deleted;
}