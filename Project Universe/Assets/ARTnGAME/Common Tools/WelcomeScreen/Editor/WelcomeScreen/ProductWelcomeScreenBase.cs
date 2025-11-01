using System;
using System.Collections.Generic;
using System.Linq;
using Artngame.CommonTools.WelcomeScreen.GuiElements;
using UnityEditor;
using UnityEngine;

namespace Artngame.CommonTools.WelcomeScreen
{
    public abstract class ProductWelcomeScreenBase : EditorWindow
    {
        public abstract string WindowTitle { get; }
        public abstract Vector2 WindowSizePx { get; }

        private string _showAtStartupPreferenceKey;
        protected string ShowAtStartupPreferenceKey => _showAtStartupPreferenceKey ?? (_showAtStartupPreferenceKey = ProductPreferenceBase.CreateDefaultShowOptionPreferenceDefinition().PreferenceKey);

        public GUIStyle LabelStyle { get; private set; }
        public GUIStyle BoldTextStyle { get; private set; }
        public GUIStyle ButtonStyle { get; private set; }
        public GUIStyle LinkStyle { get; private set; }
        public GUIStyle LabelSubTextStyle { get; private set; }
        public GUIStyle TextStyle { get; private set; }

        private ProductPreferenceBase.StartupShowMode _startupStartupShowMode;
        private GUIContent _productIcon;
        public GUIContent _productIconGIProxy;
        public GUIContent _productIconSkyMaster;
        public GUIContent _productIconInfiniGRASS;
        public GUIContent _productIconORION;
        public GUIContent _productIconETHEREAL;

        public GUIContent _productIconInfiniTREE;
        public GUIContent _productIconInfiniRIVER;
        public GUIContent _productIconInfniCLOUD;
        public GUIContent _productIconOceanis;
        public GUIContent _productIconIvyStudio;
        public GUIContent _productIconParticleDynamicMagic;

        public GUIContent _productIconGIBLI;
        public GUIContent _productIconPANGAEA;
        public GUIContent _productIconLUMINA;
        public GUIContent _productIconLIQUA;

        public GUIContent _productIconInfiniSPLAT;
        public GUIContent _productIconEnviro;
        public GUIContent _productIconTem;
        public GUIContent _productIconInfiniSNOW;
        public GUIContent _productIconInfiniBATCH;
        public GUIContent _productIconInfiniTUNES;
        public GUIContent _productIconGLAMOR;

        //v0.1
        public Texture2D banners;

        private Action<ProductWelcomeScreenBase> _renderMainScrollView;
        private Vector2 _scrollPosition = Vector2.zero;
        public string LastUpdateText { get; private set; }
        public bool IsUpdateTextViewAlreadyPreselected { get; private set; }

        private bool _isInitialized = false;

        public void OpenWindow()
        {
            OpenWindow(GetType(), WindowTitle, WindowSizePx);
        }

        protected static void OpenWindow<TWindowType>(string title, Vector2 windowSizePx) where TWindowType : ProductWelcomeScreenBase
        {
            OpenWindow(typeof(TWindowType), title, windowSizePx);
        }

        public static void OpenWindow(Type windowType, string title, Vector2 windowSizePx)
        {
            var window = GetWindow(windowType, true, title);
            window.minSize = windowSizePx;
            window.maxSize = windowSizePx;
            window.Show();
        }

        protected void OnEnableCommon(string productIconName)
        {
            if (!string.IsNullOrEmpty(productIconName) && _productIcon == null)
            {
                var iconGuid = AssetDatabase.FindAssets("ProductIcon64").FirstOrDefault();
                if (!string.IsNullOrEmpty(iconGuid))
                {
                    _productIcon = new GUIContent(AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(iconGuid)));
                }
            }
        }

        private void EnsureInitialized()
        {
            if (_isInitialized) return;

            EnsureStylesInitialized();
            _startupStartupShowMode = (ProductPreferenceBase.StartupShowMode)EditorPrefs.GetInt(ShowAtStartupPreferenceKey, (int)ProductPreferenceBase.StartupShowMode.Always);
            LastUpdateText = ProductPreferenceBase.CreateDefaultLastUpdateText().GetEditorPersistedValueOrDefault() as string;

            _isInitialized = true;
        }

        protected void RenderGUI(List<GuiSection> leftSections, GuiSection topSection, GuiSection bottomSection, ScrollViewGuiSection mainScrollViewGuiSection)
        {
            EnsureInitialized();

            if (_renderMainScrollView == null)
            {
                _renderMainScrollView = mainScrollViewGuiSection.RenderMainScrollViewSection;
            }

            EditorGUILayout.BeginHorizontal(GUIStyle.none, GUILayout.ExpandWidth(true));
            {
                if (leftSections != null && leftSections.Any())
                {
                    EditorGUILayout.BeginVertical(GUILayout.Width(leftSections.First().WidthPx));
                    foreach (var leftSection in leftSections)
                    {
                        {
                            RenderSectionStart(leftSection);

                            foreach (var element in leftSection.ClickableElements)
                            {
                                var lastUpdateButton = element as LastUpdateButton;
                                if (lastUpdateButton != null)
                                {
                                    if (!string.IsNullOrWhiteSpace(LastUpdateText))
                                    {
                                        if (!IsUpdateTextViewAlreadyPreselected)
                                        {
                                            ChangeMainScrollViewRenderFn(lastUpdateButton.RenderMainScrollViewFunction);
                                            IsUpdateTextViewAlreadyPreselected = true; 
                                        }
                                        RenderClickableElement(element);
                                    }
                                }
                                else
                                {
                                    RenderClickableElement(element);
                                }
                            }

                            GUILayout.Space(10);
                        }
                    }

                    EditorGUILayout.EndVertical();
                }


                EditorGUILayout.BeginVertical(GUILayout.Width(topSection.WidthPx), GUILayout.ExpandHeight(true));
                {
                    if (topSection != null)
                    {
                        RenderSectionStart(topSection);

                        if (topSection.ClickableElements.Count > 0)
                        {
                            EditorGUILayout.BeginHorizontal(GUILayout.Width(topSection.WidthPx));
                            {
                                foreach (var element in topSection.ClickableElements)
                                {
                                    RenderClickableElement(element);
                                }
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                    }

                    
                    if (mainScrollViewGuiSection != null)
                    {
                        RenderSectionStart(mainScrollViewGuiSection);
                        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, "ProgressBarBack", GUILayout.ExpandHeight(true), GUILayout.Width(mainScrollViewGuiSection.WidthPx));

                        _renderMainScrollView(this);

                        GUILayout.EndScrollView();
                    }


                    if (bottomSection != null)
                    {
                        EditorGUILayout.BeginHorizontal(GUILayout.Width(bottomSection.WidthPx));
                        {
                            EditorGUILayout.BeginVertical();
                            {
                                RenderSectionStart(bottomSection);

                                EditorGUILayout.BeginHorizontal();
                                {
                                    for (var i = 0; i < bottomSection.ClickableElements.Count; i++)
                                    {
                                        var bottomLink = bottomSection.ClickableElements[i];
                                        if (GUILayout.Button(bottomLink.Text, LinkStyle))
                                            bottomLink.OnClick(this);

                                        if (i < bottomSection.ClickableElements.Count - 1)
                                        {
                                            GUILayout.Label("-");
                                        }
                                    }
                                }
                                EditorGUILayout.EndHorizontal();
                                GUILayout.Space(7);
                            }
                            EditorGUILayout.EndVertical();

                            GUILayout.FlexibleSpace();
                            EditorGUILayout.BeginVertical();
                            GUILayout.Space(7);
                            if (_productIcon != null) GUILayout.Label(_productIcon);
                            EditorGUILayout.EndVertical();
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.BeginHorizontal("ProjectBrowserBottomBarBg", GUILayout.ExpandWidth(true), GUILayout.Height(22));
            {
                GUILayout.FlexibleSpace();
                EditorGUI.BeginChangeCheck();
                var cache = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 100;
                _startupStartupShowMode = (ProductPreferenceBase.StartupShowMode)EditorGUILayout.EnumPopup("Show At Startup", _startupStartupShowMode, GUILayout.Width(220));
                EditorGUIUtility.labelWidth = cache;
                if (EditorGUI.EndChangeCheck())
                {
                    EditorPrefs.SetInt(ShowAtStartupPreferenceKey, (int)_startupStartupShowMode);
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void RenderSectionStart(GuiSectionBase bottomSection)
        {
            if (!string.IsNullOrEmpty(bottomSection.LabelText)) GUILayout.Label(bottomSection.LabelText, LabelStyle);
            if (!string.IsNullOrEmpty(bottomSection.LabelSubText))
            {
                GUILayout.Label(bottomSection.LabelSubText, LabelSubTextStyle);
            }
        }

        private void RenderClickableElement(ClickableElement clickableElement)
        {
            GUIContent buttonGuiContent;
            if (!string.IsNullOrEmpty(clickableElement.IconName))
            {
                buttonGuiContent = new GUIContent($" {clickableElement.Text}", EditorGUIUtility.IconContent(clickableElement.IconName).image);
            }
            else
            {
                buttonGuiContent = new GUIContent(clickableElement.Text);
            }

            if (GUILayout.Button(buttonGuiContent, GUILayout.ExpandWidth(true)))
            {
                clickableElement.OnClick(this);
            }
        }

        public void ChangeMainScrollViewRenderFn(Action<ProductWelcomeScreenBase> renderFn)
        {
            _renderMainScrollView = renderFn;
        }

        private void EnsureStylesInitialized()
        {
            if (LabelStyle == null)
            {
                LabelStyle = new GUIStyle("BoldLabel")
                {
                    margin = new RectOffset(4, 4, 4, 4),
                    padding = new RectOffset(2, 2, 2, 2),
                    fontSize = 13
                };
            }

            if (BoldTextStyle == null)
            {
                BoldTextStyle = new GUIStyle("BoldLabel")
                {
                    margin = new RectOffset(0, 0, 0, 0),
                    padding = new RectOffset(2, 2, 2, 2),
                    fontSize = 13
                };
            }

            if (ButtonStyle == null)
            {
                ButtonStyle = new GUIStyle(GUI.skin.button)
                {
                    alignment = TextAnchor.MiddleLeft
                };
            }

            if (LinkStyle == null)
            {
                LinkStyle = new GUIStyle
                {
                    normal = { textColor = new Color(0.2980392f, 0.4901961f, 1f) },
                    hover = { textColor = Color.white },
                    active = { textColor = Color.grey },
                    margin = { top = 3, bottom = 2 }
                };
            }

            if (LabelSubTextStyle == null)
            {
                LabelSubTextStyle = new GUIStyle("WordWrappedMiniLabel") { padding = { top = -5 } };
            }

            if (TextStyle == null)
            {
                TextStyle = new GUIStyle("WordWrappedMiniLabel")
                {
                    padding = { bottom = 5 }
                };
            }
        }

        protected static void GenerateCommonWelcomeText(string productName, ProductWelcomeScreenBase screen, string customText = null)
        {
            GUILayout.Label(
                @"Thanks for using ARTnGAME assets!

If at any stage you got some questions or need help please let me know via the Discord channel or Email.

You can always get back to this screen via: Window -> ARTnGAME -> Welcome Screen 
" 
+ (string.IsNullOrWhiteSpace(customText) ? "" : $"\r\n\r\n{customText}") //+ @"

//Hope the asset will meet your expectations."
                    .Replace("<ProductName>", productName),
                screen.TextStyle, GUILayout.ExpandHeight(true));
        }
    }
}