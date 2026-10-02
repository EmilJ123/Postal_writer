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

    private void FixedUpdate()
    {
        for (int i = 0; i < snowControllerObjs.Length; i++)
        {
            if(Vector3.Distance(snowControllerObjs[i].transform.position, transform.position) > spotSize * 5f) continue;
            
            snowController = snowControllerObjs[i].GetComponent<S_SnowController>();
            snowRT = snowController.snowRT;
            //SnowComputeShader = snowController.SnowComputeShader;
            GetPosition();
            DrawSpot();
        }
    }


    void GetPosition()
    {
        float scaleX = snowController.transform.localScale.x;
        float scaleY = snowController.transform.localScale.z;

        float snowPosX = snowController.transform.position.x;
        float snowPosY = snowController.transform.position.z;

        int posX = snowRT.width / 2 - (int)(((transform.position.x - snowPosX) * snowRT.width / 2) / scaleX);
        int posY = snowRT.height / 2 - (int)(((transform.position.z - snowPosY) * snowRT.height / 2) / scaleY);
        position = new Vector2Int(posX, posY);
    }

    void DrawSpot()
    {
        if (snowRT == null) return;
        if (SnowComputeShader == null) return;

        int kernel_handle = SnowComputeShader.FindKernel(drawSpotKernel);
        SnowComputeShader.SetTexture(kernel_handle, snowImageProperty, snowRT);
        SnowComputeShader.SetFloat(colorValueProperty,0);
        SnowComputeShader.SetFloat(resolutionProperty, snowRT.width);
        SnowComputeShader.SetFloat(positionXProperty, position.x);
        SnowComputeShader.SetFloat(positionYProperty, position.y);
        SnowComputeShader.SetFloat(spotSizeProperty, spotSize);
        SnowComputeShader.Dispatch(kernel_handle, snowRT.width / 8, snowRT.height / 8, 1);
    }
}
