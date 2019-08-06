﻿//-----------------------------------------------------------------------
// <copyright file="ValidationProfileEditorDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinValidator.Editor
{
    using Sirenix.OdinInspector.Editor;
    using Sirenix.OdinInspector.Editor.Validation;
    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    public class ValidationProfileEditorDrawer : OdinValueDrawer<ValidationProfileEditor>
    {
        private LocalPersistentContext<float> menuTreeWidth;
        private List<ResizableColumn> columns;
        private Vector2 scrollPos;
        private ValidationRunner runner;
        private ValidationProfileEditor editor;
        private IValidationProfile profile;
        private InspectorProperty sourceProperty;
        private ValidationProfileTree validationProfileTree;
        private ValidationOverview overview;
        private LocalPersistentContext<bool> overviewToggle;
        private float overviewHeight = 300;

        protected override void Initialize()
        {
            this.menuTreeWidth = this.GetPersistentValue<float>("menuTreeWidth", 380);
            this.columns = new List<ResizableColumn>() { ResizableColumn.FlexibleColumn(this.menuTreeWidth.Value, 80), ResizableColumn.DynamicColumn(0, 200) };
            this.runner = new ValidationRunner();
            this.overview = new ValidationOverview();
            this.editor = this.ValueEntry.SmartValue;
            this.profile = this.editor.Profile;
            this.sourceProperty = this.Property.Children["selectedSourceTarget"];
            this.validationProfileTree = new ValidationProfileTree();
            this.overviewToggle = this.GetPersistentValue<bool>("overviewToggle", true);

            this.validationProfileTree.Selection.SelectionChanged += (x) =>
            {
                if (x == SelectionChangedType.ItemAdded)
                {
                    object value = this.validationProfileTree.Selection.SelectedValue;
                    ValidationProfileResult result = value as ValidationProfileResult;
                    if (result != null)
                    {
                        this.editor.SetTarget(result.GetSource());

                    }
                    else
                    {
                        this.editor.SetTarget(value);
                    }
                }
            };

            this.overview.OnProfileResultSelected += result =>
            {
                OdinMenuItem mi = this.validationProfileTree.GetMenuItemForObject(result);
                mi.Select();
                object source = result.GetSource();
                this.editor.SetTarget(source);
            };

            this.validationProfileTree.AddProfileRecursive(this.ValueEntry.SmartValue.Profile);

            OdinMenuTree.ActiveMenuTree = this.validationProfileTree;

            if (this.editor.ScanProfileImmediatelyWhenOpening)
            {
                this.editor.ScanProfileImmediatelyWhenOpening = false;
                this.ScanProfile(this.editor.Profile);
            }
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            // Save menuTreeWidth.
            this.menuTreeWidth.Value = this.columns[0].ColWidth;

            // Bottom Slide Toggle Bits:
            Rect overviewSlideRect = new Rect();
            Rect toggleOverviewBtnRect = new Rect();

            Rect topRect;
            GUILayout.BeginHorizontal(GUILayoutOptions.ExpandHeight());
            {
                topRect = GUIHelper.GetCurrentLayoutRect();
                GUITableUtilities.ResizeColumns(topRect, this.columns);

                // Bottom Slide Toggle Bits:
                // The bottom slide-rect toggle needs to be drawn above, but is placed below.
                overviewSlideRect = topRect.AlignBottom(4).AddY(4);
                overviewSlideRect.width += 4;
                toggleOverviewBtnRect = overviewSlideRect.AlignCenter(100).AlignBottom(14);
                EditorGUIUtility.AddCursorRect(toggleOverviewBtnRect, MouseCursor.Arrow);
                if (SirenixEditorGUI.IconButton(toggleOverviewBtnRect.AddY(-2), this.overviewToggle.Value ? EditorIcons.TriangleDown : EditorIcons.TriangleUp))
                {
                    this.overviewToggle.Value = !this.overviewToggle.Value;
                }

                if (this.overviewToggle.Value)
                {
                    this.overviewHeight -= SirenixEditorGUI.SlideRect(overviewSlideRect.SetXMax(toggleOverviewBtnRect.xMin), MouseCursor.SplitResizeUpDown).y;
                    this.overviewHeight -= SirenixEditorGUI.SlideRect(overviewSlideRect.SetXMin(toggleOverviewBtnRect.xMax), MouseCursor.SplitResizeUpDown).y;
                }

                // Left menu tree
                GUILayout.BeginVertical(GUILayoutOptions.Width(this.columns[0].ColWidth).ExpandHeight());
                {
                    EditorGUI.DrawRect(GUIHelper.GetCurrentLayoutRect(), SirenixGUIStyles.EditorWindowBackgroundColor);
                    this.validationProfileTree.Draw();
                }
                GUILayout.EndVertical();

                // Draw selected
                GUILayout.BeginVertical();
                {
                    this.DrawTopBarButtons();
                    this.DrawSelectedTests();
                }
                GUILayout.EndVertical();
                GUITableUtilities.DrawColumnHeaderSeperators(topRect, this.columns, SirenixGUIStyles.BorderColor);
            }
            GUILayout.EndHorizontal();

            // Bottom Slide Toggle Bits:
            if (this.overviewToggle.Value)
            {
                GUILayoutUtility.GetRect(0, 4); // Slide Area.
            }

            EditorGUI.DrawRect(overviewSlideRect, SirenixGUIStyles.BorderColor);
            EditorGUI.DrawRect(toggleOverviewBtnRect.AddY(-overviewSlideRect.height), SirenixGUIStyles.BorderColor);
            SirenixEditorGUI.IconButton(toggleOverviewBtnRect.AddY(-2), this.overviewToggle.Value ? EditorIcons.TriangleDown : EditorIcons.TriangleUp);

            // Overview
            if (this.overviewToggle.Value)
            {
                GUILayout.BeginVertical(GUILayout.Height(this.overviewHeight));
                {
                    this.overview.DrawOverview();
                }
                GUILayout.EndVertical();

                if (Event.current.type == EventType.Repaint)
                {
                    this.overviewHeight = Mathf.Max(50, this.overviewHeight);
                    float height = GUIHelper.CurrentWindow.position.height - overviewSlideRect.yMax;
                    this.overviewHeight = Mathf.Min(this.overviewHeight, height);
                }
            }
        }

        public void ScanProfile(IValidationProfile profile)
        {
            EditorApplication.delayCall += () =>
            {
                List<ValidationProfileResult> results = new List<ValidationProfileResult>();
                this.validationProfileTree.CleanProfile(profile);

                try
                {
                    foreach (ValidationProfileResult result in profile.Validate(this.runner))
                    {
                        this.validationProfileTree.AddResultToTree(result);
                        results.Add(result);

                        if (GUIHelper.DisplaySmartUpdatingCancellableProgressBar(result.Profile.Name, result.Name, result.Progress))
                        {
                            break;
                        }
                    }
                }
                finally
                {
                    EditorUtility.ClearProgressBar();
                    this.overview.ProfileResults = results;
                    this.overview.Update();
                    this.validationProfileTree.MarkDirty();
                    this.validationProfileTree.UpdateMenuTree();
                    this.validationProfileTree.AddErrorAndWarningIcons();
                }
            };
        }

        private void DrawSelectedTests()
        {
            this.scrollPos = GUILayout.BeginScrollView(this.scrollPos, GUIStyle.none);
            GUILayout.BeginVertical(SirenixGUIStyles.ContentPadding);
            GUIHelper.PushLabelWidth(this.columns.Last().ColWidth * 0.33f);
            this.sourceProperty.Draw(null);
            GUIHelper.PopLabelWidth();
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }

        private void DrawTopBarButtons()
        {
            string btnName = "Run " + this.profile.Name;
            float width = GUI.skin.button.CalcSize(GUIHelper.TempContent(btnName)).x + 10;
            Rect rect = GUIHelper.GetCurrentLayoutRect().AlignRight(width);
            rect.x -= 5;
            rect.y -= 26;
            rect.height = 18;

            GUIHelper.PushColor(Color.green);
            if (GUI.Button(rect, btnName))
            {
                this.ScanProfile(this.profile);
            }
            GUIHelper.PopColor();

            object selectedValue = this.validationProfileTree.Selection.SelectedValue;

            if (selectedValue is ValidationProfileResult)
            {
                ValidationProfileResult result = selectedValue as ValidationProfileResult;
                if (result != null)
                {
                    // Draw top bar buttons
                    Object source = result.GetSource() as UnityEngine.Object;
                    if (source != null)
                    {
                        rect.x -= 100;
                        rect.width = 90;
                        if (GUI.Button(rect, "Select Object", SirenixGUIStyles.ButtonRight))
                        {
                            GUIHelper.SelectObject(source);
                            GUIHelper.PingObject(source);
                        }
                        rect.x -= 80;
                        rect.width = 80;
                        if (GUI.Button(rect, "Ping Object", SirenixGUIStyles.ButtonLeft))
                        {
                            GUIHelper.PingObject(source);
                        }
                    }
                }
            }
        }
    }
}
