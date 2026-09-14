using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text.Json.Serialization;
using Xenon.Assets;
using Xenon.Context;
using Xenon.NodeSystem;

namespace Xenon.ContentSystem.Classes
{
    public class XenonSceneNode
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("type")]
        public string Type { get; set; } = "";

        [JsonPropertyName("properties")]
        public Dictionary<string, string> Properties { get; set; } = new();

        [JsonPropertyName("children")]
        public List<XenonSceneNode> Children { get; set; } = new();

        public Node BuildNode()
        {
            Type targetType = ResolveNodeType(Type);
            if (targetType == null)
            {
                XEN.Logger.LogError($"Could not resolve type '{Type}'. Falling back to Node base.", "SceneLoader");
                targetType = typeof(Node);
            }

            Node nodeInstance;
            try
            {
                nodeInstance = (Node)Activator.CreateInstance(targetType);
            }
            catch (Exception ex)
            {
                XEN.Logger.LogError($"Failed to instantiate '{Type}': {ex.Message}", "SceneLoader");
                nodeInstance = new Node();
            }

            if (!string.IsNullOrEmpty(Name))
            {
                nodeInstance.Name = Name;
            }

            ApplyProperties(nodeInstance);

            foreach (XenonSceneNode childData in Children)
            {
                Node childNode = childData.BuildNode();
                if (childNode != null)
                {
                    nodeInstance.AddChild(childNode);
                }
            }

            return nodeInstance;
        }

        private Type ResolveNodeType(string typePath)
        {
            if (string.IsNullOrWhiteSpace(typePath)) return null;

            // Direct Type lookup
            Type resolvedType = System.Type.GetType(typePath);
            if (resolvedType != null) return resolvedType;

            // Assembly-wide lookup using fully qualified type name
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                resolvedType = assembly.GetType(typePath);
                if (resolvedType != null) return resolvedType;
            }

            return null;
        }

        private void ApplyProperties(Node targetInstance)
        {
            if (Properties == null) return;

            Type targetType = targetInstance.GetType();

            foreach (var propKvp in Properties)
            {
                PropertyInfo prop = targetType.GetProperty(
                    propKvp.Key,
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

                if (prop != null && prop.CanWrite)
                {
                    try
                    {
                        object convertedVal = ParseProperty(propKvp.Value, prop.PropertyType);
                        if (convertedVal != null)
                        {
                            prop.SetValue(targetInstance, convertedVal);
                        }
                    }
                    catch (Exception ex)
                    {
                        XEN.Logger.LogError($"Assigning property '{propKvp.Key}' on '{Name}': {ex.Message}", "SceneLoader");
                    }
                }
            }
        }

        private object ParseProperty(string rawVal, Type targetType)
        {
            if (string.IsNullOrWhiteSpace(rawVal)) return null;

            if (targetType == typeof(string)) return rawVal;
            if (targetType == typeof(float)) return float.Parse(rawVal, CultureInfo.InvariantCulture);
            if (targetType == typeof(double)) return double.Parse(rawVal, CultureInfo.InvariantCulture);
            if (targetType == typeof(int)) return int.Parse(rawVal, CultureInfo.InvariantCulture);
            if (targetType == typeof(bool)) return bool.Parse(rawVal);

            // Vector Parsing
            if (targetType == typeof(Vector3))
            {
                var parts = rawVal.Trim('(', ')', ' ').Split(',').Select(p => float.Parse(p.Trim(), CultureInfo.InvariantCulture)).ToArray();
                return new Vector3(parts[0], parts[1], parts[2]);
            }

            if (targetType == typeof(Vector2))
            {
                var parts = rawVal.Trim('(', ')', ' ').Split(',').Select(p => float.Parse(p.Trim(), CultureInfo.InvariantCulture)).ToArray();
                return new Vector2(parts[0], parts[1]);
            }

            if (targetType == typeof(Quaternion))
            {
                var parts = rawVal.Trim('(', ')', ' ').Split(',').Select(p => float.Parse(p.Trim(), CultureInfo.InvariantCulture)).ToArray();
                return parts.Length == 4
                    ? new Quaternion(parts[0], parts[1], parts[2], parts[3])
                    : Quaternion.CreateFromYawPitchRoll(
                        MathF.PI / 180f * parts[0],
                        MathF.PI / 180f * parts[1],
                        MathF.PI / 180f * parts[2]);
            }

            // Engine Asset System Bindings
            try
            {
                if (typeof(Mesh).IsAssignableFrom(targetType))
                {
                    return Content.Load<Mesh>(rawVal);
                }

                if (typeof(Veldrid.Texture).IsAssignableFrom(targetType))
                {
                    return Content.Load<Veldrid.Texture>(rawVal);
                }
            }
            catch (Exception ex)
            {
                XEN.Logger.LogError($"Failed to load asset '{rawVal}' for type '{targetType.Name}': {ex.Message}", "SceneLoader");
                return null;
            }

            return Convert.ChangeType(rawVal, targetType, CultureInfo.InvariantCulture);
        }
    }
}