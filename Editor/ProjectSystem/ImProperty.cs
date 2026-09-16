using ImGuiNET;
using System;
using System.Numerics;
using System.Reflection;

namespace Editor.ProjectSystem
{
    public interface IImProperty
    {
        void EnumValue();
        object Value { get; set; }
        string ValueName { get; set; }
        void Bind(object target, PropertyInfo prop);
    }

    public class ImProperty<T> : IImProperty
    {
        public T Value { get; set; }
        public string ValueName { get; set; }

        private object _target;
        private PropertyInfo _propInfo;

        object IImProperty.Value
        {
            get => Value;
            set => Value = value is T typed ? typed : default;
        }

        public void Bind(object target, PropertyInfo prop)
        {
            _target = target;
            _propInfo = prop;
            ValueName = prop.Name;

            if (_target != null && _propInfo != null)
            {
                object rawVal = _propInfo.GetValue(_target);
                if (rawVal is T typedVal)
                {
                    Value = typedVal;
                }
            }
        }

        public void EnumValue()
        {
            bool changed = false;
            ImGui.PushID(ValueName);

            if (typeof(T) == typeof(int))
            {
                int val = Convert.ToInt32(Value);
                if (ImGui.DragInt(ValueName, ref val))
                {
                    Value = (T)(object)val;
                    changed = true;
                }
            }
            else if (typeof(T) == typeof(float))
            {
                float val = Convert.ToSingle(Value);
                if (ImGui.DragFloat(ValueName, ref val, 0.1f))
                {
                    Value = (T)(object)val;
                    changed = true;
                }
            }
            else if (typeof(T) == typeof(Vector3))
            {
                Vector3 val = Value is Vector3 vec ? vec : Vector3.Zero;
                if (ImGui.DragFloat3(ValueName, ref val, 0.1f))
                {
                    Value = (T)(object)val;
                    changed = true;
                }
            }
            else if (typeof(T) == typeof(bool))
            {
                bool val = Convert.ToBoolean(Value);
                if (ImGui.Checkbox(ValueName, ref val))
                {
                    Value = (T)(object)val;
                    changed = true;
                }
            }
            else if (typeof(T) == typeof(string))
            {
                string val = Value as string ?? string.Empty;
                if (ImGui.InputText(ValueName, ref val, 256))
                {
                    Value = (T)(object)val;
                    changed = true;
                }
            }
            else
            {
                ImGui.TextDisabled($"{ValueName}: {Value}");
            }

            ImGui.PopID();

            if (changed && _target != null && _propInfo != null)
            {
                try
                {
                    _propInfo.SetValue(_target, Value);
                }
                catch (Exception ex)
                {
                    Xenon.XEN.Logger.Log($"[Property Error] Failed to set {ValueName}: {ex.Message}");
                }
            }
        }
    }
}