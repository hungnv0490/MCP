using UnityEngine;

// Visual + drop-target marker for the one always-empty slot sitting below the
// last container in a column. E_BallContainerManager repositions it whenever a
// container is added to or removed from that column; drop detection just looks
// for this component via Physics2D.OverlapPointAll at the mouse's drop position.
public class E_ContainerSlot : MonoBehaviour
{
    public int ColumnIndex { get; set; }
}
