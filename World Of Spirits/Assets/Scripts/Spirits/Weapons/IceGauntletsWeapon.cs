namespace WorldOfSpirits.Spirits
{
    /// <summary>
    /// Ice specialization of the reusable thrust-melee weapon. Its damage,
    /// freeze, reach, timing, and target limits come from Ice Gauntlets.asset.
    /// </summary>
    public sealed class IceGauntletsWeapon : ThrustMeleeWeaponBase
    {
        protected override WeaponExecutionType ExpectedExecutionType =>
            WeaponExecutionType.PunchingMelee;
    }
}
