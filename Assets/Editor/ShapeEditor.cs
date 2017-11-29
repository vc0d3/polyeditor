﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


    [CustomEditor(typeof(ShapeCreator))]
    public class ShapeEditor : Editor
    {

        ShapeCreator shapeCreator;
        SelectionInfo selectionInfo;
        bool needsRepaint;

        void OnSceneGUI()
        {
            Event guiEvent = Event.current;

            if (guiEvent.type == EventType.repaint)
            {
                Draw();
            }

            else if (guiEvent.type == EventType.layout)
            {
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            }
            else
            {
                HandleInput(guiEvent);
                if (needsRepaint)
                {
                    HandleUtility.Repaint();
                }
            }
        }

        void CreateNewShape()
    {
        Undo.RecordObject(shapeCreator, "Create Shape");
        shapeCreator.shapes.Add(new Shape());
        selectionInfo.selectedShapeIndex = shapeCreator.shapes.Count - 1;

    }

    void  CreateNewPoint(Vector3 position)
    {
        bool mouseIsOverSelectedShape = selectionInfo.mouseOverShapeIndex == selectionInfo.selectedShapeIndex;
        int newPointIndex = (selectionInfo.mouseIsOverLine && mouseIsOverSelectedShape) ? selectionInfo.lineIndex + 1 : SelectedShape.points.Count;
        Undo.RecordObject(shapeCreator, "Add point");
        SelectedShape.points.Insert(newPointIndex, position);
        selectionInfo.pointIndex = newPointIndex;
        selectionInfo.mouseOverShapeIndex = selectionInfo.selectedShapeIndex;
        needsRepaint = true;

        SelectPointUnderMouse();
    }

    void DeletePointUnderMose()
    {
        Undo.RecordObject(shapeCreator, "Delete point");
        SelectedShape.points.RemoveAt(selectionInfo.pointIndex);
        selectionInfo.mouseIsOverPoint = false;
        selectionInfo.mouseIsOverPoint = false;
        needsRepaint = true;

    }

    void SelectPointUnderMouse()
    {
        selectionInfo.pointIsSelection = true;
        selectionInfo.mouseIsOverPoint = true;
        selectionInfo.mouseIsOverLine = false;
        selectionInfo.lineIndex = -1;

        selectionInfo.positionAtStartOfDrag = SelectedShape.points[selectionInfo.pointIndex];
        needsRepaint = true;
    }

    void SelectShapeUnderMouse()
    {
        if(selectionInfo.mouseOverShapeIndex != -1)
        {
            selectionInfo.selectedShapeIndex = selectionInfo.mouseOverShapeIndex;
            needsRepaint = true;
        }
    }
      

    void HandleInput(Event guiEvent)
        {
            Ray mouseRay = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition);
            float drawPlaneHeight = 0;
            float dstToDrawPlane = (drawPlaneHeight - mouseRay.origin.y) / mouseRay.direction.y;
            Vector3 mousePosition = mouseRay.GetPoint(dstToDrawPlane);

        if (guiEvent.type == EventType.mouseDown && guiEvent.button == 0 && guiEvent.modifiers == EventModifiers.Shift)
        {
            HandleShiftLeftMouseDown(mousePosition);
        }

        if (guiEvent.type == EventType.mouseDown && guiEvent.button == 0 && guiEvent.modifiers == EventModifiers.None)
            {
                HandleLeftMouseDown(mousePosition);
            }



            if (guiEvent.type == EventType.mouseUp && guiEvent.button == 0)
            {
                HandleLeftMouseUp(mousePosition);
            }

            if (guiEvent.type == EventType.mouseDrag && guiEvent.button == 0 && guiEvent.modifiers == EventModifiers.None)
            {
                HandleLeftMouseDrag(mousePosition);
            }
            if (!selectionInfo.pointIsSelection)
            {
                UpdateMouseOverInfo(mousePosition);
            }
        }


        void HandleShiftLeftMouseDown(Vector3 mousePosition)
    {
        CreateNewShape();
        CreateNewPoint(mousePosition);

    }

        void HandleLeftMouseDown(Vector3 mousePosition)
        {
            if(shapeCreator.shapes.Count == 0)
            {
            CreateNewShape();
            }

        SelectShapeUnderMouse();

        //if (selectionInfo.mouseIsOverPoint)
        //{
        //    CreateNewShape();
        //}

        if(selectionInfo.mouseIsOverPoint)
        {
            SelectPointUnderMouse();
        }

        else
        {
            CreateNewPoint(mousePosition);
        }
                

            selectionInfo.pointIsSelection = true;
            selectionInfo.positionAtStartOfDrag = mousePosition;
            needsRepaint = true;

    }

        void HandleLeftMouseUp(Vector3 mousePosition)
        {
            if (selectionInfo.pointIsSelection)
            {
            SelectedShape.points[selectionInfo.pointIndex] = selectionInfo.positionAtStartOfDrag;
                Undo.RecordObject(shapeCreator, "Move Point");
            SelectedShape.points[selectionInfo.pointIndex] = mousePosition;

                selectionInfo.pointIsSelection = false;
                selectionInfo.pointIndex = -1;
                needsRepaint = true;
            }
        }

        void HandleLeftMouseDrag(Vector3 mousePosition)
        {
            if (selectionInfo.pointIsSelection)
            {
                SelectedShape.points[selectionInfo.pointIndex] = mousePosition;
                needsRepaint = true;
            }
        }

        void UpdateMouseOverInfo(Vector3 mousePosition)
        {         
           
            int mouseOverPointIndex = -1;
        int mouseOverShapeIndex = -1;

        for (int shapeIndex = 0; shapeIndex < shapeCreator.shapes.Count; shapeIndex++)
        {
            Shape currentShape = shapeCreator.shapes[shapeIndex];


            for (int i = 0; i < currentShape.points.Count; i++)
            {
                if (Vector3.Distance(mousePosition, currentShape.points[i]) < shapeCreator.HandleRadius)
                {
                    mouseOverPointIndex = i;
                    mouseOverShapeIndex = shapeIndex;
                    break;
                }
            }
        }

            if (mouseOverPointIndex != selectionInfo.pointIndex || mouseOverShapeIndex != selectionInfo.mouseOverShapeIndex)
            {
            selectionInfo.mouseOverShapeIndex = mouseOverShapeIndex;
                selectionInfo.pointIndex = mouseOverPointIndex;
                selectionInfo.mouseIsOverPoint = mouseOverPointIndex != -1;

                needsRepaint = true;
            }

            if (selectionInfo.mouseIsOverPoint)
            {
                selectionInfo.mouseIsOverLine = false;
            selectionInfo.lineIndex = -1;
            }
            else
            {
                int mouseOverLineIndex = -1;
            float closestLineDst = shapeCreator.HandleRadius;

            for (int shapeIndex = 0; shapeIndex < shapeCreator.shapes.Count; shapeIndex++)
            {
                Shape currentShape = shapeCreator.shapes[shapeIndex];

                for (int i = 0; i < currentShape.points.Count; i++)
                {
                    Vector3 nextPointInShape = currentShape.points[(i + 1) % currentShape.points.Count];
                    float dstFromMouseToLine = HandleUtility.DistancePointToLineSegment(mousePosition.ToXZ(), currentShape.points[i].ToXZ(), nextPointInShape.ToXZ());
                    if (dstFromMouseToLine < closestLineDst)
                    {
                        closestLineDst = dstFromMouseToLine;
                        mouseOverLineIndex = i;
                        mouseOverShapeIndex = shapeIndex;
                    }
                }
            }

                if(selectionInfo.lineIndex != mouseOverLineIndex || mouseOverShapeIndex != selectionInfo.mouseOverShapeIndex)
            {
                selectionInfo.mouseOverShapeIndex = mouseOverShapeIndex;
                selectionInfo.lineIndex = mouseOverLineIndex;
                selectionInfo.mouseIsOverLine = mouseOverLineIndex != -1;
                needsRepaint = true;
            }
            }
        }

        void Draw()
        {
        for (int shapeIndex = 0; shapeIndex < shapeCreator.shapes.Count; shapeIndex++)
        {
            Shape shapeToDraw = shapeCreator.shapes[shapeIndex];
            bool shapeIsSelected = shapeIndex == selectionInfo.selectedShapeIndex;
            bool mouseIsOverShape = shapeIndex == selectionInfo.mouseOverShapeIndex;
            Color deselectedShapeColor = Color.grey;

            for (int i = 0; i < shapeToDraw.points.Count; i++)
            {
                Vector3 nextPoint = shapeToDraw.points[(i + 1) % shapeToDraw.points.Count];
                if (i == selectionInfo.lineIndex && mouseIsOverShape)
                {
                    Handles.color = Color.red;
                    Handles.DrawLine(shapeToDraw.points[i], nextPoint);
                }
                else
                {
                    Handles.color = (shapeIsSelected)? Color.black:deselectedShapeColor;
                    Handles.DrawDottedLine(shapeToDraw.points[i], nextPoint, 4);
                }

                if (i == selectionInfo.pointIndex && mouseIsOverShape)
                {
                    Handles.color = (selectionInfo.pointIsSelection) ? Color.black : Color.red;
                }
                else
                {
                    Handles.color = (shapeIsSelected) ? Color.white : deselectedShapeColor;
                }
                Handles.DrawSolidDisc(shapeToDraw.points[i], Vector3.up, shapeCreator.HandleRadius);
            }
        }

        needsRepaint = false;
        }

        void OnEnable()
        {
            shapeCreator = target as ShapeCreator;
            selectionInfo = new SelectionInfo();
        Undo.undoRedoPerformed += OnUndoOrRedo;

        }

    void OnDisable()
    {
        Undo.undoRedoPerformed += OnUndoOrRedo;
    }

        void OnUndoOrRedo()
    {
        if(selectionInfo.selectedShapeIndex >= shapeCreator.shapes.Count)
        {
            selectionInfo.selectedShapeIndex = shapeCreator.shapes.Count - 1;
        }
    }

        Shape SelectedShape
    {
        get
        {
            return shapeCreator.shapes[selectionInfo.selectedShapeIndex];
        }
    }


        public class SelectionInfo
        {
        public int selectedShapeIndex;
        public int mouseOverShapeIndex;
            public int pointIndex = -1;
            public bool mouseIsOverPoint;
            public bool pointIsSelection;
            public Vector3 positionAtStartOfDrag;

            public int lineIndex = -1;
            public bool mouseIsOverLine;
        }

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }

