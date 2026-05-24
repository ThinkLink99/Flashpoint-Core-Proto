using System.Collections;
using UnityEngine;

public class ShootAction : IGameAction
{
    public int Cost => 1;

    public bool CanExecute(GameActionContext ctx)
    {
        if (ctx == null) return false;

        if (ctx.SourceModel.ActionController.RemainingAP >= Cost) return true;
        // check that model has line of sight to target model 
        // will need future checks for keywords that force the weapon into a long shoot action
        else return false;
    }

    public IEnumerator Execute(GameActionContext ctx)
    {
        // TODO: add line of sight check and other checks for keywords that force the weapon into a long shoot action
        // TODO: add checks for cover and other modifiers to hit chance
        // TODO: Add Dice Roll mechanic for calculating total hits vs target's total saves
        ctx.IncomingDamage = 2;


        // apply before damage effects and modifiers here
        foreach (var handler in ctx.GetBeforeDamageHandlers(ctx.TargetModel))
            handler.BeforeDamage(ctx);

        // apply weapon armor piercing and model armor resist to damage
        ctx.IncomingDamage -= Mathf.Max(0, ctx.TargetModel.ModelConfiguration.unitArmor - ctx.WeaponUsed.weaponArmorPiercing);

        ctx.TargetModel.Wound(ctx.IncomingDamage);

        yield return null;
    }
}