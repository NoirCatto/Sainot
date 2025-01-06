using UnityEngine;

namespace Sainot;

public class HeadRag
{
    public readonly PlayerData Data;
    public Player Owner => Data.Owner;

    public bool UseRainbow => Sainot.ModOptions.HeadRagUseColor.Value && Sainot.ModOptions.HeadRagColorRainbow.Value;
    public Vector2[][,] RagSize = new Vector2[2][,];
    public float R = 0.8f;
    public float G = 0.05f;
    public float B = 0.04f;
    public Color[] RagColor = new Color[2];
    public Color[][] CustomColors = new Color[2][];
    public SharedPhysics.TerrainCollisionData ScratchTerrainCollisionData = new SharedPhysics.TerrainCollisionData();
    public Vector2 LastRotation;

    public int[] SpriteIndex = new int[3];

    public HeadRag(PlayerData data)
    {
        Data = data;

        if (Sainot.ModOptions.HeadRagUseColor.Value)
        {
            var col = Sainot.ModOptions.HeadRagColor.Value;
            R = col.r;
            G = col.g;
            B = col.b;
        }

        var treshold = 0.15f;
        var minLength = Sainot.ModOptions.HeadRagLength.Value;
        var maxLength = Mathf.CeilToInt(minLength * 1.8f);
        RagSize[0] = new Vector2[Random.Range(minLength, Random.Range(minLength, maxLength)), 6];
        RagSize[1] = new Vector2[Random.Range(minLength, Random.Range(minLength, maxLength)), 6];
        RagColor[0] = new Color(R, G, B);
        RagColor[1] = new Color(R > treshold ? R - treshold : R * 0.5f, G > treshold ? G - treshold : G * 0.5f, B > treshold ? B - treshold : B * 0.5f);
    }

    public Vector2 RagAttachPos(int i, float timeStacker, PlayerGraphics self)
    {
        return Vector2.Lerp(self.head.lastPos + new Vector2(i == 0 ? -5f : 5f, 0f), self.head.pos + new Vector2(i == 0 ? -5f : 5f, 0f), timeStacker) + Vector3.Slerp(LastRotation, self.head.connection.Rotation, timeStacker).ToVector2InPoints() * 15f;
    }
}