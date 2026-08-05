namespace WorldOfSpirits.Combat
{
    public interface IDamageable : ITargetable
    {
        void TakeDamage(float amount);
        void TakeDamage(DamageContext context);
    }
}
