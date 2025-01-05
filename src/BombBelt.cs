using System.Collections.Generic;
using System.Linq;
using RWCustom;
using UnityEngine;

namespace Sainot;

public class BombBelt
{
    public const int TicksTreshold = 20;
    public const int NoAvailableGrasps = -1;

    public readonly PlayerData Data;

    public Player Owner => Data.Owner;
    public int Capacity => Sainot.BeltCapacity;

    public Dictionary<ScavengerBomb, AbstractBombStick> Bombs = new Dictionary<ScavengerBomb, AbstractBombStick>();
    public bool Increment;

    private int counter;
    private bool locked;

    public BombBelt(PlayerData data)
    {
        Data = data;
    }

    public void Update()
    {
        foreach (var bns in Bombs.ToList())
        {
            if (bns.Key.slatedForDeletetion || bns.Key.grabbedBy.Count > 0) //Bomb steal fix
            {
                bns.Value.Deactivate();
                bns.Key.CollideWithObjects = true;
                Bombs.Remove(bns.Key);
            }
        }

        if (Increment)
        {
            if (locked)
                return;

            counter++;
            if (!IsFull() && counter > TicksTreshold && BombInMainHand(out var bomb))
            {
                BombToBelt(bomb);
                locked = true;
                counter = 0;
            }
            else if (Bombs.Any() && counter > TicksTreshold && HasAFreeHand(out var index))
            {
                BombToHand(index);
                locked = true;
                counter = 0;
            }

        }
        else
        {
            counter = 0;
            locked = false;
        }
    }

    public void BombToBelt(ScavengerBomb bomb)
    {
        Owner.ReleaseGrasp(0);
        Owner.room.PlaySound(SoundID.Overseer_Image_Big_Flicker, Owner.mainBodyChunk, false, 1.5f, 0.6f);

        bomb.ChangeMode(Weapon.Mode.OnBack);
        bomb.CollideWithObjects = false;
        Bombs.Add(bomb, new AbstractBombStick(Owner.abstractPhysicalObject, bomb.abstractPhysicalObject));
    }

    public void BombToHand(int index)
    {
        if (index == NoAvailableGrasps)
            return;

        var bns = Bombs.First();

        bns.Value.Deactivate();
        bns.Key.CollideWithObjects = true;

        Owner.SlugcatGrab(bns.Key, index);
        Owner.room.PlaySound(SoundID.Overseer_Image_Big_Flicker , Owner.mainBodyChunk, false, 1.5f, 1f);

        Bombs.Remove(bns.Key);
    }

    public void BombStraightToBelt(ScavengerBomb bomb)
    {
        Owner.room.PlaySound(SoundID.Overseer_Image_Big_Flicker, Owner.mainBodyChunk, false, 1.5f, 0.6f);

        bomb.ChangeMode(Weapon.Mode.OnBack);
        bomb.CollideWithObjects = false;
        Bombs.Add(bomb, new AbstractBombStick(Owner.abstractPhysicalObject, bomb.abstractPhysicalObject));
    }

    #region Helpers

    public void Lock()
    {
        counter = 0;
        locked = true;
    }

    public bool IsFull()
    {
        return Bombs.Count >= Capacity;
    }

    public bool ReadyToAddBomb()
    {
        return BombInMainHand(out _) && !IsFull();
    }

    public bool BombInMainHand(out ScavengerBomb bomb)
    {
        if (Owner.grasps[0]?.grabbed is ScavengerBomb bomba)
        {
            bomb = bomba;
            return true;

        }
        bomb = null;
        return false;
    }

    public bool HasAFreeHand(out int index)
    {
        for (var i = 0; i < Owner.grasps.Length; i++)
        {
            if (Owner.grasps[i] == null)
            {
                index = i;
                return true;
            }
        }

        index = NoAvailableGrasps;
        return false;
    }

    #endregion


    //Graphics
    public void GraphicsModuleUpdated(bool actuallyviewed, bool eu)
    {
        if (!Bombs.Any())
            return;

        var graphics = (PlayerGraphics)Owner.graphicsModule;

        var bodyZeroPos = graphics.drawPositions[0, 0]; //Higher body chunk
        var bodyOnePos = graphics.drawPositions[1, 0]; //Lower body chunk

        var startPos = new Vector2(12f, 0f);
        var stopPos = new Vector2(-12f, -14f);

        var dirUp = (bodyZeroPos - bodyOnePos).normalized; //Get reference direction

        startPos = Custom.RotateAroundOrigo(startPos, Custom.VecToDeg(dirUp)); //Rotate positions accordingly
        stopPos = Custom.RotateAroundOrigo(stopPos, Custom.VecToDeg(dirUp));

        startPos = bodyZeroPos + startPos; //Parent chunk position + offset we want
        stopPos = bodyZeroPos + stopPos;

        var path = (stopPos - startPos); //Path A -> B (startPos -> stopPos)


        for (var i = 0; i < Bombs.Count; i++)
        {
            var x = (i + 1f) / (Bombs.Count + 1f); //Get fraction of path (position) we want to put the bomb at
            var pos =  startPos + path * x; //Start position + path * the fraction

            var bomb = Bombs.Keys.ElementAt(i);
            bomb.firstChunk.MoveFromOutsideMyUpdate(eu, pos);
            bomb.firstChunk.vel = Owner.mainBodyChunk.vel;
            bomb.rotationSpeed = 0f;
        }
    }
}