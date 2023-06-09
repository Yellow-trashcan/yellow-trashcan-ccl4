using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;

public class FVXHandler : MonoBehaviour
{
    [SerializeField] private List<MeshRenderer> meshRenderers;
    [Tooltip("This is the id of which material will be the highlight. Could change on elements with more than one material.")]
    [SerializeField] private Material highlightMaterial;

    [Header("Only for skinned mesh renderer")]
    [SerializeField] bool hasSkinnedMesh;
    [SerializeField] SkinnedMeshRenderer skinnedMeshRenderer;

    private int idOfHighlight = 0;

    public void ActivateHighLight()
    {
        if (hasSkinnedMesh)
        {
            // Actually does not work
            skinnedMeshRenderer.AddMaterial(highlightMaterial);
            idOfHighlight = skinnedMeshRenderer.materials.Length;
            return;
        }

        foreach(MeshRenderer meshRenderer in meshRenderers)
        {
            meshRenderer.AddMaterial(highlightMaterial);
            idOfHighlight = meshRenderer.materials.Length - 1;
        }
        
    }

    public void DeactivateHighlight()
    {
        if (hasSkinnedMesh)
        {
            Material[] materials = new Material[idOfHighlight]; // Creates an array with the amount of materials we need without conuting the highlight
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = skinnedMeshRenderer.materials[i];
            }
            skinnedMeshRenderer.materials = materials;
        }

        foreach(MeshRenderer meshRenderer in meshRenderers)
        {
            int numberOfCurrentMaterials = meshRenderer.materials.Length;
            Material[] materials = new Material[idOfHighlight]; // Creates an array with the amount of materials we need without conuting the highlight
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = meshRenderer.materials[i];
            }

            meshRenderer.materials = materials;
        }
    }
}
