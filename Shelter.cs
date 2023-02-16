using System.Linq;

namespace Sainot;

public partial class Sainot
{
    private void ShelterDoorOnDoorClosed(On.ShelterDoor.orig_DoorClosed orig, ShelterDoor self)
    {
        foreach (var slug in self.room.physicalObjects.SelectMany(x => x).Where(x => x is Player))
        {
            if (IsBeltEnabled((Player)slug))
            {
                var belt = BombBelt.GetBelt((Player)slug);

                if (StartWithBombs) //Part #2 in Player.ctor
                {
                    //Prevent double spawning bombs
                    foreach (var bns in belt.Bombs.ToArray())
                    {
                        //bns.Value.Deactivate();
                        //bns.Key.Destroy();
                        self.room.abstractRoom.entities.Remove(bns.Key.abstractPhysicalObject); //Isn't removed when destroying object nor abstractobject anyways, ya, big brain time
                        //belt.Bombs.Remove(bns.Key);
                    }
                }
            }
        }

        orig(self);
    }
}