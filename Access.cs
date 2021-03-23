using Kingmaker.Visual.CharacterSystem;
using System.Collections.Generic;
using UnityEngine;

namespace RainbowMod
{
    public static class Access
    {

        public static Harmony12.AccessTools.FieldRef<EquipmentEntity, List<Texture2D>> PrimaryRamps;
        public static void Init()
        {
            PrimaryRamps = Harmony12.AccessTools.FieldRefAccess<EquipmentEntity, List<Texture2D>>("m_PrimaryRamps");
        }
    }
}