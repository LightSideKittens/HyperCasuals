using System;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [NonSerialized] public Shape currentShape;

    private void Awake()
    {
        FieldManager.Spawners.Add(this);
    }
}