using System.IO;
using System.Web.Script.Serialization;

namespace BreakFishApp.Utils
{
    public static class JsonFile
    {
        public static T Load<T>(string path) where T : class, new()
        {
            if (!File.Exists(path))
            {
                return new T();
            }

            var json = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(json))
            {
                return new T();
            }

            var serializer = new JavaScriptSerializer();
            var result = serializer.Deserialize<T>(json);
            return result ?? new T();
        }

        public static void Save<T>(string path, T value)
        {
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var serializer = new JavaScriptSerializer();
            File.WriteAllText(path, serializer.Serialize(value));
        }
    }
}
