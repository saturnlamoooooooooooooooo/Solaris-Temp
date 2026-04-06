using BepInEx;
using GorillaNetworking;
using Photon.Pun;
using Photon.Realtime;
using System;
using UnityEngine;

namespace SolarisTemp
{
    [BepInPlugin("Solaris", "Temp", "1.0.0")]
    public class Class1 : BaseUnityPlugin
    {
        public static string MenuTitle = "Solaris | GUI Temp";

        private Rect windowRect = new Rect(20, 20, 480, 580);
        public string Room = "";
        public string PlayerName = "";
        public Vector2 scrollPos;
        private int selectedTab = 0;
        private Photon.Realtime.Player selectedPlayer = null;
        
        private float tabAlpha = 1f;
        private int pendingTab = -1;
        private const float FadeSpeed = 4f;

        private GUIStyle windowStyle;
        private GUIStyle titleStyle;
        private GUIStyle tabActiveStyle;
        private GUIStyle tabInactiveStyle;
        private GUIStyle buttonStyle;
        private GUIStyle textFieldStyle;
        private GUIStyle playerButtonStyle;
        private GUIStyle playerSelectedStyle;
        private GUIStyle sectionLabelStyle;
        private GUIStyle verticalScrollBarStyle;
        private GUIStyle verticalScrollBarThumbStyle;
        private GUIStyle horizontalScrollBarStyle;
        private GUIStyle horizontalScrollBarThumbStyle;
        private bool stylesInitialized = false;
        
        private static readonly Color bgDark = new Color(0.10f, 0.10f, 0.11f, 0.98f);
        private static readonly Color bgPanel = new Color(0.13f, 0.12f, 0.15f, 1f);
        private static readonly Color bgButton = new Color(0.17f, 0.15f, 0.21f, 1f);
        private static readonly Color bgButtonHov = new Color(0.22f, 0.18f, 0.30f, 1f);
        private static readonly Color bgButtonAct = new Color(0.28f, 0.20f, 0.40f, 1f);
        private static readonly Color bgTitleBar = new Color(0.08f, 0.07f, 0.10f, 1f);
        private static readonly Color accentLight = new Color(0.72f, 0.52f, 1.00f, 1f);
        private static readonly Color accentDark = new Color(0.38f, 0.22f, 0.62f, 1f);
        private static readonly Color textPrimary = new Color(0.92f, 0.90f, 0.96f, 1f);
        private static readonly Color textMuted = new Color(0.46f, 0.43f, 0.54f, 1f);
        private static readonly Color dangerColor = new Color(0.85f, 0.28f, 0.28f, 1f);
        
        private void Update()
        {
            if (pendingTab != -1)
            {
                tabAlpha -= Time.deltaTime * FadeSpeed;
                if (tabAlpha <= 0f)
                {
                    tabAlpha    = 0f;
                    selectedTab = pendingTab;
                    pendingTab  = -1;
                    scrollPos   = Vector2.zero;
                }
            }
            else if (tabAlpha < 1f)
            {
                tabAlpha += Time.deltaTime * FadeSpeed;
                if (tabAlpha > 1f) tabAlpha = 1f;
            }
        }

        private Texture2D MakeTex(Color c)
        {
            var t = new Texture2D(1, 1);
            t.SetPixel(0, 0, c);
            t.Apply();
            return t;
        }

        private void InitStyles()
        {
            if (stylesInitialized) return;
            stylesInitialized = true;
            
            windowStyle = new GUIStyle(GUI.skin.window)
            {
                padding = new RectOffset(0, 0, 0, 0),
                border = new RectOffset(0, 0, 0, 0),
            };
            
            windowStyle.normal.background = MakeTex(bgDark);
            windowStyle.onNormal.background = MakeTex(bgDark);
            
            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
            };
            
            titleStyle.normal.textColor = textPrimary;

            tabActiveStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                fixedHeight = 34,
                border = new RectOffset(0, 0, 0, 0),
            };
            
            tabActiveStyle.normal.background = MakeTex(accentDark);
            tabActiveStyle.hover.background = MakeTex(accentLight);
            tabActiveStyle.active.background = MakeTex(accentLight);
            tabActiveStyle.normal.textColor = Color.white;
            tabActiveStyle.hover.textColor = Color.white;
            tabActiveStyle.active.textColor = Color.white;

            tabInactiveStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
                fontStyle = FontStyle.Normal,
                fixedHeight = 34,
                border = new RectOffset(0, 0, 0, 0),
            };
            
            tabInactiveStyle.normal.background = MakeTex(bgPanel);
            tabInactiveStyle.hover.background = MakeTex(bgButton);
            tabInactiveStyle.active.background = MakeTex(accentDark);
            tabInactiveStyle.normal.textColor = textMuted;
            tabInactiveStyle.hover.textColor = textPrimary;
            tabInactiveStyle.active.textColor = Color.white;

            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
                fontStyle = FontStyle.Normal,
                fixedHeight = 36,
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(14, 14, 0, 0),
                border = new RectOffset(0, 0, 0, 0),
            };
            
            buttonStyle.normal.background = MakeTex(bgButton);
            buttonStyle.hover.background = MakeTex(bgButtonHov);
            buttonStyle.active.background = MakeTex(bgButtonAct);
            buttonStyle.normal.textColor = textPrimary;
            buttonStyle.hover.textColor = Color.white;
            buttonStyle.active.textColor = Color.white;
            
            textFieldStyle = new GUIStyle(GUI.skin.textField)
            {
                fontSize = 12,
                fixedHeight = 34,
                padding = new RectOffset(12, 12, 0, 0),
                border = new RectOffset(0, 0, 0, 0),
                alignment = TextAnchor.MiddleLeft,
            };
            
            textFieldStyle.normal.background = MakeTex(bgPanel);
            textFieldStyle.hover.background = MakeTex(bgButton);
            textFieldStyle.active.background = MakeTex(bgButtonAct);
            textFieldStyle.focused.background = MakeTex(bgButton);
            textFieldStyle.normal.textColor = textPrimary;
            textFieldStyle.hover.textColor = textPrimary;
            textFieldStyle.active.textColor = Color.white;
            textFieldStyle.focused.textColor = Color.white;
            
            playerButtonStyle = new GUIStyle(buttonStyle)
            {
                alignment = TextAnchor.MiddleLeft,
            };
            
            playerSelectedStyle = new GUIStyle(buttonStyle)
            {
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
            };
            
            playerSelectedStyle.normal.background = MakeTex(accentDark);
            playerSelectedStyle.hover.background = MakeTex(accentLight);
            playerSelectedStyle.active.background = MakeTex(accentLight);
            playerSelectedStyle.normal.textColor = Color.white;
            playerSelectedStyle.hover.textColor = Color.white;
            playerSelectedStyle.active.textColor = Color.white;
            
            sectionLabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 10,
                fontStyle = FontStyle.Bold,
            };
            sectionLabelStyle.normal.textColor = textMuted;

            verticalScrollBarStyle = new GUIStyle(GUI.skin.verticalScrollbar)
            {
                fixedWidth = 8,
                margin = new RectOffset(0, 0, 0, 0),
                padding = new RectOffset(0, 0, 0, 0),
            };
            verticalScrollBarStyle.normal.background = MakeTex(bgPanel);

            verticalScrollBarThumbStyle = new GUIStyle(GUI.skin.verticalScrollbarThumb)
            {
                fixedWidth = 8,
            };
            verticalScrollBarThumbStyle.normal.background = MakeTex(accentDark);
            verticalScrollBarThumbStyle.hover.background = MakeTex(accentLight);
            verticalScrollBarThumbStyle.active.background = MakeTex(accentLight);

            horizontalScrollBarStyle = new GUIStyle(GUI.skin.horizontalScrollbar)
            {
                fixedHeight = 8,
                margin = new RectOffset(0, 0, 0, 0),
                padding = new RectOffset(0, 0, 0, 0),
            };
            horizontalScrollBarStyle.normal.background = MakeTex(bgPanel);

            horizontalScrollBarThumbStyle = new GUIStyle(GUI.skin.horizontalScrollbarThumb)
            {
                fixedHeight = 8,
            };
            horizontalScrollBarThumbStyle.normal.background = MakeTex(accentDark);
            horizontalScrollBarThumbStyle.hover.background = MakeTex(accentLight);
            horizontalScrollBarThumbStyle.active.background = MakeTex(accentLight);

            GUI.skin.settings.selectionColor = accentLight;
        }
        
        private void DrawButton(string label, Action onClick, bool isToggled = false, Color? overrideBg = null, Color? overrideFg = null)
        {
            var s = new GUIStyle(buttonStyle);
            if (isToggled)
            {
                s.normal.background = MakeTex(accentDark);
                s.hover.background = MakeTex(accentLight);
                s.active.background = MakeTex(accentLight);
                s.normal.textColor = Color.white;
                s.hover.textColor = Color.white;
                s.active.textColor = Color.white;
            }
            if (overrideBg.HasValue)
            {
                Color b = overrideBg.Value;
                s.normal.background = MakeTex(b);
                s.hover.background = MakeTex(new Color(b.r + 0.08f, b.g + 0.04f, b.b + 0.08f, 1f));
                s.active.background = MakeTex(new Color(b.r - 0.05f, b.g - 0.03f, b.b - 0.05f, 1f));
            }
            if (overrideFg.HasValue)
            {
                s.normal.textColor = overrideFg.Value;
                s.hover.textColor = overrideFg.Value;
                s.active.textColor = overrideFg.Value;
            }
            if (GUILayout.Button(label, s))
                onClick?.Invoke();
            GUILayout.Space(4);
        }

        private void SectionLabel(string text)
        {
            GUILayout.Space(10);
            GUILayout.Label(text.ToUpper(), sectionLabelStyle);
            GUILayout.Space(4);
        }
        
        private void OnGUI()
        {
            InitStyles();
            GUI.color = Color.white;
            
            GUI.skin.verticalScrollbar = verticalScrollBarStyle;
            GUI.skin.verticalScrollbarThumb = verticalScrollBarThumbStyle;
            GUI.skin.horizontalScrollbar = horizontalScrollBarStyle;
            GUI.skin.horizontalScrollbarThumb = horizontalScrollBarThumbStyle;

            windowRect = GUI.Window(0, windowRect, MenuWindow, GUIContent.none, windowStyle);
        }

        private void MenuWindow(int windowID)
        {
            GUI.DrawTexture(new Rect(0, 0, windowRect.width, 40), MakeTex(bgTitleBar));
            GUI.Label(new Rect(0, 0, windowRect.width, 40), MenuTitle, titleStyle);

            var vStyle = new GUIStyle(GUI.skin.label) { fontSize = 10, alignment = TextAnchor.MiddleCenter };
            vStyle.normal.textColor = accentLight;
            GUI.Label(new Rect(windowRect.width - 80, 0, 76, 40), "v1.0.0", vStyle);

            GUILayout.Space(44);
            
            GUILayout.BeginHorizontal();
            string[] tabs = { "Main", "Players" };
            for (int i = 0; i < tabs.Length; i++)
            {
                var style = (i == selectedTab) ? tabActiveStyle : tabInactiveStyle;
                if (GUILayout.Button(tabs[i], style))
                {
                    if (i != selectedTab && pendingTab == -1)
                        pendingTab = i;
                }
            }
            GUILayout.EndHorizontal();
            
            GUI.DrawTexture(GUILayoutUtility.GetRect(windowRect.width, 2), MakeTex(accentLight));

            GUILayout.Space(8);
            
            var prevColor = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, tabAlpha);

            GUILayout.BeginVertical(GUILayout.ExpandHeight(true));
            GUILayout.BeginHorizontal();
            GUILayout.Space(12);
            GUILayout.BeginVertical();

            switch (selectedTab)
            {
                case 0: DrawMainTab(); break;
                case 1: DrawPlayersTab(); break;
            }

            GUILayout.EndVertical();
            GUILayout.Space(12);
            GUILayout.EndHorizontal();
            GUILayout.Space(12);
            GUILayout.EndVertical();

            GUI.color = prevColor;

            GUI.DragWindow(new Rect(0, 0, windowRect.width, 40));
        }
        
        private void DrawMainTab()
        {
            scrollPos = GUILayout.BeginScrollView(scrollPos, false, false);

            SectionLabel("Room");
            Room = GUILayout.TextField(Room, textFieldStyle);
            GUILayout.Space(6);

            DrawButton("Join Room", () => PhotonNetworkController.instance.AttemptToJoinSpecificRoom(Room), false, accentDark, Color.white);

            DrawButton("Join Random Room", () => PhotonNetwork.JoinRandomRoom());

            DrawButton("Leave Room", () => PhotonNetwork.Disconnect(), false, dangerColor, Color.white);

            SectionLabel("Local Player");
            PlayerName = GUILayout.TextField(PlayerName, textFieldStyle);
            GUILayout.Space(6);
            
            DrawButton("Change Name", () =>
            {
                if (!string.IsNullOrEmpty(PlayerName))
                    PhotonNetwork.LocalPlayer.NickName = PlayerName;
            });
            
            SectionLabel("Info");

            var infoStyle = new GUIStyle(GUI.skin.label) { fontSize = 11 };
            infoStyle.normal.textColor = textMuted;

            string roomName = PhotonNetwork.InRoom ? PhotonNetwork.CurrentRoom.Name : "Not in room";
            string playerName = PhotonNetwork.LocalPlayer?.NickName ?? "Unknown";
            int ping = PhotonNetwork.GetPing();

            GUILayout.Label($"Room: {roomName}", infoStyle);
            GUILayout.Label($"Name: {playerName}", infoStyle);
            GUILayout.Label($"Ping: {ping} ms", infoStyle);
            GUILayout.Label($"Players: {PhotonNetwork.PlayerList?.Length ?? 0}", infoStyle);

            GUILayout.EndScrollView();
        }
        
        private void DrawPlayersTab()
        {
            scrollPos = GUILayout.BeginScrollView(scrollPos, false, false);

            SectionLabel("Players");

            if (PhotonNetwork.PlayerList == null || PhotonNetwork.PlayerList.Length == 0)
            {
                var emptyStyle = new GUIStyle(GUI.skin.label) { fontSize = 11, alignment = TextAnchor.MiddleCenter };
                emptyStyle.normal.textColor = textMuted;
                GUILayout.Space(20);
                GUILayout.Label("Not in a room.", emptyStyle);
            }
            else
            {
                foreach (var player in PhotonNetwork.PlayerList)
                {
                    string nick = string.IsNullOrEmpty(player.NickName) ? "Unknown" : player.NickName;
                    bool isLocal = player.IsLocal;
                    bool isSelected = selectedPlayer != null && selectedPlayer.ActorNumber == player.ActorNumber;
                    
                    string lbl = $"{nick}{(isLocal ? " [You]" : "")}";

                    var style = isSelected ? playerSelectedStyle : playerButtonStyle;
                    if (GUILayout.Button(lbl, style))
                        selectedPlayer = isSelected ? null : player;

                    GUILayout.Space(3);
                }
            }

            if (selectedPlayer != null && !selectedPlayer.IsLocal)
            {
                SectionLabel($"Actions — {selectedPlayer.NickName}");
                DrawButton("Teleport To Player", () =>
                {
                    
                });
            }

            GUILayout.EndScrollView();
        }
    }
}