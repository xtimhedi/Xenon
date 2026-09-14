using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Text;
using Veldrid;
using Xenon.NodeSystem;

namespace Xenon.Context
{
    public class Mesh
    {
        public uint[] indices;
        public VertexPositionColorTexture[] vertices;

        public string XenonFormatVersion = "";
        public bool IsXenonMesh = false;

        public AABB CalculateBounds()
        {
            Vector3 min = new Vector3(float.MaxValue);
            Vector3 max = new Vector3(float.MinValue);
            foreach (var v in vertices)
            {
                min = Vector3.Min(min, v.Position);
                max = Vector3.Max(max, v.Position);
            }
            return new AABB(min, max);
        }

        // --- BINARY LOADING ---

        public static Mesh LoadFromBinary(string filePath, RgbaFloat Color)
        {
            using Stream stream = File.OpenRead(filePath);
            return LoadFromBinary(stream, Color);
        }

        public static Mesh LoadFromBinary(Stream stream, RgbaFloat Color)
        {
            using BinaryReader reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);

            // 1. Read Magic Header "XMDL"
            byte[] magic = reader.ReadBytes(4);
            if (Encoding.ASCII.GetString(magic) != "XMDL")
                throw new InvalidDataException("Invalid magic number. File is not a valid Xenon binary mesh.");

            // 2. Read Xenon Specifics
            byte versionLength = reader.ReadByte();
            string xfv = Encoding.UTF8.GetString(reader.ReadBytes(versionLength));
            bool isXenMesh = reader.ReadByte() != 0;

            // 3. Read Array Counts
            uint vertexCount = reader.ReadUInt32();
            uint indexCount = reader.ReadUInt32();

            // 4. Read Vertices
            VertexPositionColorTexture[] vertexList = new VertexPositionColorTexture[vertexCount];
            for (uint i = 0; i < vertexCount; i++)
            {
                float x = reader.ReadSingle();
                float y = reader.ReadSingle();
                float z = reader.ReadSingle();
                float u = reader.ReadSingle();
                float v = reader.ReadSingle(); // Note: V is already flipped in the exporter

                vertexList[i] = new VertexPositionColorTexture(
                    new Vector3(x, y, z),
                    Color,
                    new Vector2(u, v));
            }

            // 5. Read Indices
            uint[] indexList = new uint[indexCount];
            for (uint i = 0; i < indexCount; i++)
            {
                indexList[i] = reader.ReadUInt32();
            }

            return new Mesh
            {
                vertices = vertexList,
                indices = indexList,
                IsXenonMesh = isXenMesh,
                XenonFormatVersion = xfv
            };
        }

        // --- LEGACY OBJ LOADING ---

        public static Mesh LoadFromObj(string filePath, RgbaFloat Color)
        {
            using Stream stream = File.OpenRead(filePath);
            return LoadFromObj(stream, Color);
        }

        public static Mesh LoadFromObj(Stream stream, RgbaFloat Color)
        {
            using StreamReader reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
            return LoadFromObj(reader, Color);
        }

        public static Mesh LoadFromObj(TextReader reader, RgbaFloat Color)
        {
            List<Vector3> positions = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<VertexPositionColorTexture> vertexList = new List<VertexPositionColorTexture>();
            List<uint> indexList = new List<uint>();
            Dictionary<(int pos, int uv), uint> indexMap = new Dictionary<(int pos, int uv), uint>();

            string XFV = "Unknown";
            bool IsXenMesh = false;
            string line;

            while ((line = reader.ReadLine()) != null)
            {
                string trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#"))
                    continue;

                string[] parts = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                // --- READ CUSTOM XENON METADATA ---
                if (parts[0] == "xenon_format_version")
                {
                    XFV = parts[1];
                    continue;
                }
                if (parts[0] == "xenon_mesh_def")
                {
                    bool.TryParse(parts[1], out IsXenMesh);
                    continue;
                }

                // --- READ STANDARD OBJ DATA ---
                if (parts[0] == "v")
                {
                    positions.Add(new Vector3(
                        float.Parse(parts[1], CultureInfo.InvariantCulture),
                        float.Parse(parts[2], CultureInfo.InvariantCulture),
                        float.Parse(parts[3], CultureInfo.InvariantCulture)));
                }
                else if (parts[0] == "vt")
                {
                    uvs.Add(new Vector2(
                        float.Parse(parts[1], CultureInfo.InvariantCulture),
                        1.0f - float.Parse(parts[2], CultureInfo.InvariantCulture))); // Flip V for Veldrid
                }
                else if (parts[0] == "f")
                {
                    List<uint> faceIndices = new List<uint>();

                    for (int i = 1; i < parts.Length; i++)
                    {
                        string[] elementParts = parts[i].Split('/');
                        int posIndex = int.Parse(elementParts[0]) - 1;
                        int uvIndex = elementParts.Length > 1 && !string.IsNullOrEmpty(elementParts[1])
                            ? int.Parse(elementParts[1]) - 1 : -1;

                        Vector2 uv = uvIndex >= 0 && uvIndex < uvs.Count ? uvs[uvIndex] : Vector2.Zero;

                        var key = (posIndex, uvIndex);
                        if (!indexMap.TryGetValue(key, out uint mappedIndex))
                        {
                            mappedIndex = (uint)vertexList.Count;
                            vertexList.Add(new VertexPositionColorTexture(positions[posIndex], Color, uv));
                            indexMap[key] = mappedIndex;
                        }
                        faceIndices.Add(mappedIndex);
                    }

                    // Triangulate
                    for (int i = 1; i < faceIndices.Count - 1; i++)
                    {
                        indexList.Add(faceIndices[0]);
                        indexList.Add(faceIndices[i]);
                        indexList.Add(faceIndices[i + 1]);
                    }
                }
            }

            // --- VALIDATION CHECK ---
            if (!IsXenMesh)
            {
                throw new InvalidDataException("This file does not contain the 'xenon_mesh_def' tag. It is not a valid XenonModel.");
            }

            if (vertexList.Count == 0 || indexList.Count == 0)
            {
                throw new InvalidDataException("The parsed XenonModel has no geometry (0 vertices or 0 indices).");
            }

            return new Mesh
            {
                vertices = vertexList.ToArray(),
                indices = indexList.ToArray(),
                IsXenonMesh = IsXenMesh,
                XenonFormatVersion = XFV
            };
        }
    }
}