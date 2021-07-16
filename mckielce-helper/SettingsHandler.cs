using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace mckielce_helper {
    public static class SettingsHandler {

        private static string appdataFile;
        private static string defaultsFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "defaults.json");

        public static void InitSettings() {
            try {
                string appdataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "mckielce_helper");
                if (!Directory.Exists(appdataDir)) Directory.CreateDirectory(appdataDir);
                appdataFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "mckielce_helper", "settings.json");
                if (File.Exists(appdataFile)) return;
                File.Copy(defaultsFile, appdataFile);
            }catch(Exception ex) {
                Console.WriteLine(ex.ToString());
            }
        }

        public static object Get(string key) {
            JObject settings = JObject.Parse(File.ReadAllText(appdataFile));
            Console.WriteLine(key);
            if (settings.ContainsKey(key)) {
                return ((JValue)settings[key]).Value;
            } else {
                JObject defaults = JObject.Parse(File.ReadAllText(defaultsFile));
                if (defaults.ContainsKey(key)) {
                    return ((JValue)defaults[key]).Value;
                } else {
                    return false;
                }
            }
        }

        public static void Set(string key, int data) {
            JObject settings = JObject.Parse(File.ReadAllText(appdataFile));
            settings[key] = data;
            File.WriteAllText(appdataFile, JsonConvert.SerializeObject(settings));
        }

        public static void Set(string key, string data) {
            JObject settings = JObject.Parse(File.ReadAllText(appdataFile));
            settings[key] = data;
            File.WriteAllText(appdataFile, JsonConvert.SerializeObject(settings));
        }

        public static void Set(string key, bool data) {
            JObject settings = JObject.Parse(File.ReadAllText(appdataFile));
            settings[key] = data;
            File.WriteAllText(appdataFile, JsonConvert.SerializeObject(settings));
        }

    }
}
