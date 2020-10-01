using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtAttractor : MonoBehaviour
{
    void Update()
    {
        transform.LookAt(Attractor.POS);
    }
}
