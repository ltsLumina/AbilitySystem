public interface IEntity
{
    void OnEnable();

    void OnDisable();

    void OnDestroy();
}

public interface IDamageable
{
    void TakeDamage(float damage);
}
