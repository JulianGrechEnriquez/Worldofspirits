using WorldOfSpirits.Combat;

namespace WorldOfSpirits.Player
{
    public sealed class PlayerCharacter : LivingEntity
    {
        public override Faction Faction => global::WorldOfSpirits.Combat.Faction.Player;
    }
}
