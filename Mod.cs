using DSL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using MelonLoader;

namespace TestMod
{
    public class Moddy : MelonMod
    {

        public override void OnInitializeMelon()
        {

        }


        public override void OnGUI()
        {

            long count = -1;
            var test = EntityBalancingStore._entityBalancingScriptableObject;
            if (test != null)
            {
                count = test.parameters.Count;
            }

            GUILayout.Label($"test mod, cards: {count}");
            if (GUILayout.Button("Add Units!!"))
            {
                GUILayout.Label("click test");
            }
        }
    }
}
