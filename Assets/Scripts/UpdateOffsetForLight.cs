using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class UpdateOffsetForLight : MonoBehaviour
{
    public new TheLight light;

    private CinemachineTransposer transposer;

    void Start()
    {
        transposer = GetComponent<CinemachineVirtualCamera>()
            .GetCinemachineComponent<CinemachineTransposer>();
    }

    void Update()
    {
        transposer.m_FollowOffset.y = light.GetScale() * Camera.main.orthographicSize;
    }
}
