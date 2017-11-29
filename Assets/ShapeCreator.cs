using System.Collections;
using System.Collections.Generic;
using UnityEngine;


    public class ShapeCreator : MonoBehaviour
    {

        [HideInInspector]
        public List<Shape> shapes = new List<Shape>();

        public float HandleRadius = .5f;

        
        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }

[System.Serializable]
public class Shape
{
    public List<Vector3> points = new List<Vector3>();
}

