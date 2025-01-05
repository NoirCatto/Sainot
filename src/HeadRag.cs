using UnityEngine;

namespace Sainot;

public class HeadRag
{
    public readonly PlayerData Data;
    public Player Owner => Data.Owner;

    //public Vector2[,] rag = new Vector2[4, 6];
    //public Vector2[,] rag2 = new Vector2[4, 6];
    public readonly bool EasterEgg;
    public Vector2[,] RagSize;
    public Vector2[,] Rag2Size;
    public float R = 0.8f;
    public float G = 0.05f;
    public float B = 0.04f;
    public Color RagColor; //= new Color(0.8f, 0.05f, 0.04f);
    public Color Rag2Color; //= new Color(0.65f, 0.035f, 0.02f);
    public Color[] CustomColors;
    public Color[] Custom2Colors;
    public SharedPhysics.TerrainCollisionData ScratchTerrainCollisionData = new SharedPhysics.TerrainCollisionData();
    public Vector2 LastRotation;

    public int[] SpriteIndex = new int[3];

    public HeadRag(PlayerData data)
    {
        Data = data;
        
        var treshold = 0.15f;
        EasterEgg = Random.Range(1, 80) == 1;
        RagSize = new Vector2[EasterEgg ? Random.Range(15, Random.Range(15, 28)) : Random.Range(5, Random.Range(5, 7)), 6];
        Rag2Size =  new Vector2[EasterEgg ? Random.Range(15, Random.Range(15, 28)) : Random.Range(5, Random.Range(5, 7)), 6];
        RagColor = !EasterEgg ? new Color(R, G, B) : new Color(0.1f, 0.2f, 0.8f);
        Rag2Color = new Color(R > treshold ? R - treshold : R * 0.5f, G > treshold ? G - treshold : G * 0.5f, B > treshold ? B - treshold : B * 0.5f);
    }

    public Vector2 RagAttachPos(float timeStacker, PlayerGraphics self)
    {
        return Vector2.Lerp(self.head.lastPos + new Vector2(-5f, 0f), self.head.pos + new Vector2(-5f, 0f), timeStacker) + Vector3.Slerp(LastRotation, self.head.connection.Rotation, timeStacker).ToVector2InPoints() * 15f;
    }

    public Vector2 Rag2AttachPos(float timeStacker, PlayerGraphics self)
    {
        return Vector2.Lerp(self.head.lastPos + new Vector2(5f, 0f), self.head.pos + new Vector2(5f, 0f), timeStacker) + Vector3.Slerp(LastRotation, self.head.connection.Rotation, timeStacker).ToVector2InPoints() * 15f;
    }
}