using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditor.IMGUI.Controls;
using GTK.Download;

public class DownloadWindow : EditorWindow 
{
	AnimBool m_ShowExtraFields;
    string m_String;
    Color m_Color = Color.white;
    int m_Number = 0;
    List<bool> foldOuts = new List<bool>();
    [MenuItem("Window/Download")]
	private static void ShowWindow() {
		var window = GetWindow<DownloadWindow>();
		window.titleContent = new GUIContent("Download");
		window.Show();
	}


    void OnEnable()
    {
        m_ShowExtraFields = new AnimBool(true);
       // m_ShowExtraFields.valueChanged = Repaint;
    }

    void OnInspectorUpdate()
    {
        Repaint();
    }
    
    void OnGUI()
    {
        for (int i = 0; i < WebFileDownloader.Downloaders.Count; i++)
        {
            if (foldOuts.Count - 1 < i)
            {
                foldOuts.Add(false);
            }
            WebFileDownloader downloader = WebFileDownloader.Downloaders[i];
            foldOuts[i] = EditorGUILayout.Foldout(foldOuts[i], WebFileDownloader.Downloaders[i].Name);
            if (foldOuts[i])
            {
                for (int j = 0; j < downloader.GetBlockCount; j++)
                {
                    Rect r = EditorGUILayout.BeginVertical();
                    EditorGUI.DrawRect(r, Color.white);
                    EditorGUI.DrawRect(new Rect(r.x, r.y, r.width * downloader.GetProgress(j), r.height), Color.cyan);
                    //EditorGUILayout.LabelField("------" + downloader.blocks[j].Progress + "------");
                    GUILayout.Space(5);
                    EditorGUILayout.EndVertical();
                    GUILayout.Space(2);
                }
            }
        }
    }
}
