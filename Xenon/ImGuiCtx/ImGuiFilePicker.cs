using System;
using System.IO;
using System.Collections.Generic;
using ImGuiNET;


namespace Xenon.ImGuiCtx
{
    public class ImGuiFilePicker
    {
        public string SelectedPath { get; private set; } = string.Empty;

        private string _currentDirectory;
        private string _filter = "*.*";
        private bool _showHidden = false;
        private string _searchQuery = "";

        private List<string> _directories = new List<string>();
        private List<string> _files = new List<string>();

        public ImGuiFilePicker(string initialDirectory = "")
        {
            if (string.IsNullOrWhiteSpace(initialDirectory) || !Directory.Exists(initialDirectory))
            {
                _currentDirectory = Directory.GetCurrentDirectory();
            }
            else
            {
                _currentDirectory = initialDirectory;
            }
            Refresh();
        }

        public void SetFilter(string filter)
        {
            _filter = filter;
            Refresh();
        }

        public void Refresh()
        {
            _directories.Clear();
            _files.Clear();

            try
            {
                if (!Directory.Exists(_currentDirectory)) return;

                var dirs = Directory.GetDirectories(_currentDirectory);
                foreach (var dir in dirs)
                {
                    var name = Path.GetFileName(dir);
                    if (!_showHidden && name.StartsWith(".")) continue;
                    if (!string.IsNullOrWhiteSpace(_searchQuery) && name.IndexOf(_searchQuery, StringComparison.OrdinalIgnoreCase) < 0) continue;
                    _directories.Add(dir);
                }

                var files = Directory.GetFiles(_currentDirectory, _filter);
                foreach (var file in files)
                {
                    var name = Path.GetFileName(file);
                    if (!_showHidden && name.StartsWith(".")) continue;
                    if (!string.IsNullOrWhiteSpace(_searchQuery) && name.IndexOf(_searchQuery, StringComparison.OrdinalIgnoreCase) < 0) continue;
                    _files.Add(file);
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Fail silently on restricted OS system paths
            }
        }

        public bool Draw(ref bool isOpen)
        {
            bool result = false;

            ImGui.SetNextWindowSize(new System.Numerics.Vector2(650, 400), ImGuiCond.FirstUseEver);

            if (ImGui.Begin("File Browser", ref isOpen))
            {
                // --- TOP PANEL ---
                if (ImGui.Button("Up"))
                {
                    var parent = Directory.GetParent(_currentDirectory);
                    if (parent != null)
                    {
                        _currentDirectory = parent.FullName;
                        Refresh();
                    }
                }
                ImGui.SameLine();
                ImGui.TextUnformatted(_currentDirectory);

                ImGui.Separator();

                ImGui.Text("Filter:");
                ImGui.SameLine();

                ImGui.PushItemWidth(150);
                if (ImGui.InputText("##Search", ref _searchQuery, (uint)256))
                {
                    Refresh();
                }
                ImGui.PopItemWidth();

                ImGui.SameLine();
                if (ImGui.Checkbox("Show Hidden", ref _showHidden))
                {
                    Refresh();
                }

                // --- CENTRAL PANEL: Columns Fallback View ---
                System.Numerics.Vector2 childSize = new System.Numerics.Vector2(0, ImGui.GetWindowHeight() - 110);

                if (ImGui.BeginChild("FileView", childSize, true))
                {
                    ImGui.Columns(2, "FileColumns", true);
                    ImGui.SetColumnWidth(0, ImGui.GetWindowWidth() - 120);

                    // Render Folder Entry Items
                    foreach (var dir in _directories)
                    {
                        string dirName = Path.GetFileName(dir);
                        bool isSelected = (SelectedPath == dir);

                        if (ImGui.Selectable($"[Dir] {dirName}##{dir}", isSelected, ImGuiSelectableFlags.AllowDoubleClick))
                        {
                            SelectedPath = dir;

                            // FIX: Pass 0 directly. Older ImGui.NET expects an int, not an enum.
                            if (ImGui.IsMouseDoubleClicked(0))
                            {
                                _currentDirectory = dir;
                                SelectedPath = string.Empty;
                                _searchQuery = "";
                                Refresh();
                                break;
                            }
                        }

                        ImGui.NextColumn();
                        ImGui.Text("Folder");
                        ImGui.NextColumn();
                    }

                    // Render File Entry Items
                    foreach (var file in _files)
                    {
                        string fileName = Path.GetFileName(file);
                        bool isSelected = (SelectedPath == file);

                        if (ImGui.Selectable($"[File] {fileName}##{file}", isSelected, ImGuiSelectableFlags.AllowDoubleClick))
                        {
                            SelectedPath = file;

                            // FIX: Pass 0 directly. Older ImGui.NET expects an int, not an enum.
                            if (ImGui.IsMouseDoubleClicked(0))
                            {
                                result = true;
                                isOpen = false;
                            }
                        }

                        ImGui.NextColumn();
                        ImGui.Text(Path.GetExtension(file).ToLower());
                        ImGui.NextColumn();
                    }

                    ImGui.Columns(1); // Reset layout scope
                    ImGui.EndChild();
                }

                // --- BOTTOM PANEL ---
                ImGui.Separator();
                ImGui.Text($"Selected: {Path.GetFileName(SelectedPath)}");

                ImGui.SameLine(ImGui.GetWindowWidth() - 140);
                if (ImGui.Button("Cancel"))
                {
                    isOpen = false;
                }
                ImGui.SameLine();

                bool canOpen = !string.IsNullOrEmpty(SelectedPath) && !Directory.Exists(SelectedPath);

                if (!canOpen)
                {
                    ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0.5f);
                }

                if (ImGui.Button("Open") && canOpen)
                {
                    result = true;
                    isOpen = false;
                }

                if (!canOpen)
                {
                    ImGui.PopStyleVar();
                }

                ImGui.End();
            }

            return result;
        }
    }
}