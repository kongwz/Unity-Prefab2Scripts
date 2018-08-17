using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class PrefabToScriptTemplate
{
    public static string UIClass =
        @"using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
public class #类名# : MonoBehaviour
{
   //auto
   #预设成员变量#
   public void Start()
   {
       #查找#
       #事件#
   }
   #事件方法#
   //这里可以写一些 框架父类的一些方法 和参数设置
}";
}

public class ClientPrefabToScript : Editor {

    public static string _ObjName = "";
    private static string _FindObjStr = "";
    private static string _eventStr = "";
    private static string _eventFunStr = "";
    public static Dictionary<string, string> _ObjTypeList = new Dictionary<string, string>();
    public static Dictionary<string, string> _ObjPathList = new Dictionary<string, string>();
    private static string BtnNameStr = "Btn_";
    private static string ImageNameStr = "Image_";
    private static string TextNameStr = "Text_";
    private static string ToggleNameStr = "Toggle_";
    private static string SliderNameStr = "Slider_";
    private static string ScrollviewNameStr = "Scroll_";
    private static string ScrollbarNameStr = "Scrollbar_";
    private static Transform currSelectObj = null;
    private static string _ObjPathName = "";

    [MenuItem("Client/自动生成客户端UI代码")]
    public static void BuildUICode() {
        ClearData();
        GameObject selectObj = null;
        if (Selection.activeObject == null)
        {
            Debug.Log("未选中组件，或者选中的组件未激活");
        }
        else if (((GameObject)Selection.activeObject).transform.childCount <= 0)
        {
            Debug.Log("您选中的物体该要个孩子了...");
        }
        else {
            selectObj = (GameObject)Selection.activeObject;
            currSelectObj = selectObj.transform;
            if (!ScriptDetection(selectObj.name + "View.cs"))
            {
                Debug.Log("项目中已经存在这个脚本了，给预设换个名字试试吧...");
            }
            else {
                GetObjsDefintion(selectObj.transform);
                GetFindObjsStr();
                CreateScript(selectObj.name + "View");
            }
        }
        
    }

    public static void ClearData() {
        _ObjTypeList.Clear();
        _ObjPathList.Clear();
        _ObjName = "";
        _FindObjStr = "";
        _ObjPathName = "";
        _eventFunStr = "";
        _eventStr = "";
    }

    public static void CreateScript(string scriptName) {
        string scriptPath = Application.dataPath + "/Scripts/" + scriptName + ".cs";
        string classStr = PrefabToScriptTemplate.UIClass;
        classStr = classStr.Replace("#类名#", scriptName);
        classStr = classStr.Replace("#预设成员变量#", _ObjName);
        classStr = classStr.Replace("#查找#", _FindObjStr);
        classStr = classStr.Replace("#事件#", _eventStr);
        classStr = classStr.Replace("#事件方法#", _eventFunStr);

        FileStream file = new FileStream(scriptPath, FileMode.CreateNew);
        StreamWriter filew = new StreamWriter(file, System.Text.Encoding.UTF8);
        filew.Write(classStr);
        filew.Flush();
        filew.Close();
        file.Close();

        Debug.Log("脚本" + scriptName + ".cs 创建成功");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    public static void GetFindObjsStr() {
        foreach (var item in _ObjTypeList) {
            switch (item.Value) {
                case "Button":
                    string btnStr = _ObjPathList[item.Key];
                    _FindObjStr += item.Key + " = transform.Find(\"" + btnStr + "\").GetComponent<Button>(); \n     ";
                    break;
                case "Image":
                    string imgStr = _ObjPathList[item.Key];
                    _FindObjStr += item.Key + " = transform.Find(\"" + imgStr + "\").GetComponent<Image>(); \n     ";
                    break;
                case "Text":
                    string textStr = _ObjPathList[item.Key];
                    _FindObjStr += item.Key + " = transform.Find(\"" + textStr + "\").GetComponent<Text>(); \n     ";
                    break;
                case "Toggle":
                    string ToggleStr = _ObjPathList[item.Key];
                    _FindObjStr += item.Key + " = transform.Find(\"" + ToggleStr + "\").GetComponent<Toggle>(); \n     ";
                    break;
                case "Slider":
                    string SliderStr = _ObjPathList[item.Key];
                    _FindObjStr += item.Key + " = transform.Find(\"" + SliderStr + "\").GetComponent<Slider>(); \n     ";
                    break;
                case "Scrollbar":
                    string ScrollbarStr = _ObjPathList[item.Key];
                    _FindObjStr += item.Key + " = transform.Find(\"" + ScrollbarStr + "\").GetComponent<Scrollbar>(); \n     ";
                    break;
            }
        }
    }

    public static void GetObjsDefintion(Transform tf) {
        if (tf != null) {
            for (int i = 0; i < tf.childCount; i++)
            {
                bool b = false;
                if (tf.GetChild(i).name.StartsWith(BtnNameStr))
                {
                    ObjPathHandle(tf.GetChild(i));
                    NameHandle("Button", tf.GetChild(i).name);
                    _eventStr += "m_" + tf.GetChild(i).name + ".onClick.AddListener(" + tf.GetChild(i).name + "OnClick);\n    ";
                    _eventFunStr += "private void " + tf.GetChild(i).name + "OnClick() \n   {\n     \n   }\n     ";
                }
                else if (tf.GetChild(i).name.StartsWith(ImageNameStr))
                {
                    ObjPathHandle(tf.GetChild(i));
                    NameHandle("Image", tf.GetChild(i).name);
                }
                else if (tf.GetChild(i).name.StartsWith(TextNameStr))
                {
                    ObjPathHandle(tf.GetChild(i));
                    NameHandle("Text", tf.GetChild(i).name);
                }
                else if (tf.GetChild(i).name.StartsWith(ToggleNameStr))
                {
                    ObjPathHandle(tf.GetChild(i));
                    NameHandle("Toggle", tf.GetChild(i).name);
                    _eventStr += "m_" + tf.GetChild(i).name + ".onValueChanged.AddListener(" + tf.GetChild(i).name + "OnChanged);\n    ";
                    _eventFunStr += "private void " + tf.GetChild(i).name + "OnChanged(bool value) \n   {\n     \n   }\n     ";
                }
                else if (tf.GetChild(i).name.StartsWith(SliderNameStr))
                {
                    ObjPathHandle(tf.GetChild(i));
                    NameHandle("Slider", tf.GetChild(i).name);
                    _eventStr += "m_" + tf.GetChild(i).name + ".onValueChanged.AddListener(" + tf.GetChild(i).name + "OnChanged);\n    ";
                    _eventFunStr += "private void " + tf.GetChild(i).name + "OnChanged() \n   {\n     \n   }\n     ";
                }
                else if (tf.GetChild(i).name.StartsWith(ScrollviewNameStr))
                {
                    ObjPathHandle(tf.GetChild(i));
                    NameHandle("Scrollview", tf.GetChild(i).name);
                }
                else if (tf.GetChild(i).name.StartsWith(ScrollbarNameStr))
                {
                    ObjPathHandle(tf.GetChild(i));
                    NameHandle("Scrollbar", tf.GetChild(i).name);
                }
               
                if (tf.GetChild(i).childCount > 0)
                {
                    //_ObjPathName += "/";
                    GetObjsDefintion(tf.GetChild(i));
                }               
            }
        }
    }

    private static void ObjPathHandle(Transform transform) {
        if (_ObjPathName.Equals(""))
        {
            _ObjPathName = transform.name;
        }
        else {
            _ObjPathName = transform.name + "/" + _ObjPathName;
        }
        if (transform.parent != currSelectObj) {
            ObjPathHandle(transform.parent);
        }
    }

    public static void NameHandle(string type , string name) {
        _ObjName += "protected " + type + " m_" + name + " = null;\n    ";
        _ObjTypeList.Add("m_" + name, type);
        _ObjPathList.Add("m_" + name, _ObjPathName);
        _ObjPathName = "";
    }

    public static bool ScriptDetection(string name) {
        name = "Assets/Scripts/" + name;
        string[] assetPath = AssetDatabase.GetAllAssetPaths();
        for (int i = 0; i < assetPath.Length; i++)
        {
            if (assetPath[i].EndsWith(".cs") && assetPath[i].Equals(name)) {
                return false;
            }
        }
        return true;
    }
}
