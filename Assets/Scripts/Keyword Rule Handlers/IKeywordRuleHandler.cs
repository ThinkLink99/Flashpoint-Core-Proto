public enum KeywordTiming
{
    Passive,
    BeforeAction,
    DuringAttack,
    AfterAttack,
    OnDamageTaken,
    OnActivationStart,
    OnActivationEnd,
    GrantsAction,
    ModifiesTargeting,
    ModifiesMovement,
    Consumable
}

public interface IKeywordRuleHandler
{
    string KeywordId { get; }
}

public interface IBeforeDamageHandler : IKeywordRuleHandler
{
    void BeforeDamage(GameActionContext context);
}
public interface IAfterDamageHandler : IKeywordRuleHandler
{
    void AfterDamage(GameActionContext context);
}