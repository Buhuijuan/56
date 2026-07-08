using UnityEngine;

[ExecuteInEditMode]
public class FullscreenEffect : MonoBehaviour
{
    public Material material;

    [Range(-1, 1)]
    public float brightness = 0;

    [Range(0, 1)]
    public float colorBlind = 0;

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (material != null)
        {
            material.SetFloat("_Brightness", brightness);
            material.SetFloat("_ColorBlind", colorBlind);
            Graphics.Blit(src, dest, material);
        }
        else
        {
            Graphics.Blit(src, dest);
        }
    }
}
