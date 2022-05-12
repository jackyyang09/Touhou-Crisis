static class Facade
{
    public static PlayerUIManager playerUI => PlayerUIManager.Instance;
    public static GameplayModifiers modifiers => GameplayModifiers.Instance;
}