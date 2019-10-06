using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneLoader : MonoBehaviour
{
    public MultiScene scenes = null;

    private void Start()
    {
        scenes.Load();
    }

}
