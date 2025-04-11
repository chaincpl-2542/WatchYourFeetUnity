using UnityEngine;

public class BoxSpawner : MonoBehaviour
{
  [Header("Dependencies")]
    public PerspectiveTransform perspectiveTransform;
    public Camera mainCamera;
    public GameObject boxPrefab;

    private int rows;
    private int cols ;

    public void SpawnGrid()
    {
        rows = perspectiveTransform.gridRows;
        cols = perspectiveTransform.gridCols;
        
        if (perspectiveTransform == null || mainCamera == null || boxPrefab == null)
        {
            Debug.LogError("Missing references in Grid3DSpawner.");
            return;
        }

        if (perspectiveTransform.GetClickedPoints().Length < 4)
        {
            Debug.LogError("Not enough points clicked in PerspectiveTransform.");
            return;
        }

        Vector2[] clicked = perspectiveTransform.GetClickedPoints();

        Vector3 topLeft = GetWorldPositionFromScreen(clicked[0]);
        Vector3 topRight = GetWorldPositionFromScreen(clicked[1]);
        Vector3 bottomRight = GetWorldPositionFromScreen(clicked[2]);
        Vector3 bottomLeft = GetWorldPositionFromScreen(clicked[3]);

        Vector3 right = (topRight - topLeft).normalized;
        Vector3 down = (bottomLeft - topLeft).normalized;
        Vector3 normal = Vector3.Cross(right, down).normalized;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                float fx = (float)x / (cols - 1);
                float fy = (float)y / (rows - 1);

                Vector3 pos = 
                      topLeft * (1 - fx) * (1 - fy) +
                      topRight * fx * (1 - fy) +
                      bottomRight * fx * fy +
                      bottomLeft * (1 - fx) * fy;

                Instantiate(boxPrefab, pos, Quaternion.LookRotation(-normal), transform);
            }
        }
    }

    Vector3 GetWorldPositionFromScreen(Vector2 screenPoint)
    {
        Ray ray = mainCamera.ScreenPointToRay(screenPoint);
        Plane plane = new Plane(mainCamera.transform.forward * -1f, mainCamera.transform.position + mainCamera.transform.forward * 2f);
        plane.Raycast(ray, out float distance);
        return ray.GetPoint(distance);
    }
}

