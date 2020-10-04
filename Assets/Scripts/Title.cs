using Cinemachine;
using UnityEngine;

public class Title : MonoBehaviour
{
    public GameObject world;
    public Transform newCameraTarget;
    public CinemachineVirtualCamera virtualCamera;

    void Update()
    {
        if (Input.anyKeyDown && virtualCamera.Follow == transform)
        {
            world.SetActive(true);
            virtualCamera.Follow = newCameraTarget;
        }
    }
}
