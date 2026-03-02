namespace _Project.Scripts.TowerDefense
{
    public static class TdConstant
    {
        public const float ATTACK_CONSTANT = 1f;
        public const float MAP_UNIT = 0.4f;
        public const float MOVEMENT_SPEED = 1f;
        public const float TIME_SCALE = 1f;
        
        public const float TD_PROJECTILE_ATTACK_DELAY = 0.2f;

        public const float TD_ENTITY_DEAD_DELAY = 0.6f;
        public const string TD_ENTITY_MOVE_PARAMETER = "isMoving";
        public const string TD_ENTITY_ATTACK_PARAMETER = "isAttacking";
        public const string TD_ENTITY_DEAD_PARAMETER = "isDead";

        public const string TD_TOWER_IDLE_PARAMETER = "isIdle";
        public const string TD_TOWER_ATTACK_PARAMETER = "isAttacking";
    }
}