using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MuzzelFlashLightTurnOff : MonoBehaviour
{
    Light _light;
    IEnumerator Start()
    {
        _light = GetComponent<Light>();
        yield return new WaitForSeconds(0.1f);
        _light.enabled = false;
    }
}
