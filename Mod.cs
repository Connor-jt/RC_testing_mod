
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

namespace TestMod{
    public class Moddy : MelonMod
    {
        const string export_folder = "C:\\Users\\Joe bingle\\Downloads\\RC modding\\exports\\";

        bool has_exported = false;
        EntityBalancingScriptableObject cached_balancing_obj = null;
        public override void OnInitializeMelon()
        {

        }


        public override void OnGUI(){
            if (has_exported) return;


            long count = -1;
            // working reflection version
            //if (cached_balancing_obj == null){
            //    FieldInfo myField = typeof(EntityBalancingStore).GetField("_entityBalancingScriptableObject", BindingFlags.NonPublic | BindingFlags.Static);
            //    if (myField != null)
            //        cached_balancing_obj = (EntityBalancingScriptableObject)myField.GetValue(null);
            //    else return;
            //} else count = cached_balancing_obj.parameters.Count;

            // not working publicized version 
            var test = EntityBalancingStore._entityBalancingScriptableObject;
            if (test != null)
                count = test.parameters.Count;
            


            GUILayout.Label($"test moddy, cards: {count}");
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
        }
    }
}
