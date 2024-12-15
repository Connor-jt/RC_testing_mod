
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using MelonLoader;

namespace TestMod{
    public class Moddy : MelonMod
    {
        EntityBalancingScriptableObject cached_balancing_obj = null;
        public override void OnInitializeMelon()
        {

        }


        public override void OnGUI(){

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
            if (GUILayout.Button("Add Units!!"))
            {
            }
        }
    }
}
