using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MatrixVisualization : MonoBehaviour
{
    public List<float> matrixArray;
    public int size;
    private RenderTexture renderTexture;
    private ComputeBuffer matrixBuffer;
    public float[] data;
    void Random_array()
    {
        for (int i=0;i<size;++i)
        {
             matrixArray.Add(Random.value);
            //matrixArray.Add(1);
        }
    }
    
    void Start()
    {
        //matrixArray = new List<float>();
        //Random_array();
        // Create a new RenderTexture and set it to "Read/Write Enabled"
        renderTexture = new RenderTexture(4, 4, 24, RenderTextureFormat.ARGBFloat);
        renderTexture.enableRandomWrite = true;

        // Create a new material and assign the RenderTexture to it
        Material material = new Material(Shader.Find("Unlit/Texture"));
        material.mainTexture = renderTexture;

        // Create a quad and apply the material to it
        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.transform.localScale = Vector3.one * 0.5f;
        quad.GetComponent<MeshRenderer>().material = material;

        // Convert the 2D float list to a 1D float array
        //float[] matrixArray = matrix.SelectMany(x => x).ToArray();

        // Create a buffer to hold the matrix data
        matrixBuffer = new ComputeBuffer(matrixArray.Count, sizeof(float));
        
    }
    void Update()
    {
        //matrixArray.Clear();
        // Load the compute shader
        //Random_array();
        matrixBuffer.SetData(matrixArray);
        ComputeShader computeShader = Resources.Load<ComputeShader>("MatrixVisualizationShader");

        // Set the parameters of the compute shader
        int kernel = computeShader.FindKernel("MatrixVisualization");
        
        computeShader.SetInt("MatrixWidth", matrixArray.Count);
        computeShader.SetInt("MatrixHeight", matrixArray.Count);
        computeShader.SetBuffer(kernel, "MatrixBuffer", matrixBuffer);
        computeShader.SetTexture(kernel, "Result", renderTexture);
 
        //print(data);
        // Dispatch the compute shader
        computeShader.Dispatch(kernel, 8, 8, 1);

        // Clear the contents of the RenderTexture
        renderTexture.DiscardContents();

        // Re-render the updated image to the RenderTexture
        Graphics.Blit(renderTexture, renderTexture);
    }
}