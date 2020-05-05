using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;

namespace FuseTools.Test {
    public class Utils {
        public static bool Click(string ObjectName) {
            var obj = GameObject.Find(ObjectName);
            if (obj == null) {
                // Assert.Fail("Could not find GameObject to click: "+ObjectName);
                return false;
            }

            return Click(obj);
        }

        public static bool Click(GameObject obj) {
            var btn = obj.GetComponent<UnityEngine.UI.Button>();
         
            if (btn != null) {
                btn.onClick.Invoke();
                return true;
            }
         
            btn = obj.GetComponentInChildren<UnityEngine.UI.Button>();
         
            if (btn != null)
            {
                btn.onClick.Invoke();
                return true;
            }
         
            // Assert.Fail("Could not find clickable component on GameObject: "+obj.name);
            return false;
        }

        public static string GetText(string objId) {
            var obj = GameObject.Find(objId);
            return obj == null ? null : GetText(obj);
        }

        public static string GetText(GameObject obj) {
            var txt = obj.GetComponentInChildren<UnityEngine.UI.Text>();
            if (txt == null) return null;
            return txt.text;
        }


        public static bool InputText(string id, string txt, bool invoke=true) {
            return InputText(GameObject.Find(id), txt, invoke);
        }

        public static bool InputText(GameObject obj, string text, bool invokeChangeEvent = true) {
            if (obj == null) return false;

            var input = obj.GetComponent<UnityEngine.UI.InputField>();

            if (input == null) return false;

            input.text = text;
            if (invokeChangeEvent) input.onValueChanged.Invoke(text);
            return true;
        }
    }
}