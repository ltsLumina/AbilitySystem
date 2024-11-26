public interface IEntity
{
    void OnEnable();

    void OnDisable();

    void OnDestroy();

    void Destroy();

    void Destroy(float delay);
}

public interface IDamageable
{
    void TakeDamage(float damage, params StatusEffect[] statusEffects);
}
