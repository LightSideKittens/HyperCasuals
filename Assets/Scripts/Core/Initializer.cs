using Common;

public class Initializer : BaseInitializer
{
    public BuyTheme defaultTheme;
    
    protected override void _Initialize()
    {
        defaultTheme.Do();
    }
}