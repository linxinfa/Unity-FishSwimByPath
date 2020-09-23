using UnityEngine;
using UnityEditor;

/// <summary>
/// ²åÈë/É¾³ý½Úµã
/// </summary>
public partial class SplineEditor : InstantInspector
{
    private GUIStyle sceneGUIStyle = null;
    private GUIStyle sceneGUIStyleToolLabel = null;

    private float toolSphereAlpha = 0f;
    private float toolSphereSize = 1f;
    private float toolTargetSphereSize = 0f;

    private float lastRealTime = 0f;
    private float deltaTime = 0f;

    public void OnSceneGUI()
    {
        Spline spline = target as Spline;

        InitSceneGUIStyles();
        CalculateTimeDelta();

        DrawSplineInfo(spline);
        DrawHandles(spline);

        HandleMouseInput(spline);

        RegisterChanges();
    }

    private bool MyAddToSelectTheSplineNode(Spline spline)
    {
        Ray mouseRay = Camera.current.ScreenPointToRay(new Vector2(Event.current.mousePosition.x, Screen.height - Event.current.mousePosition.y - 32f));
        float splineParam = spline.GetClosestPointParamToRay(mouseRay, 3);
        Vector3 position = spline.GetPositionOnSpline(splineParam);
        float currentDistance = Vector3.Cross(mouseRay.direction, position - mouseRay.origin).magnitude;
        foreach (SplineNode node in spline.SplineNodes)
        {
            float newDistance = Vector3.Distance(node.Position, position);
            if (newDistance < currentDistance || newDistance < 0.2f * HandleUtility.GetHandleSize(node.Position))
            {
                currentDistance = newDistance;
                Selection.activeGameObject = node.gameObject;
                return true;
            }
        }
        return false;
    }

    private bool IsKeyRightControl()
    {
        Event e = Event.current;
        if (e.isKey)
        {
            if (e.keyCode == KeyCode.RightControl)
            {
                return true;
            }
        }
        return false;
    }

    private void HandleMouseInput(Spline spline)
    {
        //add by linxinfa-------------------------------------------
        if (IsKeyRightControl())
        {
            MyAddToSelectTheSplineNode(spline);
            return;
        }
        //-----------------------------------------------------------

        if (!EditorGUI.actionKey)
        {
            toolSphereSize = 10f;
            toolSphereAlpha = 0f;
            return;
        }
        else
        {
            toolSphereAlpha = Mathf.Lerp(toolSphereAlpha, 0.75f, deltaTime * 4f);
            toolSphereSize = Mathf.Lerp(toolSphereSize, toolTargetSphereSize + Mathf.Sin(Time.realtimeSinceStartup * 2f) * 0.1f, deltaTime * 15f);
        }

        Ray mouseRay = Camera.current.ScreenPointToRay(new Vector2(Event.current.mousePosition.x, Screen.height - Event.current.mousePosition.y - 32f));
        float splineParam = spline.GetClosestPointParamToRay(mouseRay, 3);
        Vector3 position = spline.GetPositionOnSpline(splineParam);

        float currentDistance = Vector3.Cross(mouseRay.direction, position - mouseRay.origin).magnitude;

        SplineNode selectedNode = null;

        foreach (SplineNode node in spline.SplineNodes)
        {
            float newDistance = Vector3.Distance(node.Position, position);

            if (newDistance < currentDistance || newDistance < 0.2f * HandleUtility.GetHandleSize(node.Position))
            {
                currentDistance = newDistance;

                selectedNode = node;
            }
        }

        if (selectedNode != null)
        {
            position = selectedNode.Position;

            Handles.color = new Color(.7f, 0.15f, 0.1f, toolSphereAlpha);
            Handles.SphereCap(0, position, Quaternion.identity, HandleUtility.GetHandleSize(position) * 0.25f * toolSphereSize);

            Handles.color = Color.white;
            Handles.Label(LabelPosition2D(position, 0.3f), "Delete Node (" + selectedNode.gameObject.name + ")", sceneGUIStyleToolLabel);

            toolTargetSphereSize = 1.35f;
        }
        else
        {
            Handles.color = new Color(.5f, 1f, .1f, toolSphereAlpha);
            Handles.SphereCap(0, position, Quaternion.identity, HandleUtility.GetHandleSize(position) * 0.25f * toolSphereSize);

            Handles.color = Color.white;
            Handles.Label(LabelPosition2D(position, 0.3f), "Insert Node", sceneGUIStyleToolLabel);

            toolTargetSphereSize = 0.8f;
        }

        if (Event.current.type == EventType.MouseDown && Event.current.button == 1)
        {
            //Undo.RegisterSceneUndo( ( selectedNode != null ) ? "Delete Spline Node (" + selectedNode.name +  ")" : "Insert Spline Node" );

            if (selectedNode != null)
            {
                Undo.RecordObject(spline, "Delete Spline Node (" + selectedNode.name + ")");
                spline.RemoveSplineNode(selectedNode.gameObject);
                Undo.DestroyObjectImmediate(selectedNode.gameObject);
                //DestroyImmediate(selectedNode.gameObject);
            }
            else
            {
                Undo.RecordObject(spline, "Insert Spline Node");
                InsertNode(spline, splineParam);
            }

            ApplyChangesToTarget(spline);
        }

        HandleUtility.Repaint();
    }

    private void DrawHandles(Spline spline)
    {
        if (Event.current.alt || EditorGUI.actionKey)
            return;

        if (Tools.current == Tool.None || Tools.current == Tool.View)
            return;

        Handles.lighting = true;

        foreach (SplineNode node in spline.SplineNodes)
        {
            switch (Tools.current)
            {
                case Tool.Rotate:
                    //Undo.SetSnapshotTarget( node.transform, "Rotate Spline Node: " + node.name );

                    Quaternion newRotation = Handles.RotationHandle(node.Rotation, node.Position);

                    Handles.color = new Color(.2f, 0.4f, 1f, 1);
                    Handles.ArrowCap(0, node.Position, Quaternion.LookRotation(node.transform.forward), HandleUtility.GetHandleSize(node.Position) * 0.5f);
                    Handles.color = new Color(.3f, 1f, .20f, 1);
                    Handles.ArrowCap(0, node.Position, Quaternion.LookRotation(node.transform.up), HandleUtility.GetHandleSize(node.Position) * 0.5f);
                    Handles.color = Color.white;

                    if (!GUI.changed)
                        break;
                    Undo.RecordObject(node.transform, "Rotate Spline Node: " + node.name);
                    node.Rotation = newRotation;

                    EditorUtility.SetDirty(target);

                    break;

                case Tool.Move:
                case Tool.Scale:
                    //Undo.SetSnapshotTarget( node.transform, "Move Spline Node: " + node.name );

                    Vector3 newPosition = Handles.PositionHandle(node.Position, (Tools.pivotRotation == PivotRotation.Global) ? Quaternion.identity : node.transform.rotation);

                    if (!GUI.changed)
                        break;
                    Undo.RecordObject(node.transform, "Move Spline Node: " + node.name);
                    node.Position = newPosition;

                    EditorUtility.SetDirty(target);

                    break;
            }

            CreateSnapshot();
        }
    }

    private void DrawSplineInfo(Spline spline)
    {
        if (Event.current.alt && !Event.current.shift)
        {
            foreach (SplineNode splineNode in spline.SplineNodes)
                Handles.Label(LabelPosition2D(splineNode.Position, -0.2f), splineNode.name, sceneGUIStyle);

            foreach (SplineSegment segment in spline.SplineSegments)
            {
                for (int i = 0; i < 10; i++)
                {
                    float splineParam = segment.ConvertSegmentToSplineParamter(i / 10f);

                    Vector3 position = spline.GetPositionOnSpline(splineParam);
                    Vector3 normal = spline.GetNormalToSpline(splineParam);

                    Handles.color = new Color(.3f, 1f, .20f, 0.75f);
                    Handles.ArrowCap(0, position, Quaternion.LookRotation(normal), HandleUtility.GetHandleSize(position) * 0.5f);
                }
            }

            Vector3 tangentPosition0 = spline.GetPositionOnSpline(0);

            Handles.color = new Color(.2f, 0.4f, 1f, 1);
            Handles.ArrowCap(0, tangentPosition0, Quaternion.LookRotation(spline.GetTangentToSpline(0)), HandleUtility.GetHandleSize(tangentPosition0) * 0.5f);

            Vector3 tangentPosition1 = spline.GetPositionOnSpline(1);
            Handles.ArrowCap(0, tangentPosition1, Quaternion.LookRotation(spline.GetTangentToSpline(1)), HandleUtility.GetHandleSize(tangentPosition1) * 0.5f);
        }
        else if (Event.current.alt && Event.current.shift)
        {
            foreach (SplineSegment item in spline.SplineSegments)
            {
                Vector3 positionOnSpline = spline.GetPositionOnSpline(item.ConvertSegmentToSplineParamter(0.5f));

                Handles.Label(LabelPosition2D(positionOnSpline, -0.2f), item.Length.ToString(), sceneGUIStyle);
            }

            Handles.Label(LabelPosition2D(spline.transform.position, -0.3f), "Length: " + spline.Length.ToString(), sceneGUIStyle);
        }
    }

    private void CreateSnapshot()
    {
        /*if( Input.GetMouseButtonDown( 0 ) ) 
		{
			Undo.CreateSnapshot( );
			Undo.RegisterSnapshot( );
		}*/
    }

    private void InsertNode(Spline spline, float splineParam)
    {
        SplineNode splineNode = CreateSplineNode("New Node", spline.GetPositionOnSpline(splineParam), spline.GetOrientationOnSpline(splineParam), spline.transform);
        SplineSegment segment = spline.GetSplineSegment(splineParam);

        int startNodeIndex = spline.splineNodesArray.IndexOf(segment.StartNode);

        spline.splineNodesArray.Insert(startNodeIndex + 1, splineNode);
    }

    private SplineNode CreateSplineNode(string nodeName, Vector3 position, Quaternion rotation, Transform parent)
    {
        GameObject gameObject = new GameObject(nodeName);

        gameObject.transform.position = position;
        gameObject.transform.rotation = rotation;

        gameObject.transform.parent = parent;

        SplineNode splineNode = gameObject.AddComponent<SplineNode>();

        return splineNode;
    }

    private void RegisterChanges()
    {
        if (GUI.changed)
            ApplyChangesToTarget(target);
    }

    private Vector3 LabelPosition2D(Vector3 position, float offset)
    {
        return position - Camera.current.transform.up * HandleUtility.GetHandleSize(position) * offset;
    }

    private void CalculateTimeDelta()
    {
        deltaTime = Time.realtimeSinceStartup - lastRealTime;
        lastRealTime = Time.realtimeSinceStartup;

        deltaTime = Mathf.Clamp(deltaTime, 0, 0.1f);
    }

    private void InitSceneGUIStyles()
    {
        if (sceneGUIStyle == null)
        {
            sceneGUIStyle = new GUIStyle(EditorStyles.miniTextField);
            sceneGUIStyle.alignment = TextAnchor.MiddleCenter;
        }

        if (sceneGUIStyleToolLabel == null)
        {
            sceneGUIStyleToolLabel = new GUIStyle(EditorStyles.textField);
            sceneGUIStyleToolLabel.alignment = TextAnchor.MiddleCenter;
            sceneGUIStyleToolLabel.padding = new RectOffset(-8, -8, -2, 0);
            sceneGUIStyleToolLabel.fontSize = 20;
        }
    }
}
