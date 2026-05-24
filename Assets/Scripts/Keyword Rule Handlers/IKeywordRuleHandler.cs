public interface IKeywordRuleHandler
{
    string KeywordId { get; }
}

public interface IBeforeDamageHandler : IKeywordRuleHandler
{
    void BeforeDamage(GameActionContext context);
}