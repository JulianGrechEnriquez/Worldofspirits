namespace WorldOfSpirits.Combat
{
    public class DamageProjectile : ProjectileBase
    {
        protected override void OnHit(IDamageable target)
        {
            target.TakeDamage(Damage);
            Despawn();
        }
    }
}
