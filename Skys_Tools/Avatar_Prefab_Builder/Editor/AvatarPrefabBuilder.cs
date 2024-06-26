using UnityEngine;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;

#if VRC_SDK_VRCSDK3
using VRC.SDK3.Avatars.Components;
#endif


public class AvatarPrefabBuilder : EditorWindow
{
    public GameObject Avatar, AvatarClone;
    UnityEngine.Object ExpressionsMenu, ExpressionParms;
    string NewPrefabName  = "";
    string LayerName, SpecialLayerName;
    string LayerName;

    [MenuItem("Sky's Tools/Avatar Prefab Builder")]
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
        //Check to see if the folder does not exist to prevent duplicate blank folders
        if (!Directory.Exists("Assets/Skys_Tools/Avatar_Prefab_Builder/Avatars/" + NewPrefabName))
        {
            AssetDatabase.CreateFolder("Assets/Skys_Tools/Avatar_Prefab_Builder/Avatars", NewPrefabName);
            AssetDatabase.CreateFolder("Assets/Skys_Tools/Avatar_Prefab_Builder/Avatars/" + NewPrefabName, "Animations");
            AssetDatabase.CreateFolder("Assets/Skys_Tools/Avatar_Prefab_Builder/Avatars/" + NewPrefabName, "Expressions");
        }

          //Make sure the GameObject for the copy from Prefab is active
          if (!Avatar.activeSelf)
          {
              Avatar.SetActive(true);
          }
        
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
        var NewMenu = AssetDatabase.LoadAssetAtPath("Assets/Skys_Tools/Avatar_Prefab_Builder/Avatars/" + NewPrefabName + "/Expressions/" + NewPrefabName + "_Menu.asset", typeof(VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu)) as VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu;
        avatarDescriptor2.expressionsMenu = NewMenu;
        AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(Parms), "Assets/Skys_Tools/Avatar_Prefab_Builder/Avatars/" + NewPrefabName + "/Expressions/" + NewPrefabName + "_Parms.asset");
        var NewParms = AssetDatabase.LoadAssetAtPath("Assets/Skys_Tools/Avatar_Prefab_Builder/Avatars/" + NewPrefabName + "/Expressions/" + NewPrefabName + "_Parms.asset", typeof(VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters)) as VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters;
        avatarDescriptor2.expressionParameters = NewParms;


        //Make copies of the Special Player Layers

        var SpecialLayers = avatarDescriptor.specialAnimationLayers;

        int count2 = 0;

        foreach (var Special in SpecialLayers)
        {
            if (Special.isDefault == false)
            {
                switch (count2)
                {
                    case 0:
                        SpecialLayerName = "Sitting";
                        break;
                    case 1:
                        SpecialLayerName = "TPose";
                        break;
                    case 2:
                        SpecialLayerName = "IKPose";
                        break;
                }

                AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(Special.animatorController), "Assets/Skys_Tools/Avatar_Prefab_Builder/Avatars/" + NewPrefabName + "/Animations/" + NewPrefabName + "_" + SpecialLayerName + ".controller");

                var SpecialLayerCopy = AssetDatabase.LoadAssetAtPath("Assets/Skys_Tools/Avatar_Prefab_Builder/Avatars/" + NewPrefabName + "/Animations/" + NewPrefabName + "_" + SpecialLayerName + ".controller", typeof(AnimatorController)) as AnimatorController;
                avatarDescriptor2.specialAnimationLayers[count2].animatorController = SpecialLayerCopy;

            }




            //Once the copy is completed hide old prefab
            if (Avatar.activeSelf)
        {
            Avatar.SetActive(false);
        }
    
    
    }
   
        
    }




