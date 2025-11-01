using UnityEngine;
namespace ProjectUniverse.Environment.Hazards
{
    public enum DamageType
    {
        Fire,
        Heat,
        Explosion,
        Chemical,
        Environmental
    }

    public interface IEnvironmentalDamageReceiver
    {
        void ReceiveEnvironmentalDamage(float damage, DamageType damageType);
    }
    
}
