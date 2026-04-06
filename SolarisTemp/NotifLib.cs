using System.Collections.Generic;
using UnityEngine;

namespace SolarisTemp
{
    public class NotifLib : MonoBehaviour
    {
        private static NotifLib _instance;
        
        public static NotifLib Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("NotifLib");
                    DontDestroyOnLoad(go);
                    _instance = go.AddComponent<NotifLib>();
                }
                return _instance;
            }
        }
        
        private class Notif
        {
            public string Title;
            public string Message;
            public float Duration;
            public float Timer;
            public float SlideX;
            public float TargetY;
            public float CurrentY;
            public bool SlidingIn;
            public bool SlidingOut;
            public bool YInitialized;
        }

        private readonly List<Notif> _notifs = new List<Notif>();
        
        private const float NotifWidth = 300f;
        private const float NotifHeight = 72f;
        private const float Padding = 8f;
        private const float SlideSpeed = 1400f;
        private const float YSmooth = 12f;
        private const float ProgressH = 4f;
        private const float MarginLeft = 12f;
        private const float MarginBot = 12f;
        
        private static readonly Color BgMain = new Color(0.11f, 0.10f, 0.14f, 0.97f);
        private static readonly Color BgTitle = new Color(0.08f, 0.07f, 0.11f, 1f);
        private static readonly Color AccentLight = new Color(0.72f, 0.52f, 1.00f, 1f);
        private static readonly Color AccentDark = new Color(0.38f, 0.22f, 0.62f, 1f);
        private static readonly Color TextPrimary = new Color(0.92f, 0.90f, 0.96f, 1f);
        private static readonly Color TextMuted = new Color(0.60f, 0.57f, 0.68f, 1f);
        private static readonly Color BarBg = new Color(0.18f, 0.16f, 0.22f, 1f);

        private GUIStyle _styleTitle;
        private GUIStyle _styleMsg;
        private bool _stylesReady;

        private Texture2D MakeTex(Color c)
        {
            var t = new Texture2D(1, 1);
            t.SetPixel(0, 0, c);
            t.Apply();
            return t;
        }

        private void InitStyles()
        {
            if (_stylesReady) return;
            _stylesReady = true;

            _styleTitle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                wordWrap = false,
                clipping = TextClipping.Clip,
            };
            _styleTitle.normal.textColor = TextPrimary;

            _styleMsg = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                fontStyle = FontStyle.Normal,
                wordWrap = true,
                clipping = TextClipping.Clip,
            };
            _styleMsg.normal.textColor = TextMuted;
        }
        
        public static void Send(string title, string message, float duration = 4f)
        {
            Instance._notifs.Add(new Notif
            {
                Title = title,
                Message = message,
                Duration = duration,
                Timer = 0f,
                SlideX = NotifWidth,
                SlidingIn = true,
                SlidingOut = false,
                YInitialized = false,
            });
        }
        
        private void Update()
        {
            float dt = Time.deltaTime;
            float screenH = Screen.height;
            
            for (int i = 0; i < _notifs.Count; i++)
            {
                float targetY = screenH - MarginBot - NotifHeight - i * (NotifHeight + Padding);
                _notifs[i].TargetY = targetY;
            }
            
            for (int i = _notifs.Count - 1; i >= 0; i--)
            {
                var n = _notifs[i];
                
                if (!n.YInitialized)
                {
                    n.CurrentY = n.TargetY;
                    n.YInitialized = true;
                }

                n.CurrentY += (n.TargetY - n.CurrentY) * Mathf.Clamp01(YSmooth * dt);

                if (n.SlidingIn)
                {
                    n.SlideX -= SlideSpeed * dt;
                    if (n.SlideX <= 0f)
                    {
                        n.SlideX = 0f;
                        n.SlidingIn = false;
                    }
                }
                else if (n.SlidingOut)
                {
                    n.SlideX += SlideSpeed * dt;
                    if (n.SlideX >= NotifWidth)
                        _notifs.RemoveAt(i);
                }
                else
                {
                    n.Timer += dt;
                    if (n.Timer >= n.Duration)
                        n.SlidingOut = true;
                }
            }
        }
        
        private void OnGUI()
        {
            if (_notifs.Count == 0) return;
            InitStyles();

            foreach (var n in _notifs)
            {
                float x = MarginLeft - n.SlideX;
                DrawNotif(n, x, n.CurrentY);
            }
        }

        private void DrawNotif(Notif n, float x, float y)
        {
            GUI.DrawTexture(new Rect(x, y, NotifWidth, NotifHeight), MakeTex(BgMain));
            
            float titleBarH = 26f;
            GUI.DrawTexture(new Rect(x + 3f, y, NotifWidth - 3f, titleBarH), MakeTex(BgTitle));

            GUI.Label(new Rect(x + 12f, y + 4f, NotifWidth - 20f, titleBarH - 4f), n.Title, _styleTitle);

            float msgY = y + titleBarH + 4f;
            float msgH = NotifHeight - titleBarH - ProgressH - 8f;
            GUI.Label(new Rect(x + 12f, msgY, NotifWidth - 20f, msgH), n.Message, _styleMsg);

            float barY = y + NotifHeight - ProgressH;
            GUI.DrawTexture(new Rect(x + 3f, barY, NotifWidth - 3f, ProgressH), MakeTex(BarBg));

            if (!n.SlidingOut)
            {
                float fill = 1f - Mathf.Clamp01(n.Timer / n.Duration);
                GUI.DrawTexture(new Rect(x + 3f, barY, (NotifWidth - 3f) * fill, ProgressH), MakeTex(AccentDark));
            }
            
            GUI.DrawTexture(new Rect(x, y, 3f, NotifHeight), MakeTex(AccentLight));
        }
    }
}