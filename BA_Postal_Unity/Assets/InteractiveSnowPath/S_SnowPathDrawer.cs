using UnityEngine;

public class S_SnowPathDrawer : MonoBehaviour
{
    public ComputeShader SnowComputeShader;
    public RenderTexture snowRT;

    private string snowImageProperty = "SnowImage";
    private string colorValueProperty = "colorValueToAdd";
    private string resolutionProperty = "resolution";
    private string positionXProperty = "positionX";
    private string positionYProperty = "positionY";
    private string spotSizeProperty = "spotSize";

    private string drawSpotKernel = "DrawSpot";

    private Vector2Int position = new Vector2Int(256, 256);
    public float spotSize = 5f;

    private S_SnowController snowController;
    private GameObject[] snowControllerObjs;

    private void Awake()
    {
        snowControllerObjs = GameObject.FindGameObjectsWithTag("SnowGround");
    }
    

}
