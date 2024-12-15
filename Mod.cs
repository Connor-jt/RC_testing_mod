
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

namespace TestMod{
    public class Moddy : MelonMod
    {



        const string export_folder = "C:\\Users\\Joe bingle\\Downloads\\RC modding\\exports\\";

        bool has_exported = false;
        public override void OnInitializeMelon()
        {

        }


        public override void OnGUI(){
            if (has_exported) return;


            long count = -1;
            var test = EntityBalancingStore._entityBalancingScriptableObject;
            if (test != null) count = test.parameters.Count;
            
            // show unit count, if failed then dont bother showing any of the other UI
            GUILayout.Label($"test moddy, cards: {count}");
            if (test == null) return;

            if (GUILayout.Button("add custom unit")){
                test.parameters.Add(ExampleUnit.get_unit);
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
