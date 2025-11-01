using UnityEngine;

// Door implementation
public class Door : Opening
{
    [Header("Door Specific")]
    public bool isDoorOpen = false;

    public override bool CanWaterFlow()
    {
        return isDoorOpen;
    }

    public void OpenDoor()
    {
        isDoorOpen = true;
    }

    public void CloseDoor()
    {
        isDoorOpen = false;
    }
}

