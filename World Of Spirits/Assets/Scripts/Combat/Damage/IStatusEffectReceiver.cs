namespace WorldOfSpirits.Combat
{
    public enum CombatStatus
    {
        Burn,
        Poison,
        Bleed,
        Slow,
        Freeze,
        Stun,
        ArmorBreak
    }

    public interface IStatusEffectReceiver
    {
        void ApplyStatus(CombatStatus status, float duration, float strength);
    }
}
