namespace MongoDChatApp.ChatApp.Schemas;

public abstract class MDataclass<T>
{
    public abstract T ToCommon();
    public abstract void FromCommon(T common);
    public static TFrom Marshall<TFrom>(T common) where TFrom : MDataclass<T>, new()
    {
        var ret = new TFrom();
        ret.FromCommon(common);
        return ret;
    }
}