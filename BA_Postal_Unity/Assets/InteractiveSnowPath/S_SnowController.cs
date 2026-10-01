using UnityEditor.VisionOS;
using UnityEngine;

public class S_SnowController : MonoBehaviour
{
    public ComputeShader SnowComputeShader;
    public RenderTexture snowRT;
    public float colorValueToAdd;

    private string snowImageProperty = "SnowImage";
    private string colorValueProperty = "colorValueToAdd";
    private string resolutionProperty = "resolution";
    private string positionXProperty = "positionX";
    private string positionYProperty = "positionY";
    private string spotSizeProperty = "spotSize";

    private string csMainKernel = "CSMain";
    private string fillWhiteKernel = "FillWhite";

    public int resolution = 512;
    private MeshRenderer meshRenderer;

    private void Awake()
    {
        CreateRenderTexture();
        SetRTColorToWhite();
        SetMaterialTexture();
        InvokeRepeating(nameof(AddSnowLayer), .1f, .1f);
        ExtendBoundofMesh();
    }

    void CreateRenderTexture()
    {
        snowRT = new RenderTexture(resolution, resolution, 24);
        snowRT.enableRandomWrite = true;
        snowRT.Create();
    }

    void SetRTColorToWhite()
    {
        int kernel_handle = SnowComputeShader.FindKernel(fillWhiteKernel);
        SnowComputeShader.SetTexture(kernel_handle, snowImageProperty, snowRT);
        SnowComputeShader.SetFloat(colorValueProperty, colorValueToAdd);
        SnowComputeShader.SetFloat(resolutionProperty, resolution);
        SnowComputeShader.SetFloat(positionXProperty, 0);
        SnowComputeShader.SetFloat(positionYProperty, 0);
        SnowComputeShader.SetFloat(spotSizeProperty, 0);
        SnowComputeShader.Dispatch(kernel_handle, snowRT.width / 8, snowRT.height / 8, 1);
    }

    void SetMaterialTexture()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material.SetTexture("_PathTexture", snowRT);
    }

    void AddSnowLayer()
    {
        int kernel_handle = SnowComputeShader.FindKernel(csMainKernel);
        SnowComputeShader.SetTexture(kernel_handle, snowImageProperty, snowRT);
        SnowComputeShader.SetFloat(colorValueProperty, colorValueToAdd);
        SnowComputeShader.SetFloat(resolutionProperty, resolution);
        SnowComputeShader.SetFloat(positionXProperty, 0);
        SnowComputeShader.SetFloat(positionYProperty, 0);
        SnowComputeShader.SetFloat(spotSizeProperty, 0);
        SnowComputeShader.Dispatch(kernel_handle, snowRT.width / 8, snowRT.height / 8, 1);
    }

    void ExtendBoundofMesh()
    {
        Bounds bounds = GetComponent<MeshFilter>().mesh.bounds;
        bounds.extents = new Vector3(2, 0, 2);
        GetComponent<MeshFilter>().mesh.bounds = bounds;
    }
}
