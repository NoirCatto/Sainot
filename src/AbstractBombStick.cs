namespace Sainot;

public class AbstractBombStick : AbstractPhysicalObject.AbstractObjectStick
{
    public AbstractPhysicalObject Player
    {
        get => A;
        set => A = value;
    }

    public AbstractPhysicalObject ScavengerBomb
    {
        get => B;
        set => B = value;
    }

    public AbstractBombStick(AbstractPhysicalObject player, AbstractPhysicalObject bomb) : base(player, bomb)
    {
    }
}