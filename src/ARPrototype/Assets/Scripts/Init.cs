using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;

public class Init : MonoBehaviour
{
    void Awake()
    {
        VuforiaConfiguration.Instance.Vuforia.MaxSimultaneousImageTargets = 4;
    }
}
