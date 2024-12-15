using DSL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityModManagerNet;

namespace TestMod
{
    public class Main
    {
        public static string health;
        public static string ammo;

        static bool Load(UnityModManager.ModEntry modEntry)
        {
            modEntry.OnGUI = OnGUI;
            //modEntry.OnUpdate = OnUpdate;
            return true;
        }


        static void OnGUI(UnityModManager.ModEntry modEntry)
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
