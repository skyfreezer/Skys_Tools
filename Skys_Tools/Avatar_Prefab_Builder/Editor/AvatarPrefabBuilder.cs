using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Presets;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEditor.VersionControl;
using VRC.SDK3.Avatars.ScriptableObjects;
using System;
using static UnityEditor.Experimental.GraphView.GraphView;
using Palmmedia.ReportGenerator.Core.Reporting.Builders.Rendering;







#if VRC_SDK_VRCSDK3
using VRC.SDK3.Avatars.Components;
using VRC.SDKBase;
#endif


public class AvatarPrefabBuilder : EditorWindow
{
    public GameObject Avatar, AvatarClone;
    UnityEngine.Object ExpressionsMenu, ExpressionParms;
    AnimatorController FX_Layer, Base_Layer, Gesture_Layer, Action_Layer;
    bool CustomLayers, CustomExpressions;
    string NewPrefabName;
    string RootLocation= "Assets/Skys_Tools/Avatar_Prefab_Builder/Avatars/";
    string LayerName;
    string btnYes, btnNo;

    [MenuItem("Tools/Sky's Tools/Avatar Prefab Builder")]
    public static void ShowWindow()
    {
        GetWindow(typeof(AvatarPrefabBuilder), false, "Avatar Prefab Builder", true);   

    }


    private void OnGUI()
    {


        GUILayout.Label("Avatar Prefab Settings", EditorStyles.boldLabel);
        Avatar = EditorGUILayout.ObjectField("Copy From Prefab", Avatar, typeof(GameObject), true) as GameObject;
        NewPrefabName = EditorGUILayout.TextField("New Prefab Name", NewPrefabName);


        if (GUILayout.Button("Build Avatar Prefab"))
        {

            if (NewPrefabName != "")
            {
                if(Avatar != null)
                {               
                    if (Directory.Exists("Assets/Skys_Tools/Avatar_Prefab_Builder/Avatars/" + NewPrefabName))
                    {
                        if (EditorUtility.DisplayDialog("File Duplicate Warning", "Files have already been created for this prefeb. Would you like to overwrite them?", "Yes", "No"))
                        {
                            CreateNewPrefab();
                        }
                    }
                    else
                    {
                        CreateNewPrefab();
                    }
                }
                else
                {
                    Debug.LogError("[Avatar Prefab Builder] Copy from Prefab can not be left empty.");
                }
            }
            else
            {
                Debug.LogError("[Avatar Prefab Builder] No Prefab name was entered. Please enter a New Prefab name before clicking the Build Avatar Prefab button");
            }
        }
    }

    private void BuildAssetFolders()
    {
        //Create all the folders used by the Script Tool
        if (!Directory.Exists("Assets/Skys_Tools/Avatar_Prefab_Builder/Avatars")){
            AssetDatabase.CreateFolder("Assets/Skys_Tools/Avatar_Prefab_Builder", "Avatars");
        }
        else
        {
            Debug.Log("Could not find location to create Asset files");
        }
    }


   
    private void CreateNewPrefab()
    {
        BuildAssetFolders();

        AssetDatabase.CreateFolder("Assets/Skys_Tools/Avatar_Prefab_Builder/Avatars", NewPrefabName);
        AssetDatabase.CreateFolder("Assets/Skys_Tools/Avatar_Prefab_Builder/Avatars/" + NewPrefabName, "Animations");
        AssetDatabase.CreateFolder("Assets/Skys_Tools/Avatar_Prefab_Builder/Avatars/" + NewPrefabName, "Expressions");

        //Clone Prefab and rename based on users input field
        Instantiate(Avatar, new Vector3(0, 0, 0), Quaternion.identity);
        AvatarClone = GameObject.Find(Avatar.name + "(Clone)");
        AvatarClone.name = NewPrefabName;


        //Make copies of all AnimationsLayers and copy them to the new prefab
        VRCAvatarDescriptor avatarDescriptor = Avatar.GetComponent<VRCAvatarDescriptor>();
        VRCAvatarDescriptor avatarDescriptor2 = AvatarClone.GetComponent<VRCAvatarDescriptor>();
        var Layers = avatarDescriptor.baseAnimationLayers;

        int count = 0;

        foreach (var layer in Layers)
        {
            if (layer.isDefault == false)
            {
                switch (count)
                {
                    case 0:
                        LayerName = "Base";
                        break;
                    case 1:
                        LayerName = "Additive";
                        break;
                    case 2:
                        LayerName = "Gesture";
                        break;
                    case 3:
                        LayerName = "Action";
                        break;
                    case 4:
                        LayerName = "FX";
                        break;
                }
                AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(layer.animatorController), "Assets/Skys_Tools/Avatar_Prefab_Builder/Avatars/" + NewPrefabName + "/Animations/" + NewPrefabName + "_" + LayerName + ".controller");

                var LayerCopyFromLocation = AssetDatabase.LoadAssetAtPath("Assets/Skys_Tools/Avatar_Prefab_Builder/Avatars/" + NewPrefabName + "/Animations/" + NewPrefabName + "_" + LayerName + ".controller", typeof(AnimatorController)) as AnimatorController;
                avatarDescriptor2.baseAnimationLayers[count].animatorController = LayerCopyFromLocation;
            }
            count++;
        }


        //Make a copy of the expressions menu and copy them to the new prefab

        var Menu = avatarDescriptor.expressionsMenu;
        var Parms = avatarDescriptor.expressionParameters;

        AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(Menu), "Assets/Skys_Tools/Avatar_Prefab_Builder/Avatars/" + NewPrefabName + "/Expressions/" + NewPrefabName + "_Menu.asset");
        AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(Parms), "Assets/Skys_Tools/Avatar_Prefab_Builder/Avatars/" + NewPrefabName + "/Expressions/" + NewPrefabName + "_Parms.asset");
        avatarDescriptor2.expressionsMenu = Menu;
        avatarDescriptor2.expressionParameters = Parms;
    }
   
        
    }




