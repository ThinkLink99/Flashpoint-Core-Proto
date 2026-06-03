using UnityEngine;

public class LethalHandler : IAfterDamageHandler
{
    public string KeywordId => "lethal";
    public void AfterDamage(GameActionContext context)
    {
        // need to add weapon used to the context for this to work properly, but for now we'll just assume it has the keyword
        if (context.WeaponUsed.Keywords.HasKeyword(KeywordId))
        {
            var lethal = context.WeaponUsed.Keywords.GetKeyword(KeywordId).Value;

            Debug.Log($"{context.SourceModel.name} took {lethal} damage from Lethal");

            context.TargetModel.Wound(lethal);
        }
    }
}
