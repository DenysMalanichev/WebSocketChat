class InitialMessageModel 
{
    public string UserName { get; set; } = null!;

    public string ChatId { get; set; } = null!;

    // override object.Equals
    public override bool Equals(object? obj)
    {        
        if (obj != null && obj is InitialMessageModel model)
        {
            return model.UserName == UserName && model.ChatId == ChatId;
        }
        
        return false;
    }

    public override int GetHashCode()
    {
        return (UserName + ChatId).GetHashCode();
    }
}