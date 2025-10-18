using System;
using UnityEngine;
using FronkonGames.LUTs.Horror;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(HorrorDemo))]
public class HorrorDemoWarning : Editor
{
  private GUIStyle Style => style ??= new GUIStyle(GUI.skin.GetStyle("HelpBox")) { richText = true, fontSize = 14, alignment = TextAnchor.MiddleCenter };
  private GUIStyle style;
  public override void OnInspectorGUI()
  {
    EditorGUILayout.TextArea($"\nThis code is only for the demo\n\n<b>DO NOT USE</b> it in your projects\n\nIf you have any questions,\ncheck the <a href='{Constants.Support.Documentation}'>online help</a> or use the <a href='mailto:{Constants.Support.Email}'>support email</a>,\n<b>thanks!</b>\n", Style);
    this.DrawDefaultInspector();
  }
}
#endif

/// <summary> LUTs: Horror demo. </summary>
/// <remarks>
/// This code is designed for a simple demo, not for production environments.
/// </remarks>
public class HorrorDemo : MonoBehaviour
{
  [SerializeField]
  private Profile[] profiles;

  private Horror.Settings settings;

  private GUIStyle styleTitle;
  private GUIStyle styleLabel;
  private GUIStyle styleButton;

  private readonly Dictionary<string, List<Profile>> catalogue = new();
  private readonly List<bool> catalogueEnable = new();
  private readonly List<int> profilesIndex = new();

  private int currentCatalogueIndex;
  private int currentProfileIndex;

  private void ResetEffect()
  {
    settings.ResetDefaultValues();

    currentCatalogueIndex = currentProfileIndex = 0;

    for (int i = 0; i < catalogueEnable.Count; ++i)
      catalogueEnable[i] = i == currentCatalogueIndex;

    for (int i = 0; i < profilesIndex.Count; ++i)
      profilesIndex[i] = 0;
  }

  private void Awake()
  {
    if (Horror.IsInRenderFeatures() == false)
    {
      Debug.LogWarning($"Effect '{Constants.Asset.Name}' not found. You must add it as a Render Feature.");
#if UNITY_EDITOR
      if (EditorUtility.DisplayDialog($"Effect '{Constants.Asset.Name}' not found", $"You must add '{Constants.Asset.Name}' as a Render Feature.", "Quit") == true)
        EditorApplication.isPlaying = false;
#endif
    }

    for (int i = 0; i < profiles.Length; ++i)
    {
      string name = profiles[i].name[..profiles[i].name.IndexOf("_")];
      if (catalogue.ContainsKey(name) == true)
        catalogue[name].Add(profiles[i]);
      else
      {
        catalogue.Add(name, new List<Profile>() { profiles[i] });
        catalogueEnable.Add(catalogueEnable.Count == 0);
        profilesIndex.Add(0);
      }
    }

    this.enabled = Horror.IsInRenderFeatures();
  }

  private void Start()
  {
    settings = Horror.Instance.settings;
    ResetEffect();
  }

  private void OnGUI()
  {
    styleTitle = new GUIStyle(GUI.skin.label)
    {
      alignment = TextAnchor.LowerCenter,
      fontSize = 26,
      fontStyle = FontStyle.Bold
    };

    styleLabel = new GUIStyle(GUI.skin.label)
    {
      alignment = TextAnchor.MiddleLeft,
      fontSize = 20
    };

    styleButton = new GUIStyle(GUI.skin.button)
    {
      fontSize = 20
    };

    GUILayout.BeginHorizontal();
    {
      const float width = 400.0f;
      GUILayout.BeginVertical("box", GUILayout.Width(width), GUILayout.Height(Screen.height));
      {
        const float space = 10.0f;

        GUILayout.Space(space);

        GUILayout.Label("HORROR DEMO", styleTitle);

        GUILayout.Space(space);

        settings.intensity = SliderField("Intensity", settings.intensity);

        if (catalogue.Keys.Count > 0)
        {
          for (int i = 0; i < catalogue.Keys.Count; ++i)
          {
            GUILayout.BeginHorizontal();
            {
              catalogueEnable[i] = GUILayout.Toggle(catalogueEnable[i], string.Empty, GUILayout.Width(16));

              GUILayout.Label(catalogue.Keys.ElementAt(i), styleLabel, GUILayout.Width(150));

              GUI.enabled = i == currentCatalogueIndex;

              int profileIndex = (int)GUILayout.HorizontalSlider(profilesIndex[i], 0, catalogue[catalogue.Keys.ElementAt(i)].Count - 1, GUILayout.Width(width - 150 - 16));
              if (profileIndex != profilesIndex[i])
              {
                profilesIndex[i] = currentProfileIndex = profileIndex;
                settings.profile = catalogue[catalogue.Keys.ElementAt(currentCatalogueIndex)][currentProfileIndex];
              }

              GUI.enabled = true;
            }
            GUILayout.EndHorizontal();
          }

          for (int i = 0; i < catalogue.Keys.Count; ++i)
          {
            if (catalogueEnable[i] == true && currentCatalogueIndex != i)
            {

              catalogueEnable[currentCatalogueIndex] = false;
              currentCatalogueIndex = i;
              currentProfileIndex = profilesIndex[currentCatalogueIndex];
              catalogueEnable[currentCatalogueIndex] = true;
              settings.profile = catalogue[catalogue.Keys.ElementAt(currentCatalogueIndex)][currentProfileIndex];
            }
          }
        }

        GUILayout.Space(space * 2.0f);

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("RESET", styleButton) == true)
          ResetEffect();

        GUILayout.Space(4.0f);

        if (GUILayout.Button("ONLINE DOCUMENTATION", styleButton) == true)
          Application.OpenURL(Constants.Support.Documentation);

        GUILayout.Space(4.0f);

        if (GUILayout.Button("❤️ LEAVE A REVIEW ❤️", styleButton) == true)
          Application.OpenURL(Constants.Support.Store);

        GUILayout.Space(space * 2.0f);
      }
      GUILayout.EndVertical();

      GUILayout.FlexibleSpace();
    }
    GUILayout.EndHorizontal();
  }

  private void OnDestroy()
  {
    settings?.ResetDefaultValues();
  }

  private bool ToggleField(string label, bool value)
  {
    GUILayout.BeginHorizontal();
    {
      GUILayout.Label(label, styleLabel);

      value = GUILayout.Toggle(value, string.Empty);
    }
    GUILayout.EndHorizontal();

    return value;
  }

  private float SliderField(string label, float value, float min = 0.0f, float max = 1.0f)
  {
    GUILayout.BeginHorizontal();
    {
      GUILayout.Label(label, styleLabel);

      value = GUILayout.HorizontalSlider(value, min, max);
    }
    GUILayout.EndHorizontal();

    return value;
  }

  private int SliderField(string label, int value, int min, int max)
  {
    GUILayout.BeginHorizontal();
    {
      GUILayout.Label(label, styleLabel);

      value = (int)GUILayout.HorizontalSlider(value, min, max);
    }
    GUILayout.EndHorizontal();

    return value;
  }

  private Color ColorField(string label, Color value, bool alpha = true)
  {
    GUILayout.BeginHorizontal();
    {
      GUILayout.Label(label, styleLabel);

      float originalAlpha = value.a;

      Color.RGBToHSV(value, out float h, out float s, out float v);
      h = GUILayout.HorizontalSlider(h, 0.0f, 1.0f);
      value = Color.HSVToRGB(h, s, v);

      if (alpha == false)
        value.a = originalAlpha;
    }
    GUILayout.EndHorizontal();

    return value;
  }

  private Vector3 Vector3Field(string label, Vector3 value, string x = "X", string y = "Y", string z = "Z", float min = 0.0f, float max = 1.0f)
  {
    GUILayout.Label(label, styleLabel);

    value.x = SliderField($"   {x}", value.x, min, max);
    value.y = SliderField($"   {y}", value.y, min, max);
    value.z = SliderField($"   {z}", value.z, min, max);

    return value;
  }

  private T EnumField<T>(string label, T value) where T : Enum
  {
    string[] names = System.Enum.GetNames(typeof(T));
    Array values = System.Enum.GetValues(typeof(T));
    int index = Array.IndexOf(values, value);

    GUILayout.BeginHorizontal();
    {
      GUILayout.Label(label, styleLabel);

      if (GUILayout.Button("<", styleButton) == true)
        index = index > 0 ? index - 1 : values.Length - 1;

      GUILayout.Label(names[index], styleLabel);

      if (GUILayout.Button(">", styleButton) == true)
        index = index < values.Length - 1 ? index + 1 : 0;
    }
    GUILayout.EndHorizontal();

    return (T)(object)index;
  }
}
