using UnityEngine;

public class DoorAnimationController : MonoBehaviour
{
    [SerializeField]
    private Door _door;

    public void OnDoorClosed()
    {
        _door.Close(false);    
    }

    public void OnDoorOpened()
    {
        _door.Open(true);
    }
}
