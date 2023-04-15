using UnityEngine;

using Newtonsoft.Json;
using System.Text.RegularExpressions;

public class JsonHelper {

    

    public static T[] getJsonArray<T>(string json) {
        string newJson = "{ \"array\": " + json + "}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
        return wrapper.array;
    }

    public static string ToJson<T>(T[] array) {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.array = array;

       // string json = JsonUtility.ToJson(wrapper);

        string json = JsonConvert.SerializeObject(wrapper, new JsonSerializerSettings {
            NullValueHandling = NullValueHandling.Include
        });

        json = json.Length < 10 ? "[]" : json.Substring(10, json.Length - 12);

        

        return json;
    }

    [System.Serializable]
    private class Wrapper<T> {
        public T[] array;
    }
}
