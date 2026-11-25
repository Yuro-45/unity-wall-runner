using UnityEngine;

// PURPOSE: every frame, find out whether a runnable wall is beside the player,
// which side it is on, and which way its surface faces (its normal).
// only reports; never moves the player.
public class WallSensor : MonoBehaviour
{

    [SerializeField] MovementSettings settings;
    [SerializeField] float rayHeight = 1f;


    static readonly Vector3[] localDirections =      // ray directions, relative to the player
    {
        new Vector3( 1f, 0f,  0f),                   // right
        new Vector3( 1f, 0f,  1f),                   // right-forward
        new Vector3( 1f, 0f, -1f),                   // right-back
        new Vector3(-1f, 0f,  0f),                   // left
        new Vector3(-1f, 0f,  1f),                   // left-forward
        new Vector3(-1f, 0f, -1f),                   // left-back
    };

    //  four results other scripts can read but only this script can set.
   
    public bool HasWall { get; private set; }      // true if runnable wall found
    public bool WallOnRight { get; private set; }      
    public Vector3 WallPoint { get; private set; }     // where the ray touched it
    public Vector3 WallNormal { get; private set; }

    void Update()
    {
        
        Scan();
    }

    public void Scan()
    {
        // TODO 5: start from "no wall found".
        //   - set HasWall to false
        //   - a local float "closest" starting at float.MaxValue
        //   - a local Vector3 "origin": the player's position raised by the ray height

        HasWall = false;
        float closest = float.MaxValue;
        Vector3 origin = transform.position +new Vector3(0f, rayHeight, 0f);

        // TODO 6: loop over the six directions with foreach. For each one:
        foreach (Vector3 local in localDirections)                         // try each of the six rays
        {
            Vector3 direction = transform.TransformDirection(local).normalized;   // local → world, length 1

            bool hitSomething = Physics.Raycast(origin, direction, out RaycastHit hit,
                settings.wallCheckDistance, settings.runnableMask);        // only hits Runnable walls

            if (!hitSomething) continue;                                   // a) nothing there
            if (Mathf.Abs(hit.normal.y) > 0.1f) continue;                  // b) floor/slope, not a wall
            if (hit.distance >= closest) continue;                         // c) found a nearer wall already

            closest = hit.distance;                                        // d) best wall so far
            HasWall = true;
            WallNormal = hit.normal;                                       // which way the wall faces
            WallPoint = hit.point;                                         // where the ray touched it
        }

        // TODO 7: after the loop, if a wall was found, work out its side.
        if (HasWall)
        {
            WallOnRight = Vector3.Dot(WallNormal, transform.right) < 0f;   // true if wall is on the player's right
        }
      
    }

    void OnDrawGizmos()
    {
        // TODO 8 (do this last): draw the sensor in the Scene view.
        if (settings == null) return;                                      // nothing to draw until settings is assigned

        Vector3 origin = transform.position + Vector3.up * rayHeight;      // same start point as Scan()

        Gizmos.color = Color.yellow;                                       // the six rays
        foreach (Vector3 local in localDirections)
        {
            Vector3 direction = transform.TransformDirection(local).normalized;
            Gizmos.DrawRay(origin, direction * settings.wallCheckDistance); // drawn at the real ray length
        }

        if (HasWall)                                                       // a wall was found this frame
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(WallPoint, 0.08f);                           // small dot where the ray touched
            Gizmos.DrawRay(WallPoint, WallNormal);                         // 1 m arrow showing which way the wall faces
        }
        //   - return early if settings is null
        //   - yellow: the six rays. Look up: Gizmos.color, Gizmos.DrawRay(origin, direction * length)
        //   - if HasWall, cyan: a small sphere at WallPoint (Gizmos.DrawSphere)
        //     and a ray from WallPoint along WallNormal
    }
}
