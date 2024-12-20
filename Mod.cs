
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using MelonLoader;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using TestMod.CustomUnits;
using System.Runtime.InteropServices.ComTypes;
using HarmonyLib;

namespace TestMod{
    public class Moddy : MelonMod{
        public static List<string> entities_to_localize = new List<string>();
        public static Dictionary<string, AssetBundle> mod_bundles = new Dictionary<string, AssetBundle>();

        [HarmonyPatch(typeof(EntityBalancingStore), "SetScriptableObject", new Type[] { typeof(EntityBalancingScriptableObject) })]
        private static class UnitPatch {
            public static void Prefix(EntityBalancingScriptableObject entity){
                // The code inside this method will run before 'SetScriptableObject()' is executed
                // copied from the actual function, because we should always block if true
                if (EntityBalancingStore._entityBalancingScriptableObject != null)
                    return;

                Melon<Moddy>.Logger.Msg("hook reached!!");
                // locate folder with all asset bundles
                List<AssetBundle> loaded_bundles = new List<AssetBundle>();
                string assets_folder = "C:\\Users\\Joe bingle\\Downloads\\RC modding\\mods";
                foreach (string file in Directory.GetFiles(assets_folder)){
                    try{
                        AssetBundle mod_bundle = AssetBundle.LoadFromFile(file);
                        if (mod_bundle != null) loaded_bundles.Add(mod_bundle);
                    }catch (Exception ex){
                        Melon<Moddy>.Logger.Msg("bundle opening exception: " + ex);
                    }
                }
                // foreach asset bundle
                foreach (AssetBundle bundle in loaded_bundles){
                    try{
                        // open the 'units' scriptable object, append structures to entity list
                        EntityBalancingScriptableObject bundle_units = (EntityBalancingScriptableObject)bundle.LoadAsset("Units");
                        if (bundle_units == null){
                            Melon<Moddy>.Logger.Msg("bundle unit processing exception: entity balancing obj is null");
                            continue;
                        }

                        foreach (EntityBalancingParameters curr_entity in bundle_units.parameters){
                            // open localisation files, and process them via the same method as the game
                            TextAsset blueprint_name = (TextAsset)bundle.LoadAsset("Localization - BlueprintName");
                            TextAsset blueprint_desc = (TextAsset)bundle.LoadAsset("Localization - BlueprintDescription");
                            TextAsset skill_desc = (TextAsset)bundle.LoadAsset("Localization - SkillDescription");
                            // we may need resource re-routing for this (where we hijack the resource loading to accept asset bundles)

                            // since the localizations haven't actually loaded yet, just set aside the info for later
                            entities_to_localize.Add(curr_entity.entityId);

                            Melon<Moddy>.Logger.Msg("bundle unit processing success: " + curr_entity.entityId);
                        }
                        entity.parameters.AddRange(bundle_units.parameters);
                        mod_bundles.Add(bundle.name, bundle);

                        // temp thing to prevent entityid overlap
                        //foreach (var item in bundle_units.parameters)
                        //{
                        //    for (int i = 0; i < entity.parameters.Count; i++)
                        //    {
                        //        if (entity.parameters[i].entityId == item.entityId)
                        //        {
                        //            entity.parameters.RemoveAt(i);
                        //            entity.parameters.Add(item);
                        //            Melon<Moddy>.Logger.Msg("replaced entity: " + item.entityId);
                        //        }
                        //    }
                        //}
                        

                    } catch (Exception ex){
                        Melon<Moddy>.Logger.Msg("bundle unit processing exception: " + ex);
                    }
                }

            }

        }


        //[HarmonyPatch(typeof(Loca), "Init")]
        //private static class LocalizationPatch{
        //    private static void Postfix(){
        //        Melon<Moddy>.Logger.Msg("updating localizations");

        //        foreach (string item in entities_to_localize){
        //            Loca.BlueprintNameDictionary["en-US"][item] = "placeholder" + item;
        //            Loca.BlueprintDescriptionDictionary["en-US"][item] = "placeholder" + item;
        //            //Loca.SkillDescriptionDictionary["en-US"][item] = item;

        //            Melon<Moddy>.Logger.Msg("bundle unit localized: " + item);
        //        }

        //    }
        //}


        public static UnityEngine.Object load_from_bundle_or_resource(string path){
            if (path.Length > 0 && path[0] == '<'){
                string[] strings = path.Split(new char[] { '>' });

                string asset_bundle = strings[0].Substring(1);
                string asset_path = strings[1];

                if (mod_bundles.TryGetValue(asset_bundle, out AssetBundle bundle))
                {
                    Melon<Moddy>.Logger.Msg("successfully loadaed: " + asset_path + " from bundle: " + asset_bundle);
                    return bundle.LoadAsset(asset_path);
                }
                else
                {
                    Melon<Moddy>.Logger.Msg("CRITICAL FAILURE!!!! bundle not found: " + asset_bundle);
                    return null;
                }

            } else return Resources.Load(path);
        }


        [HarmonyPatch(typeof(EntityFactory), "InstantiateEntity", new Type[] { typeof(string), typeof(Vector3), typeof(EntityController), typeof(string), typeof(string), typeof(Transform), typeof(UnitRole), typeof(bool) })]
        private static class EntityFactoryPatch1{
            public static bool Prefix(ref EntityController __result, string entityId, Vector3 position, EntityController originEntity, string tag, string name, Transform parentTransform, UnitRole additionalRoles, bool hasBeenCalledFromAbove){

                Melon<Moddy>.Logger.Msg("entity instantiation hook: " + entityId);

                string path = EntityBalancingStore.PrefabLocation(entityId);
                GameObject gameObject = (((object)parentTransform == null) ? ((GameObject)UnityEngine.Object.Instantiate(load_from_bundle_or_resource(path), position, Quaternion.identity))
                                                                           : ((GameObject)UnityEngine.Object.Instantiate(load_from_bundle_or_resource(path), position, Quaternion.identity, parentTransform)));
                if (name != "") gameObject.name = name;
                if (!string.IsNullOrEmpty(tag)) gameObject.tag = tag;
                EntityController component = gameObject.GetComponent<EntityController>();
                component.Init(originEntity);
                component.SetUniqueEntityId(EntityFactory.NextUniqueEntityId());
                component.AddRoles(additionalRoles);
                gameObject.SetActive(value: true);
                if (Tags.IsPlayer(tag)){
                    component.ReplaceMaterial(EntityFactory.PlayerMaterial);
                    component.SetHealthBarColor(GameBalancingStore.PlayerHealthBarColor, GameBalancingStore.PlayerHealthBarBatteredColor);
                    if (Game.CardUpgrades.TryGetValue(entityId, out var value))
                    {
                        foreach (string item in value)
                            component.AddUpgrade(item);
                    }

                } else if (Tags.IsAi(tag)){
                    component.ReplaceMaterial(component.IsBuilding ? EntityFactory.AiBuildingMaterial : EntityFactory.AiMaterial);
                    component.SetHealthBarColor(GameBalancingStore.AiHealthBarColor, GameBalancingStore.AiHealthBarBatteredColor);
                    if (!component.HasRole(UnitRole.Spawn) && component.IsBuilding)
                        EntityFactory.AiBuildingCount++;
                }
                component.OnHasBeenInstantiated(hasBeenCalledFromAbove);
                __result = component;

                return false;
            }
        }

        [HarmonyPatch(typeof(EntityFactory), "CreateBuildingPlacementGhost", new Type[] { typeof(string), typeof(bool) })]
        private static class EntityFactoryPatch2{
            public static bool Prefix(ref GameObject __result, string entityId, bool returnInactive){
                Melon<Moddy>.Logger.Msg("ghost building placement hook: " + entityId);

                GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(load_from_bundle_or_resource(EntityBalancingStore.PrefabLocation(entityId)));
                EntityController component = gameObject.GetComponent<EntityController>();
                EntityController.RangeToVisualize visualizeRangeWhileSelected = component.visualizeRangeWhileSelected;
                UnityEngine.Object.Destroy(component);
                UnityEngine.Object.Destroy(gameObject.GetComponent<EntityBaseParameters>());
                UnityEngine.Object.Destroy(gameObject.GetComponent<BoxCollider>());
                AudioSource[] components = gameObject.GetComponents<AudioSource>();
                for (int i = 0; i < components.Length; i++)
                    UnityEngine.Object.Destroy(components[i]);

                UnityEngine.Object.Destroy(gameObject.transform.Find("FogOfWarVisibleArea").gameObject);
                UnityEngine.Object.Destroy(gameObject.transform.Find("MinimapShape").gameObject);
                if (visualizeRangeWhileSelected != 0){
                    GameObject gameObject2 = UnityEngine.Object.Instantiate(EntityFactory.RangeCirclePrefab, gameObject.transform, worldPositionStays: true);
                    gameObject2.name = "PlacementGhostRangeCircle";
                    float num;
                    switch (visualizeRangeWhileSelected){
                        case EntityController.RangeToVisualize.No:
                            num = 0f;
                            break;
                        case EntityController.RangeToVisualize.SkillRange:
                            num = (float)EntityBalancingStore.SkillRange(entityId);
                            break;
                        case EntityController.RangeToVisualize.WeaponRange:
                            num = EntityBalancingStore.WeaponRange(entityId, false);
                            break;
                        case EntityController.RangeToVisualize.SightRange:
                            num = (float)EntityBalancingStore.SightRadius(entityId, false);
                            break;
                        case EntityController.RangeToVisualize.EffectRadius1:
                            num = EntityBalancingStore.EffectRadius1(entityId);
                            break;
                        case EntityController.RangeToVisualize.EffectRadius2:
                            num = EntityBalancingStore.EffectRadius2(entityId);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    gameObject2.transform.localScale = 10f * num * Vector3.one;
                }
                if (!returnInactive)
                {
                    gameObject.SetActive(value: true);
                }
                __result = gameObject;
                return false;
            }
        }

        [HarmonyPatch(typeof(EntityFactory), "CreateEntityMesh", new Type[] { typeof(string), typeof(Transform), typeof(float) })]
        private static class EntityFactoryPatch3{
            public static bool Prefix(GameObject __result, string entityId, Transform parentTransform, float scaleMultiplier = 1f){
                Melon<Moddy>.Logger.Msg("entity mesh creation hook: " + entityId);

		        string text = EntityBalancingStore.PrefabLocation(entityId);
		        UnityEngine.Object @object = load_from_bundle_or_resource(text);
		        if (@object == null){
			        Logger.Warn("Cannot find prefab (" + text + ") for entityId: " + entityId);
			        __result = null;
                    return false;
		        }
		        GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(@object, parentTransform);
		        gameObject.SetLayerRecursively(22, 24);
		        gameObject.SetActive(value: true);
		        if (Math.Abs(scaleMultiplier - 1f) > 1E-06f)
			        gameObject.transform.localScale *= scaleMultiplier;

		        UnityEngine.Object.Destroy(gameObject.GetComponent<EntityController>());
		        UnityEngine.Object.Destroy(gameObject.GetComponent<EntityBaseParameters>());
		        UnityEngine.Object.Destroy(gameObject.GetComponent<BoxCollider>());
		        AudioSource[] components = gameObject.GetComponents<AudioSource>();
		        for (int i = 0; i < components.Length; i++)
                    UnityEngine.Object.Destroy(components[i]);
		        UnityEngine.Object.Destroy(gameObject.GetComponentInChildren<ScaleByChangeableValue>());


		        Transform transform = gameObject.transform.Find("UnitSpawnedEffect");
		        if ((bool)transform) UnityEngine.Object.Destroy(transform.gameObject);

		        Transform transform2 = gameObject.transform.Find("BarCanvases");
		        if ((bool)transform2) UnityEngine.Object.Destroy(transform2.gameObject);

		        Transform transform3 = gameObject.transform.Find("SelectionCircles");
		        if ((bool)transform3) UnityEngine.Object.Destroy(transform3.gameObject);

		        Transform transform4 = gameObject.transform.Find("MinimapShape");
		        if ((bool)transform4) UnityEngine.Object.Destroy(transform4.gameObject);

			    __result = gameObject;
                return false;
	        }
        }



        //const string export_folder = "C:\\Users\\Joe bingle\\Downloads\\RC modding\\exports\\";
        //bool has_exported = false;
        //public override void OnInitializeMelon()
        //{
        //}
        //public override void OnGUI(){
        //    if (has_exported) return;


        //    long count = -1;
        //    var test = EntityBalancingStore._entityBalancingScriptableObject;
        //    if (test != null) count = test.parameters.Count;
            
        //    // show unit count, if failed then dont bother showing any of the other UI
        //    GUILayout.Label($"test moddy, cards: {count}");
        //    if (test == null) return;


        //    if (GUILayout.Button("unlock enemy units")){
        //        List<EntityBalancingParameters> added_entities = new List<EntityBalancingParameters>();
        //        foreach (var item in test.parameters){
        //            // if entity is a building spawner, 
        //            if ((item.roles & UnitRole.Factory)  != UnitRole.None 
        //            &&  (item.roles & UnitRole.Building) != UnitRole.None
        //            &&  (item.roles & UnitRole.PCXCard)  != UnitRole.None
        //            &&  item.isAllowedForAi){
        //                // then we duplicate the struct and make it a friendly card??
        //                EntityBalancingParameters converted_unit = item;

        //                converted_unit.roles &= ~UnitRole.PCXCard; // clear PCXCard role
        //                converted_unit.isAllowedForAi = false;
        //                converted_unit.isAllowedAsBlueprint = true;

        //                added_entities.Add(converted_unit);
        //            }
        //        }
        //        // then loop back and add all the new units in
        //        foreach (var item in added_entities)
        //            test.parameters.Add(item);
        //    }

        //    if (GUILayout.Button("Export units")){
        //        has_exported = true;

        //        JsonSerializer serializer = new JsonSerializer();
        //        serializer.Converters.Add(new JavaScriptDateTimeConverter());
        //        serializer.NullValueHandling = NullValueHandling.Ignore;
        //        // Create the file.

        //        using (StreamWriter sw = new StreamWriter(export_folder + "units.txt"))
        //        using (JsonWriter writer = new JsonTextWriter(sw)){

        //            writer.Formatting = Formatting.Indented;
        //            //using (FileStream fs = File.Create(export_folder + "units.txt")){
        //            int index = 0;
        //            sw.Write("{");
        //            foreach (var item in test.parameters){
        //                if (index > 0) sw.Write(",");
        //                sw.Write(" " + index + "\":\n");
        //                //string serialized_unit = Newtonsoft.Json.JsonConvert.SerializeObject(item);
        //                //fs.Write(Encoding.UTF8.GetBytes(serialized_unit), 0, serialized_unit.Length);

        //                serializer.Serialize(writer, item);
        //                index++;
        //            }
        //            sw.Write("\n}");
        //        }
        //        //}

        //    }
        //}
    }
}
