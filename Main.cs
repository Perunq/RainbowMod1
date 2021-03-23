using UnityEngine;
using UnityModManagerNet;
using static UnityModManagerNet.UnityModManager;
using System;
using System.Reflection;
using System.IO;
using Kingmaker.Visual.CharacterSystem;
using Kingmaker.ResourceLinks;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Items.Equipment;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Linq;
using Kingmaker.Blueprints.Classes;
using UnityEngine.SceneManagement;
using Kingmaker;
using UnityEngine.UI;

namespace RainbowMod
{
    public class Main
    {
        public static bool enabled;
        public static ModEntry modEntry;
        public static HashSet<String> eeLookupHair = new HashSet<string>();
        public static HashSet<String> eeLookupSkin = new HashSet<string>();
        static Dictionary<string, Texture2D> textureLookup = new Dictionary<string, Texture2D>();
        private static bool loaded;


        public static Texture2D LoadTexture(string filePath)
        {
            if (textureLookup.ContainsKey(filePath))
            {
                return textureLookup[filePath];
            }
            else
            {
                var fileData = File.ReadAllBytes(filePath);
                var texture = new Texture2D(2, 2);
                texture.LoadImage(fileData);
                textureLookup[filePath] = texture;
                return texture;
            }
        }

        public static void Log(string text)
        {
            modEntry.Logger.Log(text);
        }
        public static void Error(string text)
        {
            modEntry.Logger.Log(text);
        }

        public static void Error(Exception exception)
        {
            modEntry.Logger.Log(exception.ToString());
        }
        public static bool IsHairEE(string assetId)
        {
            return eeLookupHair.Contains(assetId);
        }

        static void AddHair(EquipmentEntityLink[] Options)
        {
            foreach (var link in Options)
            {
                if (!eeLookupHair.Contains(link.AssetId))
                {
                    eeLookupHair.Add(link.AssetId);
                }
            }
        }

        static void AddRace(BlueprintRace race)
        {

            AddHair(race.FemaleOptions.Hair);
            AddHair(race.FemaleOptions.Eyebrows);
            AddHair(race.MaleOptions.Hair);
            AddHair(race.MaleOptions.Beards);
            AddHair(race.MaleOptions.Eyebrows);
        }

        private static void CreateLookup(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == SceneName.MainMenu)
            {
                if (Main.loaded)
                {
                    return;
                }
                foreach (BlueprintRace race in Main.GetAllRaces())
                {
                    AddRace(race);
                }
                Main.loaded = true;

                if (!Main.enabled)
                {
                    return;
                }
            }
        }


        private static BlueprintRace[] GetAllRaces()
        {
            return Game.Instance.BlueprintRoot.Progression.CharacterRaces;
        }



        static bool Load(ModEntry modEntry)
        {
            Main.modEntry = modEntry;
            try
            {
                Access.Init();
                var harmony = Harmony12.HarmonyInstance.Create(modEntry.Info.Id);
                SceneManager.sceneLoaded += Main.CreateLookup;
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            catch (Exception ex)
            {
                Error(ex);
                return false;
            }
            return true;
        }


        [Harmony12.HarmonyPatch(typeof(AssetBundle), "LoadFromFile", new Type[] { typeof(string) })]
        static class AssetBundle_LoadFromFilePatch
        {
            static void Postfix(string path, ref AssetBundle __result)
            {
                try
                {
                    var assetId = Path.GetFileName(path).Replace("resource_", "");

                    if (IsHairEE(assetId))
                    {
                        var ee = __result.LoadAllAssets<EquipmentEntity>()[0];
                        if (ee.ColorsProfile != null)
                        {
                            AddTexture(ee, LoadTexture(modEntry.Path + @"\NewTextures\NCHairTex1.png"));
                            AddTexture(ee, LoadTexture(modEntry.Path + @"\NewTextures\NCHairTex2.png"));
                            AddTexture(ee, LoadTexture(modEntry.Path + @"\NewTextures\NCHairTex3.png"));
                        }
                    }

                }
                catch (Exception ex)
                {
                    Main.Error(ex);
                }
            }
        }

        public static void AddTexture(EquipmentEntity ee, Texture2D Tex)
        {

            if (!ee.ColorsProfile.PrimaryRamps.Contains(Tex))
            {
                ee.ColorsProfile.PrimaryRamps.Add(Tex);
            }

        }
    }
}