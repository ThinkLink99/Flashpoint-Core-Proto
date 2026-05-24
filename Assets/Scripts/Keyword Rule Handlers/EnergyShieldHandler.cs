using UnityEngine;

public class EnergyShieldHandler : IBeforeDamageHandler
{
    public string KeywordId => "energy_shield";
    public void BeforeDamage(GameActionContext context)
    {
        // Example implementation: Reduce incoming damage by the value of the Energy Shield keyword
        if (context.TargetModel.HasKeyword(KeywordId))
        {
            var shieldKeyword = context.TargetModel.GetKeyword(KeywordId);
            int shieldValue = shieldKeyword.CurrentUses;

            shieldKeyword.DecrementCurrentUses(context.IncomingDamage); // Reduce the shield's current uses by the incoming damage. Will max out a default as we can't have negative uses.

            // Reduce incoming damage by shield value, but not below 0
            context.IncomingDamage = Mathf.Max(0, context.IncomingDamage - shieldValue);
        }
    }
}

public class LethalHandler : IAfterDamageHandler
{
    public string KeywordId => "lethal";
    public void AfterDamage(GameActionContext context)
    {
        // need to add weapon used to the context for this to work properly, but for now we'll just assume it has the keyword

    }
}
