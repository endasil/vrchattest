using System.Collections;
using System.Collections.Generic;
using Assets.Scripts;
using UnityEngine;

public class PlayerDamageListener : PlayerListenerBase
{

    public override void OnIncreaseHealth(Player healer, Player receiver, int value)
    {
        base.OnIncreaseHealth(healer, receiver, value);
    }

    public override void OnDecreaseHealth(Player attacker, Player receiver, int value)
    {
        base.OnDecreaseHealth(attacker, receiver, value);
    }

    public override void OnMaxHealth(Player healer, Player receiver, int value)
    {
        base.OnMaxHealth(healer, receiver, value);
    }

    public override void OnMinHealth(Player attacker, Player receiver, int value)
    {
        base.OnMinHealth(attacker, receiver, value);
    }
}
