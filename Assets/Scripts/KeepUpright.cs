using UnityEngine;

public class KeepUpright : MonoBehaviour
{
    [Tooltip("Cancel the mirroring when a parent is flipped, so this object always reads the same way.")]
    public bool cancelFlip = true;

    private Vector3 upright;

    void Awake()
    {
        Capture();
    }

    public void Capture()
    {
        upright = transform.localScale;
        upright.x = Mathf.Abs(upright.x);
    }

    void LateUpdate()
    {
        if (!cancelFlip) return;

        Transform parent = transform.parent;
        if (parent == null) return;

        if (HandledHigherUp())
        {
            if (!Mathf.Approximately(transform.localScale.x, upright.x))
            {
                transform.localScale = upright;
            }
            return;
        }

        float wanted = parent.lossyScale.x < 0f ? -upright.x : upright.x;

        if (!Mathf.Approximately(transform.localScale.x, wanted))
        {
            transform.localScale = new Vector3(wanted, upright.y, upright.z);
        }
    }

    bool HandledHigherUp()
    {
        Transform parent = transform.parent;

        while (parent != null)
        {
            KeepUpright other = parent.GetComponent<KeepUpright>();
            if (other != null && other.enabled && other.cancelFlip) return true;

            parent = parent.parent;
        }

        return false;
    }
}
