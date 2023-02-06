using System.Collections.Generic;
using System.Linq;
using On.MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace Sainot;

public partial class Sainot
{
    public static List<BombBelt> BombBelts = new List<BombBelt>();

    public class BombBelt {
        
        public Player Owner;

        public Dictionary<ScavengerBomb, AbstractBombStick> Bombs;

        public int Capacity = BeltsCapacity;

        public bool Increment;

        private int counter;

        private bool locked;
        
        public BombBelt(Player owner)
        {
            Owner = owner;
            Bombs = new Dictionary<ScavengerBomb, AbstractBombStick>();
            BombBelts.Add(this);
        }

        public void GraphicsModuleUpdated(bool actuallyViewed, bool eu)
        {
            if (!Bombs.Any())
                return;

            var graphics = (PlayerGraphics)Owner.graphicsModule;
            
            var bodyZeroPos = graphics.drawPositions[0, 0]; //Higher body chunk
            var bodyOnePos = graphics.drawPositions[1, 0]; //Lower body chunk
            var headPos = graphics.head.pos;

            var startPos = new Vector2(12f, 0f);
            var stopPos = new Vector2(-12f, -14f);

            var dirUp = (bodyZeroPos - bodyOnePos).normalized; //Get reference direction
            
            startPos = Custom.RotateAroundOrigo(startPos, Custom.VecToDeg(dirUp)); //Rotate positions accordingly
            stopPos = Custom.RotateAroundOrigo(stopPos, Custom.VecToDeg(dirUp));

            startPos = bodyZeroPos + startPos; //Parent chunk position + offset we want
            stopPos = bodyZeroPos + stopPos;

            var path = (stopPos - startPos); //Path A -> B (startPost -> stopPos)


            for (var i = 0; i < Bombs.Count; i++)
            {
                var x = (i + 1f) / (Bombs.Count + 1f); //Get fraction of path (position) we want to put the bomb at
                var pos =  startPos + path * x; //Start position + path * the fraction
                
                Bombs.Keys.ElementAt(i).firstChunk.MoveFromOutsideMyUpdate(eu, pos);
                Bombs.Keys.ElementAt(i).firstChunk.vel = Owner.mainBodyChunk.vel;
                Bombs.Keys.ElementAt(i).rotationSpeed = 0f;
            }
        }

        public void Update(bool increment)
        {
            Increment = increment;
            this.Update();
        }
        
        public void Update()
        {
            if (Increment)
            {
                if (locked) 
                    return;
                
                counter++;
                if (!IsFull() && counter > 20 && BombInMainHand(out var bomb))
                {
                    BombToBelt(bomb);
                    locked = true;
                    counter = 0;
                }
                else if (Bombs.Any() && counter > 20 && HasAFreeHand(out var index))
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
            //Debug.Log("Putting bomb to belt");
            Owner.ReleaseGrasp(0);
            Owner.room.PlaySound(SoundID.Overseer_Image_Big_Flicker, Owner.mainBodyChunk, false, 1.5f, 0.6f);
            
            bomb.ChangeMode(Weapon.Mode.OnBack);
            bomb.CollideWithObjects = false;
            Bombs.Add(bomb, new AbstractBombStick(Owner.abstractPhysicalObject, bomb.abstractPhysicalObject));
        }

        public void BombToHand(int index)
        {
            //Debug.Log("Retrieving bomb from belt");
            if (index == NoAvailableGrasps)
                return;
            
            Bombs.First().Value.Deactivate();
            Bombs.First().Key.CollideWithObjects = true;
            
            Owner.SlugcatGrab(Bombs.First().Key, index);
            Owner.room.PlaySound(SoundID.Overseer_Image_Big_Flicker , Owner.mainBodyChunk, false, 1.5f, 1f);

            Bombs.Remove(Bombs.First().Key);
        }

        public void BombStraightToBelt(ScavengerBomb bomb)
        {
            Owner.room.PlaySound(SoundID.Overseer_Image_Big_Flicker, Owner.mainBodyChunk, false, 1.5f, 0.6f);
            
            bomb.ChangeMode(Weapon.Mode.OnBack);
            bomb.CollideWithObjects = false;
            Bombs.Add(bomb, new AbstractBombStick(Owner.abstractPhysicalObject, bomb.abstractPhysicalObject));
        }

        #region Helpers
        
        public static BombBelt GetBelt(Player owner)
        {
            return BombBelts.FirstOrDefault(x => x.Owner == owner);
        }

        public void Lock()
        {
            this.Increment = false;
            this.locked = true;
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

        public const int NoAvailableGrasps = -1;
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
    }
    
}