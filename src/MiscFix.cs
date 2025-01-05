namespace Sainot;

public partial class Sainot
{
    private void RoomOnAddObject(On.Room.orig_AddObject orig, Room self, UpdatableAndDeletable obj)
    {
        if (obj is ScavengerBomb && self.updateList.Contains(obj))
            return;

        orig(self, obj);
    }
}