using UnityEngine;

public class RoomWallColliderSetup : MonoBehaviour
{
    [Tooltip("GameObject name prefix for room containers whose meshes should receive MeshColliders.")]
    public string roomContainerPrefix = "Bedroom Built-In";

    [Tooltip("Child name of the architectural room mesh inside each room container.")]
    public string roomChildName = "Room";

    void Awake()
    {
        AddWallColliders();
    }

    private void AddWallColliders()
    {
        Transform[] allTransforms = FindObjectsOfType<Transform>(false);
        for (int i = 0; i < allTransforms.Length; i++)
        {
            Transform t = allTransforms[i];
            if (t.name != roomChildName)
            {
                continue;
            }

            if (t.parent == null || !t.parent.name.StartsWith(roomContainerPrefix))
            {
                continue;
            }

            MeshFilter[] meshFilters = t.GetComponentsInChildren<MeshFilter>(false);
            for (int m = 0; m < meshFilters.Length; m++)
            {
                MeshFilter mf = meshFilters[m];
                if (mf == null || mf.sharedMesh == null)
                {
                    continue;
                }

                GameObject go = mf.gameObject;
                if (go.CompareTag("kolek") || go.CompareTag("Enemy"))
                {
                    continue;
                }

                if (go.GetComponent<Collider>() == null)
                {
                    go.AddComponent<MeshCollider>();
                }
            }
        }
    }
}
