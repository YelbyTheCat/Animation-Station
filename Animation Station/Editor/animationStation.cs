using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class animationStation : EditorWindow
{
    //Blendshapes - Case 0
    public GameObject objectShape;
    public SkinnedMeshRenderer blendshapesMesh;
    public List<float> slidersIndex = new List<float>();
    public Vector2 scrollBarLocation;
    public string searchBarSearch = "";

    //ToolBar
    public int toolbarIndex = 0;
    public string[] toolbar = { "Blendshapes" };

    [MenuItem("Yelby/Animation Station")]
    public static void ShowWindow() { GetWindow<animationStation>("Animation Station"); }

    private void OnGUI()
    {
        GUILayout.Label("Version: 1.0");

        switch(toolbarIndex)
        {
            case 0:
                {
                    objectShape = EditorGUILayout.ObjectField("Object: ", objectShape, typeof(GameObject), true) as GameObject;
                    if (objectShape != null)
                    {
                        blendshapesMesh = getBlendshapes(objectShape);
                        guiSliders(blendshapesMesh);
                        if(GUILayout.Button("Zero Out"))
                        {
                            for(int i = 0; i < slidersIndex.Count; i++)
                            {
                                blendshapesMesh.SetBlendShapeWeight(i, 0.0f);
                            }
                        }
                    }
                    break;
                }
        }
    }

    private SkinnedMeshRenderer getBlendshapes(GameObject obj)
    {
        //Get the Body part of the bodymesh
        GameObject bodyMesh = null;
        for(int i = 0; i < obj.transform.childCount; i++)
        {
            if(obj.transform.GetChild(i).gameObject.name == "Body")
            {
                bodyMesh = obj.transform.GetChild(i).gameObject;
                break;
            }
        }

        //Get the blendshapes
        return bodyMesh.GetComponent<SkinnedMeshRenderer>();
    }

    private void guiSliders(SkinnedMeshRenderer blendshapesMesh)
    {
        Mesh blendshapes = blendshapesMesh.sharedMesh;
        int size = blendshapes.blendShapeCount;

        if(slidersIndex.Count < size)
        {
            for(int i = 0; i < size; i++)
            {
                slidersIndex.Add(0.0f);
            }
        }

        searchBarSearch = EditorGUILayout.TextField("Search: ", searchBarSearch);
        scrollBarLocation = EditorGUILayout.BeginScrollView(scrollBarLocation);
        for (int i = 0; i < size; i++)
        {
            slidersIndex[i] = blendshapesMesh.GetBlendShapeWeight(i);
            string name = blendshapes.GetBlendShapeName(i).ToLower();
            if (name.Contains(searchBarSearch.ToLower()))
            {
                slidersIndex[i] = EditorGUILayout.Slider(blendshapes.GetBlendShapeName(i), slidersIndex[i], 0, 100);
            }
            blendshapesMesh.SetBlendShapeWeight(i, slidersIndex[i]);
        }
        EditorGUILayout.EndScrollView();
    }
}
