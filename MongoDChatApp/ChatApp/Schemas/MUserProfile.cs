using ChatApp.Schemas;
using ChatApp.Tables;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoDChatApp.ChatApp.Schemas;

public class MUserProfile : MDataclass<IChatUserProfile>
{
    [BsonId]
    [BsonElement("user_id")]
    public string UserId { get; set; } = "";
    
    [BsonElement("username")]
    public string UserName { get; set; } = "";
    
    [BsonElement("pfp_url")]
    public string ProfilePictureUrl { get; set; } = "";
    
    [BsonElement("description")]
    public string Description { get; set; } = "";
    
    [BsonElement("is_disabled")]
    public bool Disabled { get; set; } = false;
    
    [BsonElement("is_deleted")]
    public bool Deleted { get; set; } = false;

    public override IChatUserProfile ToCommon()
    {
        return new UserProfile()
        {
            UserId = UserId,
            UserName = UserName,
            ProfilePictureUrl = ProfilePictureUrl,
            Description = Description,
            Disabled = Disabled,
            Deleted = Deleted,
        };
    }

    public override void FromCommon(IChatUserProfile common)
    {
        UserId = common.GetUserId();
        UserName = common.GetUserName();
        ProfilePictureUrl = common.GetPfpUrl();
        Description = common.GetDescription();
        Disabled = common.IsDisabled();
        Deleted = common.IsDeleted();
    }
    
    public static MUserProfile Marshall(IChatUserProfile common)
        => Marshall<MUserProfile>(common);
}