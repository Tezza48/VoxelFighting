using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkInteraction : MonoBehaviour
{
    Vector3 debugCursorPos;
    Vector3Int selectedVoxelPos;
    Vector3 normal;

    Camera cam;

    Map map;

    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<Camera>();
        map = FindObjectOfType<Map>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            RaycastHit hit;
            var ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                var chunk = hit.collider.gameObject;
                var position = hit.point;
                normal = hit.normal;

                position -= normal / 2.0f;

                var floored = MathUtils.Floor(position);
                var asVec3Int = MathUtils.ToVector3Int(floored);

                debugCursorPos = floored;
                map.BreakVoxel(asVec3Int);
            }
        }

        if (Input.GetButtonDown("Fire2"))
        {
            RaycastHit hit;
            var ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                var chunk = hit.collider.gameObject;
                var position = hit.point;
                normal = hit.normal;

                position -= normal / 2.0f;

                var floored = MathUtils.Floor(position);
                floored += normal;
                var asVec3Int = MathUtils.ToVector3Int(floored);
                map.SetVoxel(asVec3Int, Color.red);
            }
        }

        {
            // make sure gizmo is drawn around current block and new block pos is drawn too
            RaycastHit hit;
            var ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                var chunk = hit.collider.gameObject;
                var position = hit.point;
                normal = hit.normal;

                position -= normal / 2.0f;

                var floored = MathUtils.Floor(position);
                var asVec3Int = MathUtils.ToVector3Int(floored);
                selectedVoxelPos = asVec3Int;

                debugCursorPos = floored;
            }
        }
    }

    private void OnDrawGizmos()
    {

        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(debugCursorPos + Vector3.one / 2.0f, Vector3.one * 1.01f);


        Gizmos.color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
        Gizmos.DrawCube(debugCursorPos + Vector3.one / 2.0f + normal, Vector3.one / 2.0f);
    }

    private void OnGUI()
    {
        GUILayout.Label("Pos: " + selectedVoxelPos);
    }
}
