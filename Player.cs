using System;
using System.Collections.Generic;
using System.Linq;
using Menu.Remix.MixedUI;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MoreSlugcats;
using RWCustom;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Sainot;

public partial class Sainot
{
    private void PlayerOnctor(On.Player.orig_ctor orig, Player self, AbstractCreature abstractcreature, World world)
    {
        orig(self, abstractcreature, world);

        if (IsBeltEnabled(self))
        {
            var belt = new BombBelt(self);
            if (StartWithBombs)
            {
                for (var i = 0; i < belt.Capacity; i++)
                {
                    var newBomb = new ScavengerBomb(new AbstractPhysicalObject(world, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, null, self.room.GetWorldCoordinate(self.mainBodyChunk.pos), self.room.game.GetNewID()), world);
                    belt.BombStraightToBelt(newBomb);
                }
            }

            new HeadRag(self);
        }
    }

    private void PlayerOnGrabUpdate(On.Player.orig_GrabUpdate orig, Player self, bool eu)
    {
        orig(self, eu);

        if (IsBeltEnabled(self))
        {
            var belt = BombBelt.GetBelt(self);
            belt.Increment = self.input[0].pckp;

            if (self.FoodInStomach < self.MaxFoodInStomach && self.grasps[0]?.grabbed is IPlayerEdible)
            {
                belt.Lock();
            }

            belt.Update();
        }
    }

    private void PlayerILGrabUpdate(ILContext il)
    {
        try
        {
            var c = new ILCursor(il);
            ILLabel label = null;

            //Saint stop eating batflies
            c.GotoNext(i => i.MatchCallOrCallvirt<Player>("BiteEdibleObject"));
            c.GotoPrev(i => i.MatchLdfld<Player>("SlugCatClass"));
            c.GotoPrev(MoveType.After, i => i.MatchBrfalse(out label));

            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate((Player self) =>
            {
                if (IsBeltEnabled(self))
                {
                    var belt = BombBelt.GetBelt(self);
                    if (belt.ReadyToAddBomb())
                    {
                        return false;
                    }

                    if (self.grasps[0]?.grabbed != null)
                        belt.Lock();
                }

                return true;
            });
            c.Emit(OpCodes.Brfalse, label);

            //Stop eating in general
            c.GotoNext(i => i.MatchCallOrCallvirt<Player>("BiteEdibleObject"));
            c.GotoPrev(MoveType.After, i => i.MatchBge(out label));
            c.GotoNext(i => i.MatchCallOrCallvirt<Player>("BiteEdibleObject"));
            c.GotoPrev(MoveType.Before, i => i.MatchLdarg(0));

            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate((Player self) =>
            {
                if (IsBeltEnabled(self))
                {
                    var belt = BombBelt.GetBelt(self);
                    if (belt.ReadyToAddBomb())
                    {
                        return false;
                    }

                    if (self.grasps[0]?.grabbed != null)
                        belt.Lock();
                }

                return true;
            });
            c.Emit(OpCodes.Brfalse, label);

            //Bomb straight to belt
            c.GotoNext(i => i.MatchLdsfld<SoundID>("Slugcat_Switch_Hands_Init"));
            c.GotoPrev(i => i.MatchLdfld<Player>("pickUpCandidate"));
            c.GotoPrev(MoveType.Before, i => i.MatchLdarg(0));
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate((Player self) =>
            {
                if (IsBeltEnabled(self))
                {
                    var belt = BombBelt.GetBelt(self);

                    if (self.wantToPickUp > 0 && self.pickUpCandidate is ScavengerBomb bombCandidate)
                    {
                        if (!belt.HasAFreeHand(out _) && !belt.IsFull())
                        {
                            Debug.Log("Bomb straight to belt");
                            belt.Lock();
                            belt.BombStraightToBelt(bombCandidate);
                        }
                    }
                }
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
            throw;
        }
    }


    private void PlayerOnUpdate(On.Player.orig_Update orig, Player self, bool eu)
    {
        orig(self, eu);
    }

    #region Graphics Hooks

    private void PlayerOnGraphicsModuleUpdated(On.Player.orig_GraphicsModuleUpdated orig, Player self, bool actuallyviewed, bool eu)
    {
        if (IsBeltEnabled(self))
        {
            BombBelt.GetBelt(self).GraphicsModuleUpdated(actuallyviewed, eu);
        }

        orig(self, actuallyviewed, eu);
    }

    private void PlayerGraphicsOnUpdate(On.PlayerGraphics.orig_Update orig, PlayerGraphics self)
    {
        orig(self);
        if (IsBeltEnabled(self.player))
        {
            HeadRag.GetHeadRag(self.player).GraphicsUpdate(self);
        }
    }

    private void PlayerGraphicsOnInitiateSprites(On.PlayerGraphics.orig_InitiateSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sleaser, RoomCamera rcam)
    {
        orig(self, sleaser, rcam);

        if (IsBeltEnabled(self.player))
        {
            HeadRag.GetHeadRag(self.player).InitiateSprites(self, sleaser, rcam);
        }
    }

    private void PlayerGraphicsOnDrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sleaser, RoomCamera rcam, float timestacker, Vector2 campos)
    {
        orig(self, sleaser, rcam, timestacker, campos);

        if (IsBeltEnabled(self.player))
        {
            HeadRag.GetHeadRag(self.player).DrawSprites(self, sleaser, rcam, timestacker, campos);
        }
    }
    
    private void PlayerGraphicsOnAddToContainer(On.PlayerGraphics.orig_AddToContainer orig, PlayerGraphics self, RoomCamera.SpriteLeaser sleaser, RoomCamera rcam, FContainer newcontatiner)
    {
        orig(self, sleaser, rcam, newcontatiner);

        if (IsBeltEnabled(self.player))
        {
            var headRag = HeadRag.GetHeadRag(self.player);
            if (headRag.IsInit)
            {
                headRag.AddToContainer(self, sleaser, rcam, newcontatiner);
            }
            
        }
    }

    private void PlayerOnNewRoom(On.Player.orig_NewRoom orig, Player self, Room newroom)
    {
        orig(self, newroom);

        if (IsBeltEnabled(self))
        {
            HeadRag.GetHeadRag(self).NewRoom(self, newroom);
        }
    }

    #endregion

    public static List<HeadRag> HeadRags = new List<HeadRag>();

    //It gets a bit messy from down here)
    public class HeadRag
    {
        public Player Owner;

        //public Vector2[,] rag = new Vector2[4, 6];
        //public Vector2[,] rag2 = new Vector2[4, 6];
        public Vector2[,] rag = new Vector2[Random.Range(5, Random.Range(5, 7)), 6];
        public Vector2[,] rag2 = new Vector2[Random.Range(5, Random.Range(5, 7)), 6];
        public float R = 0.8f;
        public float G = 0.05f;
        public float B = 0.04f;
        public Color ragColor; //= new Color(0.8f, 0.05f, 0.04f);
        public Color rag2Color; //= new Color(0.65f, 0.035f, 0.02f);
        public SharedPhysics.TerrainCollisionData scratchTerrainCollisionData = new SharedPhysics.TerrainCollisionData();
        public Vector2 lastRotation;
        public int SpriteIndex; //Index we start adding sprites at

        public bool IsInit;

        public HeadRag(Player self)
        {
            var treshold = 0.15f;
            Owner = self;
            ragColor = new Color(R, G, B);
            rag2Color = new Color(R > treshold ? R - treshold : R * 0.5f, G > treshold ? G - treshold : G * 0.5f, B > treshold ? B - treshold : B * 0.5f);
            HeadRags.Add(this);
        }

        public static HeadRag GetHeadRag(Player owner)
        {
            return HeadRags.FirstOrDefault(x => x.Owner == owner);
        }

        public void GraphicsUpdate(PlayerGraphics self)
        {
            var player = self.player;
            var conRad = 7f;

            #region rag1

            for (int i = 0; i < rag.GetLength(0); i++)
            {
                float t = (float)i / (float)(rag.GetLength(0) - 1);
                rag[i, 1] = rag[i, 0];
                rag[i, 0] += rag[i, 2];
                rag[i, 2] -= player.firstChunk.Rotation * Mathf.InverseLerp(1f, 0f, (float)i) * 0.8f;
                rag[i, 4] = rag[i, 3];
                rag[i, 3] = (rag[i, 3] + rag[i, 5] * Custom.LerpMap(Vector2.Distance(rag[i, 0], rag[i, 1]), 1f, 18f, 0.05f, 0.3f)).normalized;
                rag[i, 5] = (rag[i, 5] + Custom.RNV() * Random.value * Mathf.Pow(Mathf.InverseLerp(1f, 18f, Vector2.Distance(rag[i, 0], rag[i, 1])), 0.3f)).normalized;
                if (player.room.PointSubmerged(rag[i, 0]))
                {
                    rag[i, 2] *= Custom.LerpMap(rag[i, 2].magnitude, 1f, 10f, 1f, 0.5f, Mathf.Lerp(1.4f, 0.4f, t));
                    rag[i, 2].y += 0.05f;
                    rag[i, 2] += Custom.RNV() * 0.1f;
                }
                else
                {
                    rag[i, 2] *= Custom.LerpMap(Vector2.Distance(rag[i, 0], rag[i, 1]), 1f, 6f, 0.999f, 0.7f, Mathf.Lerp(1.5f, 0.5f, t));
                    rag[i, 2].y -= player.room.gravity * Custom.LerpMap(Vector2.Distance(rag[i, 0], rag[i, 1]), 1f, 6f, 0.6f, 0f);
                    if (i % 3 == 2 || i == rag.GetLength(0) - 1)
                    {
                        SharedPhysics.TerrainCollisionData terrainCollisionData = scratchTerrainCollisionData.Set(rag[i, 0], rag[i, 1], rag[i, 2], 1f, new IntVector2(0, 0), false);
                        terrainCollisionData = SharedPhysics.HorizontalCollision(player.room, terrainCollisionData);
                        terrainCollisionData = SharedPhysics.VerticalCollision(player.room, terrainCollisionData);
                        terrainCollisionData = SharedPhysics.SlopesVertically(player.room, terrainCollisionData);
                        rag[i, 0] = terrainCollisionData.pos;
                        rag[i, 2] = terrainCollisionData.vel;
                        if (terrainCollisionData.contactPoint.x != 0)
                        {
                            rag[i, 2].y *= 0.6f;
                        }

                        if (terrainCollisionData.contactPoint.y != 0)
                        {
                            rag[i, 2].x *= 0.6f;
                        }
                    }
                }
            }

            for (int j = 0; j < rag.GetLength(0); j++)
            {
                if (j > 0)
                {
                    Vector2 normalized = (rag[j, 0] - rag[j - 1, 0]).normalized;
                    float num = Vector2.Distance(rag[j, 0], rag[j - 1, 0]);
                    float d = (num > conRad) ? 0.5f : 0.25f;
                    rag[j, 0] += normalized * (conRad - num) * d;
                    rag[j, 2] += normalized * (conRad - num) * d;
                    rag[j - 1, 0] -= normalized * (conRad - num) * d;
                    rag[j - 1, 2] -= normalized * (conRad - num) * d;
                    if (j > 1)
                    {
                        normalized = (rag[j, 0] - rag[j - 2, 0]).normalized;
                        rag[j, 2] += normalized * 0.2f;
                        rag[j - 2, 2] -= normalized * 0.2f;
                    }

                    if (j < rag.GetLength(0) - 1)
                    {
                        rag[j, 3] = Vector3.Slerp(rag[j, 3], (rag[j - 1, 3] * 2f + rag[j + 1, 3]) / 3f, 0.1f);
                        rag[j, 5] = Vector3.Slerp(rag[j, 5], (rag[j - 1, 5] * 2f + rag[j + 1, 5]) / 3f, Custom.LerpMap(Vector2.Distance(rag[j, 1], rag[j, 0]), 1f, 8f, 0.05f, 0.5f));
                    }
                }
                else
                {
                    rag[j, 0] = RagAttachPos(1f, self);
                    rag[j, 2] *= 0f;
                }
            }

            #endregion

            #region rag2

            for (int i = 0; i < rag2.GetLength(0); i++)
            {
                float t = (float)i / (float)(rag2.GetLength(0) - 1);
                rag2[i, 1] = rag2[i, 0];
                rag2[i, 0] += rag2[i, 2];
                rag2[i, 2] -= player.firstChunk.Rotation * Mathf.InverseLerp(1f, 0f, (float)i) * 0.8f;
                rag2[i, 4] = rag2[i, 3];
                rag2[i, 3] = (rag2[i, 3] + rag2[i, 5] * Custom.LerpMap(Vector2.Distance(rag2[i, 0], rag2[i, 1]), 1f, 18f, 0.05f, 0.3f)).normalized;
                rag2[i, 5] = (rag2[i, 5] + Custom.RNV() * Random.value * Mathf.Pow(Mathf.InverseLerp(1f, 18f, Vector2.Distance(rag2[i, 0], rag2[i, 1])), 0.3f)).normalized;
                if (player.room.PointSubmerged(rag2[i, 0]))
                {
                    rag2[i, 2] *= Custom.LerpMap(rag2[i, 2].magnitude, 1f, 10f, 1f, 0.5f, Mathf.Lerp(1.4f, 0.4f, t));
                    rag2[i, 2].y += 0.05f;
                    rag2[i, 2] += Custom.RNV() * 0.1f;
                }
                else
                {
                    rag2[i, 2] *= Custom.LerpMap(Vector2.Distance(rag2[i, 0], rag2[i, 1]), 1f, 6f, 0.999f, 0.7f, Mathf.Lerp(1.5f, 0.5f, t));
                    rag2[i, 2].y -= player.room.gravity * Custom.LerpMap(Vector2.Distance(rag2[i, 0], rag2[i, 1]), 1f, 6f, 0.6f, 0f);
                    if (i % 3 == 2 || i == rag2.GetLength(0) - 1)
                    {
                        SharedPhysics.TerrainCollisionData terrainCollisionData = scratchTerrainCollisionData.Set(rag2[i, 0], rag2[i, 1], rag2[i, 2], 1f, new IntVector2(0, 0), false);
                        terrainCollisionData = SharedPhysics.HorizontalCollision(player.room, terrainCollisionData);
                        terrainCollisionData = SharedPhysics.VerticalCollision(player.room, terrainCollisionData);
                        terrainCollisionData = SharedPhysics.SlopesVertically(player.room, terrainCollisionData);
                        rag2[i, 0] = terrainCollisionData.pos;
                        rag2[i, 2] = terrainCollisionData.vel;
                        if (terrainCollisionData.contactPoint.x != 0)
                        {
                            rag2[i, 2].y *= 0.6f;
                        }

                        if (terrainCollisionData.contactPoint.y != 0)
                        {
                            rag2[i, 2].x *= 0.6f;
                        }
                    }
                }
            }

            for (int j = 0; j < rag2.GetLength(0); j++)
            {
                if (j > 0)
                {
                    Vector2 normalized = (rag2[j, 0] - rag2[j - 1, 0]).normalized;
                    float num = Vector2.Distance(rag2[j, 0], rag2[j - 1, 0]);
                    float d = (num > conRad) ? 0.5f : 0.25f;
                    rag2[j, 0] += normalized * (conRad - num) * d;
                    rag2[j, 2] += normalized * (conRad - num) * d;
                    rag2[j - 1, 0] -= normalized * (conRad - num) * d;
                    rag2[j - 1, 2] -= normalized * (conRad - num) * d;
                    if (j > 1)
                    {
                        normalized = (rag2[j, 0] - rag2[j - 2, 0]).normalized;
                        rag2[j, 2] += normalized * 0.2f;
                        rag2[j - 2, 2] -= normalized * 0.2f;
                    }

                    if (j < rag2.GetLength(0) - 1)
                    {
                        rag2[j, 3] = Vector3.Slerp(rag2[j, 3], (rag2[j - 1, 3] * 2f + rag2[j + 1, 3]) / 3f, 0.1f);
                        rag2[j, 5] = Vector3.Slerp(rag2[j, 5], (rag2[j - 1, 5] * 2f + rag2[j + 1, 5]) / 3f, Custom.LerpMap(Vector2.Distance(rag2[j, 1], rag2[j, 0]), 1f, 8f, 0.05f, 0.5f));
                    }
                }
                else
                {
                    rag2[j, 0] = Rag2AttachPos(1f, self);
                    rag2[j, 2] *= 0f;
                }
            }

            #endregion

            lastRotation = self.head.connection.Rotation;
        }

        public void InitiateSprites(PlayerGraphics self, RoomCamera.SpriteLeaser sleaser, RoomCamera rcam)
        {
            SpriteIndex = sleaser.sprites.Length;
            Array.Resize(ref sleaser.sprites, SpriteIndex + 3); //+ number of sprites we want to add

            var trgMesh = TriangleMesh.MakeLongMesh(rag.GetLength(0), false, false); //Headband strips
            var trgMesh2 = TriangleMesh.MakeLongMesh(rag2.GetLength(0), false, false);
            var headBandFront = new FSprite("Symbol_Rock", false);

            trgMesh.color = ragColor;
            trgMesh.shader = rcam.game.rainWorld.Shaders["JaggedSquare"];
            trgMesh2.color = rag2Color;
            trgMesh2.shader = rcam.game.rainWorld.Shaders["JaggedSquare"];
            headBandFront.color = ragColor;
            headBandFront.SetPosition(self.head.pos);

            sleaser.sprites[SpriteIndex] = trgMesh;
            sleaser.sprites[SpriteIndex + 1] = trgMesh2;
            sleaser.sprites[SpriteIndex + 2] = headBandFront;

            AddToContainer(self, sleaser, rcam, null);
            
            IsInit = true;
        }

        public void DrawSprites(PlayerGraphics self, RoomCamera.SpriteLeaser sleaser, RoomCamera rcam, float timestacker, Vector2 campos)
        {
            var isCrawl = self.player.bodyMode == Player.BodyModeIndex.Crawl || self.player.bodyMode == Player.BodyModeIndex.CorridorClimb;

            var headPos = Vector2.Lerp(self.head.lastPos, self.head.pos, timestacker);
            var bodyZeroPos = Vector2.Lerp(self.drawPositions[0, 1], self.drawPositions[0, 0], timestacker);
            var bodyOnePos = Vector2.Lerp(self.drawPositions[1, 1], self.drawPositions[1, 0], timestacker);

            var newPos = new Vector2(0f, 3.5f);

            var dirUp = (bodyZeroPos - bodyOnePos).normalized;

            newPos = Custom.RotateAroundOrigo(newPos, Custom.VecToDeg(dirUp));
            newPos = headPos + newPos + new Vector2(isCrawl ? self.player.flipDirection == 1 ? -4f : 4f : 0f, isCrawl ? 3f : 0f);

            sleaser.sprites[SpriteIndex + 2].SetPosition(newPos - campos);
            sleaser.sprites[SpriteIndex + 2].rotation = isCrawl ? 0f : Custom.VecToDeg(self.head.connection.Rotation);
            sleaser.sprites[SpriteIndex + 2].color = ragColor;
            sleaser.sprites[SpriteIndex + 2].scaleY = isCrawl ? 0.4f : 0.5f;
            sleaser.sprites[SpriteIndex + 2].scaleX = isCrawl ? 1.0f : 1.3f;

            #region rag1

            sleaser.sprites[SpriteIndex].color = ragColor;

            var num = 0f;
            var a = RagAttachPos(timestacker, self);
            for (var i = 0; i < this.rag.GetLength(0); i++)
            {
                var f = (float)i / (float)(this.rag.GetLength(0) - 1);
                var vector = Vector2.Lerp(this.rag[i, 1], this.rag[i, 0], timestacker);
                var num2 = (2f + 2f * Mathf.Sin(Mathf.Pow(f, 2f) * 3.1415927f)) * Vector3.Slerp(this.rag[i, 4], this.rag[i, 3], timestacker).x;
                var normalized = (a - vector).normalized;
                var a2 = Custom.PerpendicularVector(normalized);
                var d = Vector2.Distance(a, vector) / 5f;
                (sleaser.sprites[SpriteIndex] as TriangleMesh).MoveVertice(i * 4, a - normalized * d - a2 * (num2 + num) * 0.5f - campos);
                (sleaser.sprites[SpriteIndex] as TriangleMesh).MoveVertice(i * 4 + 1, a - normalized * d + a2 * (num2 + num) * 0.5f - campos);
                (sleaser.sprites[SpriteIndex] as TriangleMesh).MoveVertice(i * 4 + 2, vector + normalized * d - a2 * num2 - campos);
                (sleaser.sprites[SpriteIndex] as TriangleMesh).MoveVertice(i * 4 + 3, vector + normalized * d + a2 * num2 - campos);
                a = vector;
                num = num2;
            }

            #endregion

            #region rag2

            sleaser.sprites[SpriteIndex + 1].color = rag2Color;

            var num22 = 0f;
            var a22 = Rag2AttachPos(timestacker, self);
            for (var i = 0; i < this.rag2.GetLength(0); i++)
            {
                var f = (float)i / (float)(this.rag2.GetLength(0) - 1);
                var vector = Vector2.Lerp(this.rag2[i, 1], this.rag2[i, 0], timestacker);
                var num2 = (2f + 2f * Mathf.Sin(Mathf.Pow(f, 2f) * 3.1415927f)) * Vector3.Slerp(this.rag2[i, 4], this.rag2[i, 3], timestacker).x;
                var normalized = (a22 - vector).normalized;
                var a2 = Custom.PerpendicularVector(normalized);
                var d = Vector2.Distance(a22, vector) / 5f;
                (sleaser.sprites[SpriteIndex + 1] as TriangleMesh).MoveVertice(i * 4, a22 - normalized * d - a2 * (num2 + num22) * 0.5f - campos);
                (sleaser.sprites[SpriteIndex + 1] as TriangleMesh).MoveVertice(i * 4 + 1, a22 - normalized * d + a2 * (num2 + num22) * 0.5f - campos);
                (sleaser.sprites[SpriteIndex + 1] as TriangleMesh).MoveVertice(i * 4 + 2, vector + normalized * d - a2 * num2 - campos);
                (sleaser.sprites[SpriteIndex + 1] as TriangleMesh).MoveVertice(i * 4 + 3, vector + normalized * d + a2 * num2 - campos);
                a22 = vector;
                num22 = num2;
            }

            #endregion
        }

        public void AddToContainer(PlayerGraphics self, RoomCamera.SpriteLeaser sleaser, RoomCamera rcam, FContainer newcontatiner)
        {
            if (newcontatiner == null)
            {
                newcontatiner = rcam.ReturnFContainer("Midground");
            }

            newcontatiner.AddChild(sleaser.sprites[SpriteIndex]);
            newcontatiner.AddChild(sleaser.sprites[SpriteIndex + 1]);
            newcontatiner.AddChild(sleaser.sprites[SpriteIndex + 2]);
            sleaser.sprites[SpriteIndex].MoveBehindOtherNode(sleaser.sprites[0]);
            sleaser.sprites[SpriteIndex + 1].MoveBehindOtherNode(sleaser.sprites[0]);
            sleaser.sprites[SpriteIndex + 2].MoveBehindOtherNode(sleaser.sprites[9]);
        }

        public Vector2 RagAttachPos(float timeStacker, PlayerGraphics self)
        {
            return Vector2.Lerp(self.head.lastPos + new Vector2(-5f, 0f), self.head.pos + new Vector2(-5f, 0f), timeStacker) + Vector3.Slerp(lastRotation, self.head.connection.Rotation, timeStacker).ToVector2InPoints() * 15f;
        }

        public Vector2 Rag2AttachPos(float timeStacker, PlayerGraphics self)
        {
            return Vector2.Lerp(self.head.lastPos + new Vector2(5f, 0f), self.head.pos + new Vector2(5f, 0f), timeStacker) + Vector3.Slerp(lastRotation, self.head.connection.Rotation, timeStacker).ToVector2InPoints() * 15f;
        }


        public void NewRoom(Player self, Room newroom)
        {
            IsInit = false;
            
            var graphics = (PlayerGraphics)self.graphicsModule;

            #region rag

            var vector = RagAttachPos(1f, graphics);
            for (var i = 0; i < rag.GetLength(0); i++)
            {
                rag[i, 0] = vector;
                rag[i, 1] = vector;
                rag[i, 2] *= 0f;
            }

            #endregion

            #region rag2

            var vector2 = Rag2AttachPos(1f, graphics);
            for (var i = 0; i < rag2.GetLength(0); i++)
            {
                rag2[i, 0] = vector2;
                rag2[i, 1] = vector2;
                rag2[i, 2] *= 0f;
            }

            #endregion
        }
    }
}