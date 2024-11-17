using UdonSharp;
using UnityEngine;

namespace Assets.Scripts
{
    public abstract class PlayerListenerBase : UdonSharpBehaviour
    {
        public PlayerHandler playerHandler;

        public bool ControlsDamage;

        public virtual void OnIncreaseHealth(Player healer, Player receiver, int value)
        {

        }
        public virtual void OnDecreaseHealth(Player attacker, Player receiver, int value)
        {

        }
        public virtual void OnMaxHealth(Player healer, Player receiver, int value)
        {

        }
        public virtual void OnMinHealth(Player attacker, Player receiver, int value)
        {

        }

        public virtual bool CanDealHeal(Player healer, Player receiver)
        {
            return true;
        }

        public virtual bool CanDealDamage(Player attacker, Player receiver)
        {
            return true;
        }

        public virtual int AdjustDamage(Player attacker, Player receiver, int damage)
        {
            return damage;
        }
        //public override bool CanDealDamage(Player attacker, Player receiver)
        //{
        //    // Check if the game object is active. If not, the player can deal damage.
        //    bool isGameObjectActive = gameObject.activeInHierarchy;
        //    if (!isGameObjectActive)
        //    {
        //        return true;
        //    }

        //    // Calculate if the receiver is still within the invincibility period after respawning.
        //    float timeSinceReceiverRespawned = Time.timeSinceLevelLoad - respawnTimes[receiver.id];
        //    float totalInvincibilityDuration = respawnDelayTime + respawnInvincibilityTime;
        //    bool isReceiverVulnerable = timeSinceReceiverRespawned > totalInvincibilityDuration;

        //    return isReceiverVulnerable;
        //}
    }
}
