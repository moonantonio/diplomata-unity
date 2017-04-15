﻿using UnityEngine;
using UnityEditor;
using DiplomataLib;

namespace DiplomataEditor {

    public class CharacterListMenu : EditorWindow {

        public Vector2 scrollPos = new Vector2(0, 0);

        [MenuItem("Diplomata/Character List")]
        static public void Init() {
            Diplomata.Instantiate();

            CharacterListMenu window = (CharacterListMenu)GetWindow(typeof(CharacterListMenu), false, "Character List");
            window.minSize = new Vector2(DGUI.WINDOW_MIN_WIDTH + 80, 300);
            window.Show();
        }

        public void OnGUI() {
            scrollPos = DGUI.ScrollWindow(() => {
                
                if (Diplomata.preferences.characterList.Count <= 0) {
                    EditorGUILayout.HelpBox("No characters yet.", MessageType.Info);
                }

                foreach (string name in Diplomata.preferences.characterList) {
                    var half = Screen.width - (2 * DGUI.MARGIN) - 6;

                    if (DGUI.hasSlider) {
                        half -= 15;
                    }

                    half /= 2;

                    var style = GUI.skin.label;

                    DGUI.Horizontal(() => {

                        DGUI.Horizontal(() => {

                            style.alignment = TextAnchor.MiddleLeft;
                            GUILayout.Label(name, style);
                            
                            style.alignment = TextAnchor.MiddleRight;
                            style.fontStyle = FontStyle.Bold;
                            if (Diplomata.preferences.playerCharacterName == name) {
                                GUILayout.Label("[Player]", style);
                            }

                            style.alignment = TextAnchor.MiddleLeft;
                            style.fontStyle = FontStyle.Normal;

                        }, half);

                        DGUI.Horizontal(() => {

                            if (GUILayout.Button("Edit", GUILayout.Height(DGUI.BUTTON_HEIGHT_SMALL))) {
                                CharacterEditor.Edit(Diplomata.FindCharacter(name));
                            }

                            if (GUILayout.Button("Edit Messages", GUILayout.Height(DGUI.BUTTON_HEIGHT_SMALL))) {
                                CharacterMessagesManager.OpenContextMenu(Diplomata.FindCharacter(name));
                            }

                            if (GUILayout.Button("Delete", GUILayout.Height(DGUI.BUTTON_HEIGHT_SMALL))) {
                                if (EditorUtility.DisplayDialog("Are you sure?", "Do you really want to delete?\nThis data will be lost forever.", "Yes", "No")) {
                                    JSONHandler.Delete(name, "Diplomata/Characters/");

                                    Character.UpdateList();
                                    JSONHandler.Update(Diplomata.preferences, "preferences", "Diplomata/");

                                    CharacterInspector.characterList = ArrayHandler.ListToArray(Diplomata.preferences.characterList);

                                    CharacterEditor.Reset(name);
                                    CharacterMessagesManager.Reset(name);
                                    AddContext.Reset(name);
                                }
                            }

                        }, half);

                    });

                    EditorGUILayout.Separator();
                }

                if (GUILayout.Button("Create", GUILayout.Height(DGUI.BUTTON_HEIGHT))) {
                    CharacterEditor.Create();
                }
            }, scrollPos, ((DGUI.BUTTON_HEIGHT_SMALL + 10) * Diplomata.preferences.characterList.Count) + DGUI.BUTTON_HEIGHT + 10 + (3 * DGUI.MARGIN) );

        }

        public void OnInspectorUpdate() {
            Repaint();
        }
    }

}
