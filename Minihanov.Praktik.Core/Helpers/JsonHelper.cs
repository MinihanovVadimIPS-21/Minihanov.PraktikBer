using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Minihanov.Praktik.Core.Helpers
{
    public static class JsonHelper
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        public static void SaveToFile<T>(T data, string filePath)
        {
            var json = JsonSerializer.Serialize(data, Options);
            File.WriteAllText(filePath, json);
        }

        public static T LoadFromFile<T>(string filePath)
        {
            if (!File.Exists(filePath))
                return default;

            var json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<T>(json, Options);
        }
    }
}