using System.Runtime.CompilerServices;
using MoreSlugcats;

namespace Sainot;

public static class PlayerCWT
{
    public static readonly ConditionalWeakTable<Player, PlayerData> PlayerDeets = new ConditionalWeakTable<Player, PlayerData>();
    public static PlayerData GetData(this Player player) => PlayerDeets.GetValue(player, _ => new(player));
    public static bool TryGetData(this Player player, out PlayerData playerData)
    {
        if (player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Saint && Sainot.Rambo)
        {
            playerData = GetData(player);
            return true;
        }

        playerData = null;
        return false;
    }
}

public class PlayerData //Rambo only
{
    public readonly Player Owner;
    public HeadRag Rag;

    public int TotalSprites;
    public int NewSprites;

    public bool CallingAddToContainerFromOrigInitiateSprites;

    public PlayerData(Player owner)
    {
        Owner = owner;
        Rag = new HeadRag(this);
        NewSprites = Rag.SpriteIndex.Length;
    }
}