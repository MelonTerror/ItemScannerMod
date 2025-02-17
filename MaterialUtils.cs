using System;
using System.Collections.Generic;
using System.Text;
using MelonLoader;
using UnityEngine;

namespace ItemScannerMod
{
    class MaterialUtils
    {
        public static void AddMaterialToGameObject(MeshRenderer _meshRenderer, Material _mat)
        {
            // Get the existing materials
            Material[] existingMaterials = _meshRenderer.materials;

            // Create a new array with space for the additional material
            Material[] newMaterials = new Material[existingMaterials.Length + 1];

            // Copy the old materials into the new array
            for (int i = 0; i < existingMaterials.Length; i++)
            {
                newMaterials[i] = existingMaterials[i];
            }

            // Add the new material at the end
            newMaterials[newMaterials.Length - 1] = _mat;

            // Apply the new materials array
            _meshRenderer.materials = newMaterials;
        }

        public static void RemoveCustomMaterial(Renderer renderer, string materialName)
        {
            Material[] existingMaterials = renderer.sharedMaterials; // Use sharedMaterials to avoid unnecessary instancing

            if (existingMaterials.Length <= 1)
            {
                MelonLogger.Msg("Only one material found. Skipping removal.");
                return; // Don't remove the last material to avoid rendering issues
            }

            // Create a new list and filter out the unwanted material
            List<Material> newMaterials = new List<Material>();

            foreach (Material mat in existingMaterials)
            {
                // Unity might append (Instance) to material names, so we check with StartsWith
                if (!mat.name.StartsWith(materialName))
                {
                    newMaterials.Add(mat);
                }
            }

            // Apply the new materials array
            if (newMaterials.Count < existingMaterials.Length)
            {
                renderer.sharedMaterials = newMaterials.ToArray(); // Use sharedMaterials for persistent change
                MelonLogger.Msg("Removed material: " + materialName);
            }
            else
            {
                MelonLogger.Warning("Material not found in the renderer's material list.");
            }
        }


    }
}
