
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


        [HarmonyPatch(typeof(EntityBalancingStore), "SetScriptableObject", new Type[] { typeof(EntityBalancingScriptableObject) })]
        private static class Patch 
        {
            private static void Prefix(EntityBalancingScriptableObject entity){
                Melon<Moddy>.Logger.Msg("hook reached!!");
                // The code inside this method will run before 'SetScriptableObject()' is executed

                // copied from the actual function, because we should always block if true
                if (EntityBalancingStore._entityBalancingScriptableObject != null)
                    return;

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

                        entity.parameters.AddRange(bundle_units.parameters);
                        foreach (EntityBalancingParameters curr_entity in bundle_units.parameters){
                            // open localisation files, and process them via the same method as the game
                            TextAsset blueprint_name = (TextAsset)bundle.LoadAsset("Localization - BlueprintName");
                            TextAsset blueprint_desc = (TextAsset)bundle.LoadAsset("Localization - BlueprintDescription");
                            TextAsset skill_desc = (TextAsset)bundle.LoadAsset("Localization - SkillDescription");
                            // we may need resource re-routing for this (where we hijack the resource loading to accept asset bundles)

                            // skip processing the localization files, just fill in blank according to the custom entity id's
                            Loca.BlueprintNameDictionary["en-US"][curr_entity.entityId] = curr_entity.entityId;
                            Loca.BlueprintDescriptionDictionary["en-US"][curr_entity.entityId] = curr_entity.entityId;
                            Loca.SkillDescriptionDictionary["en-US"][curr_entity.entityId] = curr_entity.entityId;



                            Melon<Moddy>.Logger.Msg("bundle unit processing success: " + curr_entity.entityId);
                        }
                    } catch (Exception ex){
                        Melon<Moddy>.Logger.Msg("bundle unit processing exception: " + ex);
                    }
                }

            }

            //private static void Postfix(EntityBalancingScriptableObject entity)
            //{
            //    // The code inside this method will run after 'PrivateMethod' has executed
            //}
        }





        const string export_folder = "C:\\Users\\Joe bingle\\Downloads\\RC modding\\exports\\";

        bool has_exported = false;
        public override void OnInitializeMelon()
        {

        }

        public void rebuild_entity_store()
        {
            EntityBalancingScriptableObject thing = EntityBalancingStore._entityBalancingScriptableObject;
            EntityBalancingStore._entityBalancingScriptableObject = null;

            EntityBalancingStore.ChangeableIntValueCache.Clear();
            EntityBalancingStore.ChangeableFloatValueCache.Clear();
            EntityBalancingStore.SetScriptableObject(thing);
            Melon<Moddy>.Logger.Msg("rebuilt entities cache!!");
        }

        public void load_mod_bundles()
        {
            // make sure our thing is loaded
            var entities_store = EntityBalancingStore._entityBalancingScriptableObject;
            if (entities_store == null){
                Melon<Moddy>.Logger.Msg("entity store not loaded yet!!");
                return;
            }


            // locate folder with all asset bundles
            List<AssetBundle> loaded_bundles = new List<AssetBundle>();
            string assets_folder = "C:\\Users\\Joe bingle\\Downloads\\RC modding\\mods";
            foreach (string file in Directory.GetFiles(assets_folder))
            {
                try
                {
                    AssetBundle mod_bundle = AssetBundle.LoadFromFile(file);
                    if (mod_bundle != null) loaded_bundles.Add(mod_bundle);
                }
                catch (Exception ex)
                {
                    Melon<Moddy>.Logger.Msg("bundle opening exception: " + ex);
                }
            }

            // foreach asset bundle
            foreach (AssetBundle bundle in loaded_bundles)
            {
                try
                {
                    // open the 'units' scriptable object, append structures to entity list
                    EntityBalancingScriptableObject bundle_units = (EntityBalancingScriptableObject)bundle.LoadAsset("Units");
                    if (bundle_units == null)
                    {
                        Melon<Moddy>.Logger.Msg("bundle unit processing exception: entity balancing obj is null");
                        continue;
                    }

                    entities_store.parameters.AddRange(bundle_units.parameters);

                    foreach (EntityBalancingParameters entity in bundle_units.parameters)
                    {
                        // open localisation files, and process them via the same method as the game
                        TextAsset blueprint_name = (TextAsset)bundle.LoadAsset("Localization - BlueprintName");
                        TextAsset blueprint_desc = (TextAsset)bundle.LoadAsset("Localization - BlueprintDescription");
                        TextAsset skill_desc = (TextAsset)bundle.LoadAsset("Localization - SkillDescription");
                        // we may need resource re-routing for this (where we hijack the resource loading to accept asset bundles)

                        // skip processing the localization files, just fill in blank according to the custom entity id's
                        Loca.BlueprintNameDictionary["en-US"][entity.entityId] = entity.entityId;
                        Loca.BlueprintDescriptionDictionary["en-US"][entity.entityId] = entity.entityId;
                        Loca.SkillDescriptionDictionary["en-US"][entity.entityId] = entity.entityId;



                        Melon<Moddy>.Logger.Msg("bundle unit processing success: " + entity.entityId);
                    }
                }
                catch (Exception ex)
                {
                    Melon<Moddy>.Logger.Msg("bundle unit processing exception: " + ex);
                }
            }
        }


        public override void OnGUI(){
            if (has_exported) return;


            long count = -1;
            var test = EntityBalancingStore._entityBalancingScriptableObject;
            if (test != null) count = test.parameters.Count;
            
            // show unit count, if failed then dont bother showing any of the other UI
            GUILayout.Label($"test moddy, cards: {count}");
            if (test == null) return;

            if (GUILayout.Button("add basic custom unit")){
                test.parameters.Add(ExampleUnit.get_unit);
                rebuild_entity_store();
            }

            if (GUILayout.Button("import custom units from mods")){
                load_mod_bundles();
                rebuild_entity_store();
            }

            if (GUILayout.Button("unlock enemy units")){
                List<EntityBalancingParameters> added_entities = new List<EntityBalancingParameters>();
                foreach (var item in test.parameters){
                    // if entity is a building spawner, 
                    if ((item.roles & UnitRole.Factory)  != UnitRole.None 
                    &&  (item.roles & UnitRole.Building) != UnitRole.None
                    &&  (item.roles & UnitRole.PCXCard)  != UnitRole.None
                    &&  item.isAllowedForAi){
                        // then we duplicate the struct and make it a friendly card??
                        EntityBalancingParameters converted_unit = item;

                        converted_unit.roles &= ~UnitRole.PCXCard; // clear PCXCard role
                        converted_unit.isAllowedForAi = false;
                        converted_unit.isAllowedAsBlueprint = true;

                        added_entities.Add(converted_unit);
                    }
                }
                // then loop back and add all the new units in
                foreach (var item in added_entities)
                    test.parameters.Add(item);
                rebuild_entity_store();
            }

            if (GUILayout.Button("Export units")){
                has_exported = true;

                JsonSerializer serializer = new JsonSerializer();
                serializer.Converters.Add(new JavaScriptDateTimeConverter());
                serializer.NullValueHandling = NullValueHandling.Ignore;
                // Create the file.

                using (StreamWriter sw = new StreamWriter(export_folder + "units.txt"))
                using (JsonWriter writer = new JsonTextWriter(sw)){

                    writer.Formatting = Formatting.Indented;
                    //using (FileStream fs = File.Create(export_folder + "units.txt")){
                    int index = 0;
                    sw.Write("{");
                    foreach (var item in test.parameters){
                        if (index > 0) sw.Write(",");
                        sw.Write(" " + index + "\":\n");
                        //string serialized_unit = Newtonsoft.Json.JsonConvert.SerializeObject(item);
                        //fs.Write(Encoding.UTF8.GetBytes(serialized_unit), 0, serialized_unit.Length);

                        serializer.Serialize(writer, item);
                        index++;
                    }
                    sw.Write("\n}");
                }
                //}

            }

            


        //List<string> list = new List<string>();
        //    foreach (EntityBalancingParameters entityBalancingParameters in EntityBalancingStore._entityBalancingScriptableObject.parameters)
        //    {
        //        if (!entityBalancingParameters.inactive 
        //            && entityBalancingParameters.isAllowedAsBlueprint)
        //        {
        //            list.Add(entityBalancingParameters.entityId);
        //            NullableString factoryForEntityId = entityBalancingParameters.factoryForEntityId;
        //            if (factoryForEntityId.hasValue)
        //            {
        //                list.Add(factoryForEntityId.value);
        //            }
        //        }
        //    }
        
        }
    }
}
