using Editor.ProjectSystem;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Reflection;
using Xenon.Context;
using Xenon.NodeSystem;

namespace Editor.Windows
{
    public class PropertiesWindow : EditorWindow
    {
        private string _tl1 = "";
        private int _currentNodeId = -1;
        private Node _targetNode;

        public List<IImProperty> Properties { get; } = new List<IImProperty>();

        public PropertiesWindow(string title) : base(title) { }

        public void SetupProperties(Node node)
        {
            _targetNode = node;
            Properties.Clear();

            if (_targetNode == null) return;

            PropertyInfo[] props = _targetNode.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo prop in props)
            {
                if (!prop.CanRead || !prop.CanWrite) continue;

                Type openType = typeof(ImProperty<>);
                Type closedType = openType.MakeGenericType(prop.PropertyType);

                IImProperty instance = (IImProperty)Activator.CreateInstance(closedType);
                instance.Bind(_targetNode, prop);

                Properties.Add(instance);
            }
        }

        public void LoadNodeProperties(int nodeId)
        {
            _currentNodeId = nodeId;
            _targetNode = null;

            if (Program.tw?.root != null)
            {
                Program.tw.root.CascadeAction(CascadeActionLoad);
            }
        }

        private void CascadeActionLoad(ImTreeNode node)
        {
            if (node != null && node.id == _currentNodeId)
            {
                SetupProperties(node.ReferenceNode);
            }
        }

        public override void Window(double dt, RenderContext ctx)
        {
            if (ImGui.Begin(Title))
            {
                ImGui.InputText("##LoadNodeID", ref _tl1, 32);
                ImGui.SameLine();

                if (ImGui.Button("Load"))
                {
                    if (int.TryParse(_tl1, out int targetId))
                    {
                        LoadNodeProperties(targetId);
                    }
                }

                ImGui.Separator();

                if (_targetNode != null)
                {
                    ImGui.TextDisabled($"Editing Node ID: {_currentNodeId} ({_targetNode.GetType().Name})");
                    ImGui.Separator();

                    foreach (IImProperty prop in Properties)
                    {
                        prop?.EnumValue();
                    }
                }
                else
                {
                    ImGui.TextDisabled("No node loaded or ID not found.");
                }
            }
            ImGui.End();
        }
    }
}