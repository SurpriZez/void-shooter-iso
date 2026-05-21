public abstract class StatusEffect
{
    public float TimeRemaining { get; private set; }
    public abstract string Id { get; }

    protected StatusEffect(float duration) => TimeRemaining = duration;

    public void Refresh(float duration)
    {
        if (duration > TimeRemaining)
            TimeRemaining = duration;
    }

    public virtual void OnStack(StatusEffect incoming) => Refresh(incoming.TimeRemaining);

    public abstract void Tick(Enemy enemy, float delta);

    // Returns true when expired
    public bool Update(Enemy enemy, float delta)
    {
        Tick(enemy, delta);
        TimeRemaining -= delta;
        return TimeRemaining <= 0f;
    }
}
