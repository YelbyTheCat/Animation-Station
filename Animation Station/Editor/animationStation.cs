using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class animationStation : EditorWindow
{
    public string root = "Assets/Yelby/Programs/Animation Station/";

    //Blendshapes - Case 0
    public GameObject objectShape;
    public SkinnedMeshRenderer blendshapesMesh;
    public List<float> slidersIndex = new List<float>();
    public Vector2 scrollBarLocation;
    public string searchBarSearch = "";
    public string location;
    public string animationName;
    public AnimationClip animation;
    public bool includeVRC = false;
    public bool hideVRC = true;

    //ToolBar
    public int toolbarIndex = 0;
    public string[] toolbar = { "Blendshapes" };

    [MenuItem("Yelby/Animation Station")]
    public static void ShowWindow() { GetWindow<animationStation>("Animation Station"); }

    private void OnGUI()
    {
        GUILayout.Label("Version: 1.2");

        switch(toolbarIndex)
        {
            case 0:
                {
                    objectShape = EditorGUILayout.ObjectField("Object: ", objectShape, typeof(GameObject), true) as GameObject;
                    if (objectShape != null)
                    {
                        EditorGUILayout.BeginHorizontal();

                        animation = EditorGUILayout.ObjectField("Animation: ", animation, typeof(AnimationClip), true) as AnimationClip;
                        if(GUILayout.Button("Import"))
                        {
                            importAnimation(animation, blendshapesMesh);
                        }

                        EditorGUILayout.EndHorizontal();
                        if (location == null)
                        {
                            location = "Assets/Yelby/Programs/Animation Station/" + objectShape.name + "/";
                        }

                        EditorGUILayout.BeginHorizontal();
                        animationName = EditorGUILayout.TextField("Animation Name: ", animationName);
                        if (animationName != null)
                            animationName = cleanName(animationName);
                        else
                            animationName = "Yelby";

                        if (GUILayout.Button("Generate Animation"))
                        {
                            if(location == "Assets/Yelby/Programs/Animation Station/" + objectShape.name)
                                createFolder(root, objectShape);
                            createAnimation(location, blendshapesMesh, animationName, includeVRC);
                        }

                        if (GUILayout.Button("Zero Out"))
                        {
                            for (int i = 0; i < slidersIndex.Count; i++)
                            {
                                blendshapesMesh.SetBlendShapeWeight(i, 0.0f);
                            }
                        }
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        includeVRC = EditorGUILayout.Toggle("Include vrc.", includeVRC);
                        hideVRC = EditorGUILayout.Toggle("Hide vrc.", hideVRC);
                        EditorGUILayout.EndHorizontal();

                        blendshapesMesh = getBlendshapes(objectShape);
                        guiSliders(blendshapesMesh, includeVRC, hideVRC);
                    }
                    break;
                }
        }
    }

    /*~~~~~Methods~~~~~*/

    //BlendShapes
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

    private void guiSliders(SkinnedMeshRenderer blendshapesMesh, bool includeVRC, bool hideVRC)
    {
        Mesh blendshapes = blendshapesMesh.sharedMesh;
        int size = blendshapes.blendShapeCount;

        searchBarSearch = EditorGUILayout.TextField("Search: ", searchBarSearch);
        scrollBarLocation = EditorGUILayout.BeginScrollView(scrollBarLocation);
        for (int i = 0; i < size; i++)
        {
            if(slidersIndex.Count < size)
                slidersIndex.Add(0.0f);

            slidersIndex[i] = blendshapesMesh.GetBlendShapeWeight(i);

            string name = blendshapes.GetBlendShapeName(i).ToLower();
            if (name.Contains(searchBarSearch.ToLower()))
            {
                if (hideVRC)
                    if (blendshapes.GetBlendShapeName(i).Contains("vrc."))
                        continue;
                slidersIndex[i] = EditorGUILayout.Slider(blendshapes.GetBlendShapeName(i), slidersIndex[i], 0, 100);
            }
            blendshapesMesh.SetBlendShapeWeight(i, slidersIndex[i]);
        }
        EditorGUILayout.EndScrollView();
    }

    private void createAnimation(string location, SkinnedMeshRenderer blendshapeMesh, string name, bool includeVRC)
    {
        AnimationClip clip = new AnimationClip();
        clip.legacy = false;
        Keyframe[] keys = new Keyframe[1];
        AnimationCurve curve;

        Mesh blenshapes = blendshapeMesh.sharedMesh;
        int size = blenshapes.blendShapeCount;

        for(int i = 0; i < size; i++)
        {
            if(!includeVRC)
            {
                if (blenshapes.GetBlendShapeName(i).Contains("vrc."))
                    continue;
            }

            keys[0] = new Keyframe(0, blendshapeMesh.GetBlendShapeWeight(i)); //Set the key

            //Add the key
            curve = new AnimationCurve(keys);
            clip.SetCurve(blendshapeMesh.name, typeof(SkinnedMeshRenderer), "blendShape." + blenshapes.GetBlendShapeName(i), curve);
        }

        AssetDatabase.CreateAsset(clip, location + name + ".anim");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private void createFolder(string root, GameObject obj)
    {
        //Clean name
        string name = cleanName(obj.name);

        string tempRoot = root;
        tempRoot = tempRoot.Substring(0, tempRoot.Length - 1);

        if (!AssetDatabase.IsValidFolder(root + name))
        {
            AssetDatabase.CreateFolder(tempRoot, name);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private string cleanName(string animationName)
    {
        string name = animationName;

        string[] characters = { "<", ">", ":", "/", "\\", "\"", "|", "?", "*", ".", "," };
        for (int i = 0; i < characters.Length; i++)
        {
            name = name.Replace(characters[i], "");
        }

        return name;
    }

    private void importAnimation(AnimationClip animation, SkinnedMeshRenderer skinnedMesh)
    {
        Mesh blendshapes = skinnedMesh.sharedMesh;
        var clip = AnimationUtility.GetAllCurves(animation);
        string name = "";

        for(int i = 0; i < clip.Length; i++)
        {
            //Debug.Log("i: " + clip[i].propertyName.Substring(11));
            name = clip[i].propertyName.Substring(11);
            for (int j = 0; j < blendshapes.blendShapeCount; j++)
            {
                //Debug.Log("j: " + blendshapes.GetBlendShapeName(j));
                if(name == blendshapes.GetBlendShapeName(j))
                {
                    //Debug.Log(clip[i].propertyName + " == " + blendshapes.GetBlendShapeName(j));
                    skinnedMesh.SetBlendShapeWeight(j, clip[i].curve.keys[0].value);
                }
            }
        }
    }
}
