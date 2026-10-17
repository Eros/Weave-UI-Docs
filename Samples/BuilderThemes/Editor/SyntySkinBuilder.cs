using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace WeaveUI.Samples.BuilderThemes.Editor
{
    public static class SyntySkinBuilder
    {
        private const string Prefix = "SPR_FantasyMenus_";
        private const string Pack = "https://syntystore.com/products/interface-fantasy-menus";

        /// <summary>Where BuilderThemesSample looks for the skin, under a Resources folder.</summary>
        public const string ResourcePath = "WeaveUISynty/SyntyFantasySkin";

        [MenuItem("Tools/Weave UI Samples/Build Synty Fantasy Menus Skin")]
        public static void BuildFromMenu()
        {
            var result = Build();
            EditorUtility.DisplayDialog("Synty Fantasy Menus skin", result, "OK");
        }

        public static string Build()
        {
            var button = Find("Menu_Button_01_Background");
            var buttonFrame = Find("Menu_Button_01_Frame");
            var panel = Find("Frame_Box_Medium_01_Mask");
            var panelFrame = Find("Frame_Box_Medium_01");
            if (button == null || buttonFrame == null || panel == null || panelFrame == null)
            {
                return "Couldn't find Synty's INTERFACE - Fantasy Menus sprites in this project.\n\n" +
                       $"Import the pack ({Pack}) from the Synty Store or the Package Manager, then run this again.";
            }

            // Next to the pack, so the skin stays with the art it was built from.
            var root = $"{Path.GetDirectoryName(Path.GetDirectoryName(button))!.Replace('\\', '/')}/WeaveSkin";
            var folder = $"{root}/Resources/{Path.GetDirectoryName(ResourcePath)}";
            Directory.CreateDirectory($"{root}/Sprites");
            Directory.CreateDirectory(folder);

            // Synty draws a button as a white fill tinted per state, plus a frame on top. USS has one background
            // image per element, so each state is flattened into one sprite, keeping Synty's 9-slice borders.
            var buttonBorder = new Vector4(500, 0, 130, 0);
            Sprite Button(string name, string fill) => Flatten($"{root}/Sprites/{name}.png", button, Hex(fill), buttonFrame, buttonBorder);
            var sprites = new Dictionary<string, Sprite>
            {
                ["button"] = Button("button-normal", "#3b2a4f"),
                ["button-hover"] = Button("button-hover", "#56407a"),
                ["button-active"] = Button("button-active", "#25192f"),
                ["primary"] = Button("primary-normal", "#1f6b45"),
                ["primary-hover"] = Button("primary-hover", "#2b8c5c"),
                ["primary-active"] = Button("primary-active", "#134a2f"),
                ["panel"] = Flatten($"{root}/Sprites/panel.png", panel, Hex("#1c1428"), panelFrame, new Vector4(180, 180, 180, 180)),
            };

            var skinPath = $"{folder}/{Path.GetFileName(ResourcePath)}.asset";
            var skin = AssetDatabase.LoadAssetAtPath<UISkin>(skinPath);
            if (skin == null)
            {
                skin = ScriptableObject.CreateInstance<UISkin>();
                AssetDatabase.CreateAsset(skin, skinPath);
            }

            skin.skinName = "synty-fantasy";
            var cream = Hex("#f6ead0");
            SkinState State(SkinStateKind kind, Sprite sprite, float scale, Vector4? padding = null) => new(kind)
            {
                sprite = sprite,
                sliceScale = scale,
                useTextColor = kind == SkinStateKind.Normal,
                textColor = cream,
                usePadding = padding.HasValue,
                padding = padding ?? Vector4.zero,
            };

            // The button art is 1024 x 256 with a 500 px gem on the left. At 0.2 scale the gem is 100 px wide, so the
            // text is padded past it, and the vertical padding keeps buttons about 51 px tall so the gem keeps its shape.
            var buttonPadding = new Vector4(88, 13, 28, 13);
            skin.components = new List<SkinComponent>
            {
                new()
                {
                    name = "Panel", selector = ".panel",
                    states = new List<SkinState> { State(SkinStateKind.Normal, sprites["panel"], 0.25f, new Vector4(48, 48, 48, 48)) },
                },
                new()
                {
                    name = "Button", selector = ".unity-button",
                    states = new List<SkinState>
                    {
                        State(SkinStateKind.Normal, sprites["button"], 0.2f, buttonPadding),
                        State(SkinStateKind.Hover, sprites["button-hover"], 0.2f),
                        State(SkinStateKind.Focus, sprites["button-hover"], 0.2f),
                        State(SkinStateKind.Active, sprites["button-active"], 0.2f),
                    },
                },
                new()
                {
                    name = "Primary button", selector = ".btn--primary",
                    states = new List<SkinState>
                    {
                        State(SkinStateKind.Normal, sprites["primary"], 0.2f, buttonPadding),
                        State(SkinStateKind.Hover, sprites["primary-hover"], 0.2f),
                        State(SkinStateKind.Focus, sprites["primary-hover"], 0.2f),
                        State(SkinStateKind.Active, sprites["primary-active"], 0.2f),
                    },
                },
            };

            EditorUtility.SetDirty(skin);
            AssetDatabase.SaveAssetIfDirty(skin);
            WeaveUI.Editor.SkinUssGenerator.Generate(skin);
            return $"Built {skinPath}.\n\nPress Play in the Builder Themes sample and pick Synty from the Skin dropdown.";
        }

        /// <summary>The project path of one of the pack's sprites, wherever the pack was imported.</summary>
        private static string Find(string name)
        {
            foreach (var guid in AssetDatabase.FindAssets($"{Prefix}{name} t:Texture2D"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileNameWithoutExtension(path) == $"{Prefix}{name}")
                {
                    return path;
                }
            }

            return null;
        }

        /// <summary>Draws <paramref name="fill"/> (a white shape) tinted, then <paramref name="frame"/> over it.</summary>
        private static Sprite Flatten(string path, string fill, Color tint, string frame, Vector4 border)
        {
            var bottom = Load(fill);
            var top = Load(frame);
            var a = bottom.GetPixels();
            var b = top.GetPixels();
            for (var i = 0; i < a.Length; i++)
            {
                var under = a[i] * tint;
                var over = b[i];
                var alpha = over.a + under.a * (1 - over.a);
                var rgb = alpha <= 0 ? Color.clear : (over * over.a + under * under.a * (1 - over.a)) / alpha;
                a[i] = new Color(rgb.r, rgb.g, rgb.b, alpha);
            }

            bottom.SetPixels(a);
            bottom.Apply();
            File.WriteAllBytes(path, bottom.EncodeToPNG());
            Object.DestroyImmediate(bottom);
            Object.DestroyImmediate(top);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spriteBorder = border;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        /// <summary>Reads the PNG itself, so the pack's import settings (readable or not) don't matter.</summary>
        private static Texture2D Load(string path)
        {
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            texture.LoadImage(File.ReadAllBytes(path));
            return texture;
        }

        private static Color Hex(string hex) => ColorUtility.TryParseHtmlString(hex, out var c) ? c : Color.magenta;
    }
}
