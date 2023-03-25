using UnityEngine;

namespace Modifiers
{
    [ExecuteInEditMode]
    public class Array : MonoBehaviour
    {
        public GameObject prefab;
        public Transform parent;
        public Vector3 offset;
        public int count;

        public Transform[] children;

        //private static EditorWindow window;
        //private Vector2 scrollPos;

        public bool useManuallySetChildren;

        public bool set;

        //----------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------FOR SCRIPTABLEWIZARD--------------------------------------------------------------------
        //----------------------------------------------------------------------------------------------------------------------------------
        /*
        [MenuItem("Modifiers/Array")]
        static void CreateArray()
        {
            DisplayWizard<Array>("Create Array", "Create", "Apply");
            //If you don't want to use the secondary button simply leave it out:
            //DisplayWizard<Array>("Create Array", "Create");
        }

        //Called when a ScriptableWizard's createButton is pressed
        void OnWizardCreate()
        {
            if(prefab && parent)
            {
                GameObject g;

                children = new Transform[count];

                for(int i = 0; i < count; i++)
                {
                    g = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                    Debug.LogError(g.name);
                    children[i] = g.transform;
                    g.transform.parent = parent;

                    if(i == 0)
                        g.transform.position = parent.transform.position;
                    else
                        g.transform.position = children[i - 1].transform.position + offset;
                }
            }
            else
            {
                helpString = "prefab or parent is null";
            }
        }

        void OnWizardUpdate()
        {
            //helpString = "Please set the color of the light!";
        }

        //Called when a ScriptableWizard's otherButton is pressed
        void OnWizardOtherButton()
        {

        }

        //----------------------------------------------------------------------------------------------------------------------------------
        */


        //----------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------FOR EDITORWINDOW------------------------------------------------------------------------
        //----------------------------------------------------------------------------------------------------------------------------------

        /*[MenuItem("Tools/Array")]
        public static void ShowWindow()
        {
            window = GetWindow(typeof(Array), false);
        }

        private void OnInspectorUpdate()
        {
            if(!window)
                window = GetWindow(typeof(Array), false);
        }

        private void OnGUI()
        {
            if(window)
            {
                GUILayout.BeginArea(new Rect(0, 0, window.position.size.x, window.position.size.y));
                GUILayout.BeginVertical();
                scrollPos = GUILayout.BeginScrollView(scrollPos, false, true, GUILayout.ExpandHeight(true));
            }


            GUIStyle BigBold = new GUIStyle();
            BigBold.fontSize = 16;
            BigBold.fontStyle = FontStyle.Bold;
            BigBold.wordWrap = true;
            BigBold.alignment = TextAnchor.MiddleCenter;

            GUIStyle Wrap = new GUIStyle();
            Wrap.wordWrap = true;
            Wrap.alignment = TextAnchor.MiddleCenter;

            GUIStyle warn = new GUIStyle();
            warn.richText = true;
            warn.wordWrap = true;
            warn.fontStyle = FontStyle.Bold;
            warn.alignment = TextAnchor.MiddleCenter;
            warn.normal.textColor = new Color(0.7f, 0, 0);

            GUIStyle preview = new GUIStyle();
            preview.alignment = TextAnchor.UpperCenter;

            GUILayout.Space(10f);
            GUILayout.Label("Add Textures to be packed", BigBold);
            //GUILayout.Label("Warning Text", warn);
            GUILayout.Space(10f);


            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Space(10f);
            //Albedo = (Texture2D)EditorGUILayout.ObjectField("Albedo", Albedo, typeof(Texture2D), false);
            parent = (Transform)EditorGUILayout.ObjectField("Parent", parent, typeof(Transform), false);

            GUILayout.Space(10f);
            GUILayout.EndVertical();

            GUILayout.Label("Output texture will be the same height and width as input", Wrap);

            GUILayout.Space(100);
            if(window)
            {
                GUILayout.EndScrollView();
                GUILayout.EndVertical();
                GUILayout.EndArea();
            }
        }*/

        //----------------------------------------------------------------------------------------------------------------------------------

        //----------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------FOR MONOBEHAVIOUR-----------------------------------------------------------------------
        //----------------------------------------------------------------------------------------------------------------------------------
        private void Update()
        {
            if(set)
            {
                CreateArray();


                set = false;
            }
        }

        private void CreateArray()
        {
            if(prefab && parent)
            {
                GameObject g;

                if(!useManuallySetChildren)
                    children = new Transform[count];

                for(int i = 0; i < count; i++)
                {
                    if(!useManuallySetChildren)
                    {
                        g = Instantiate(prefab);

                        children[i] = g.transform;
                    }
                    else
                    {
                        g = children[i].gameObject;
                    }

                    g.transform.parent = parent;

                    if(i == 0)
                        g.transform.position = parent.transform.position;
                    else
                        g.transform.position = children[i - 1].transform.position + offset;
                }
            }
        }
    }
}
