public interface IEntity
{
    void OnEnable();
    void OnDisable();
    void OnDestroy();
    
    void Destroy();
    void Destroy(float delay);
}
