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
        ctx.IncomingDamage = 5;
        //ctx.IncomingDamage = Random.Range(0, 6);
        Debug.Log($"Rolled {ctx.IncomingDamage} Damage.");

        // apply before damage effects and modifiers here
        foreach (var handler in ctx.GetHandlers<IBeforeDamageHandler>(ctx.TargetModel))
            handler.BeforeDamage(ctx);
        foreach (var handler in ctx.GetHandlers<IBeforeDamageHandler>(ctx.WeaponUsed))
            handler.BeforeDamage(ctx);

        var armor = Mathf.Max(0, ctx.TargetModel.ModelConfiguration.unitArmor - ctx.WeaponUsed.WeaponConfiguration.weaponArmorPiercing);
        Debug.Log($"Armor blocked {armor} Damage.");

        // apply weapon armor piercing and model armor resist to damage
        ctx.IncomingDamage -= Mathf.Max(0, armor);

        if (ctx.IncomingDamage > 0)
        {
            ctx.TargetModel.Wound(ctx.IncomingDamage);

            // Check for after damage effects like Lethal
            foreach (var handler in ctx.GetHandlers<IAfterDamageHandler>(ctx.TargetModel))
                handler.AfterDamage(ctx);
            foreach (var handler in ctx.GetHandlers<IAfterDamageHandler>(ctx.WeaponUsed))
                handler.AfterDamage(ctx);
        }

        yield return null;
    }
}