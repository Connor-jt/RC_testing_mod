using DSL;
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
            if (cached_balancing_obj == null){
                FieldInfo myField = typeof(EntityBalancingStore).GetField("_entityBalancingScriptableObject", BindingFlags.NonPublic | BindingFlags.Static);
                if (myField != null)
                    cached_balancing_obj = (EntityBalancingScriptableObject)myField.GetValue(null);
                else return;
            }
            else count = cached_balancing_obj.parameters.Count;
            

            GUILayout.Label($"test mod, cards: {count}");
            if (GUILayout.Button("Add Units!!"))
            {
                GUILayout.Label("click test");
            }
        }
    }
}
