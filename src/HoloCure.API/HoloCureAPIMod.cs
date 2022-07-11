using DogScepterLib.Core;
using DogScepterLib.Core.Chunks;
using DogScepterLib.Core.Models;
using HoloCure.ModLoader.API;

namespace HoloCure.API
{
    public class HoloCureAPIMod : Mod
    {
        public override void PatchGame(GMData gameData) {
            GMString? versStr = gameData.GetChunk<GMChunkSTRG>().List.Find(x => x.Content.Contains("version "));

            if (versStr is not null) versStr.Content = "HoloCure.API vTEST\n" + versStr.Content;
        }
    }
}