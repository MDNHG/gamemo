using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace MoAnToan.Editor
{
    public static class MATAssetGenerator
    {
        const string BaseDir = "Assets/MoAnToan/Resources/Sprites";
        const string UIDir = BaseDir + "/UI";
        const string IconDir = BaseDir + "/Icons";
        const string EnvDir = BaseDir + "/Environment";
        const string CCTVDir = BaseDir + "/CCTV";

        [MenuItem("Mỏ An Toàn/Tạo Bộ Asset Mẫu (Màn 1)")]
        public static void GenerateAllAssets()
        {
            EnsureDirectories();

            // 1. UI 9-slice frames & buttons
            CreateFrameHUD();
            CreateFrameCamera();
            CreateFrameDialogue();
            CreateFrameHint();
            CreateChunkyButton("Btn_Green", Hex("#2ecc71"), Hex("#27ae60"), Hex("#1e824c"));
            CreateChunkyButton("Btn_Red", Hex("#e74c3c"), Hex("#c0392b"), Hex("#962d22"));
            CreateChunkyButton("Btn_Yellow", Hex("#f39c12"), Hex("#d68910"), Hex("#b76d05"));
            CreateTitleBadge();
            CreateSlotCard();

            // 2. Icons
            CreateAvatarChibi();
            CreateShieldIcon();
            CreateHeartIcon();
            CreateGearIcon();
            CreateLightbulbIcon();
            CreateCheckIcon();
            CreateCrossIcon();
            CreateGasIcon();
            CreateTempIcon();
            CreateHumidityIcon();
            CreateLocationIcon();
            CreateBookIcon();
            CreateGraphIcon();
            CreateMongDuongLogo();

            // 3. CCTV Camera Views
            CreateCCTVViews();

            // 4. Character & Background
            CreateDispatcherCharacter();
            CreateControlRoomBG();

            AssetDatabase.Refresh();
            ConfigureTextureImporters();
            AssetDatabase.SaveAssets();

            Debug.Log("<color=#35c76a><b>[Mỏ An Toàn]</b> Đã tạo thành công toàn bộ Asset mẫu cho Màn 1!</color>");
        }

        static void EnsureDirectories()
        {
            Directory.CreateDirectory(UIDir);
            Directory.CreateDirectory(IconDir);
            Directory.CreateDirectory(EnvDir);
            Directory.CreateDirectory(CCTVDir);
        }

        static Color Hex(string hex)
        {
            Color c;
            ColorUtility.TryParseHtmlString(hex, out c);
            return c;
        }

        static void SaveTexture(Texture2D tex, string path)
        {
            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(path, bytes);
        }

        // ========================================================================= UI FRAMES & BUTTONS
        static void CreateFrameHUD()
        {
            int size = 128;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color fillTop = Hex("#16233d");
            Color fillBot = Hex("#0c1527");
            Color border = Hex("#2563eb");
            Color glow = Hex("#3b82f6");

            for (int y = 0; y < size; y++)
            {
                float ty = (float)y / size;
                Color bg = Color.Lerp(fillBot, fillTop, ty);
                for (int x = 0; x < size; x++)
                {
                    int dLeft = x; int dRight = size - 1 - x;
                    int dBot = y; int dTop = size - 1 - y;
                    int dEdge = Mathf.Min(Mathf.Min(dLeft, dRight), Mathf.Min(dBot, dTop));

                    if (dEdge < 2) tex.SetPixel(x, y, glow);
                    else if (dEdge < 6) tex.SetPixel(x, y, border);
                    else if (dEdge < 8) tex.SetPixel(x, y, Color.Lerp(border, bg, 0.5f));
                    else tex.SetPixel(x, y, bg);
                }
            }
            tex.Apply();
            SaveTexture(tex, UIDir + "/Frame_HUD.png");
        }

        static void CreateFrameCamera()
        {
            int size = 96;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color fill = Hex("#0f172a");
            Color border = Hex("#1e293b");
            Color accent = Hex("#38bdf8");

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int dEdge = Mathf.Min(Mathf.Min(x, size - 1 - x), Mathf.Min(y, size - 1 - y));
                    if (dEdge < 2) tex.SetPixel(x, y, accent);
                    else if (dEdge < 5) tex.SetPixel(x, y, border);
                    else tex.SetPixel(x, y, fill);
                }
            }
            tex.Apply();
            SaveTexture(tex, UIDir + "/Frame_Camera.png");
        }

        static void CreateFrameDialogue()
        {
            int size = 128;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color fill = Hex("#ffffff");
            Color border = Hex("#cbd5e1");
            Color outline = Hex("#94a3b8");

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int dEdge = Mathf.Min(Mathf.Min(x, size - 1 - x), Mathf.Min(y, size - 1 - y));
                    if (dEdge < 2) tex.SetPixel(x, y, outline);
                    else if (dEdge < 6) tex.SetPixel(x, y, border);
                    else tex.SetPixel(x, y, fill);
                }
            }
            tex.Apply();
            SaveTexture(tex, UIDir + "/Frame_Dialogue.png");
        }

        static void CreateFrameHint()
        {
            int size = 96;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color fill = Hex("#fef3c7");
            Color border = Hex("#f59e0b");
            Color outline = Hex("#b45309");

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int dEdge = Mathf.Min(Mathf.Min(x, size - 1 - x), Mathf.Min(y, size - 1 - y));
                    if (dEdge < 2) tex.SetPixel(x, y, outline);
                    else if (dEdge < 5) tex.SetPixel(x, y, border);
                    else tex.SetPixel(x, y, fill);
                }
            }
            tex.Apply();
            SaveTexture(tex, UIDir + "/Frame_Hint.png");
        }

        static void CreateChunkyButton(string name, Color hi, Color mid, Color shadow)
        {
            int w = 128; int h = 64;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            for (int y = 0; y < h; y++)
            {
                float ty = (float)y / h;
                for (int x = 0; x < w; x++)
                {
                    int dEdge = Mathf.Min(Mathf.Min(x, w - 1 - x), Mathf.Min(y, h - 1 - y));
                    if (dEdge < 2)
                    {
                        tex.SetPixel(x, y, shadow);
                    }
                    else if (y < 8)
                    {
                        // 3D bottom bevel shadow
                        tex.SetPixel(x, y, shadow);
                    }
                    else if (y > h - 6)
                    {
                        // Top glossy highlight
                        tex.SetPixel(x, y, Color.Lerp(mid, Color.white, 0.45f));
                    }
                    else
                    {
                        Color c = Color.Lerp(mid, hi, ty);
                        tex.SetPixel(x, y, c);
                    }
                }
            }
            tex.Apply();
            SaveTexture(tex, UIDir + "/" + name + ".png");
        }

        static void CreateTitleBadge()
        {
            int w = 128; int h = 64;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Color fill = Hex("#1e3a8a");
            Color border = Hex("#60a5fa");
            Color outline = Hex("#172554");

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int dEdge = Mathf.Min(Mathf.Min(x, w - 1 - x), Mathf.Min(y, h - 1 - y));
                    if (dEdge < 2) tex.SetPixel(x, y, outline);
                    else if (dEdge < 5) tex.SetPixel(x, y, border);
                    else tex.SetPixel(x, y, fill);
                }
            }
            tex.Apply();
            SaveTexture(tex, UIDir + "/Badge_Title.png");
        }

        static void CreateSlotCard()
        {
            int size = 96;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color fill = Hex("#1e293b");
            Color border = Hex("#334155");

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int dEdge = Mathf.Min(Mathf.Min(x, size - 1 - x), Mathf.Min(y, size - 1 - y));
                    if (dEdge < 3) tex.SetPixel(x, y, border);
                    else tex.SetPixel(x, y, fill);
                }
            }
            tex.Apply();
            SaveTexture(tex, UIDir + "/Slot_Card.png");
        }

        // ========================================================================= ICONS
        static void CreateAvatarChibi()
        {
            int size = 128;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Vector2 c = new Vector2(size / 2f, size / 2f);
            float r = size / 2f - 2;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), c);
                    if (d > r) { tex.SetPixel(x, y, Color.clear); continue; }
                    if (d > r - 3) { tex.SetPixel(x, y, Hex("#3b82f6")); continue; }

                    // Avatar face & helmet
                    if (y > 64)
                    {
                        // Helmet (white with blue badge)
                        float hd = Vector2.Distance(new Vector2(x, y), new Vector2(64, 76));
                        if (hd < 12) tex.SetPixel(x, y, Hex("#2563eb")); // Emblem
                        else tex.SetPixel(x, y, Hex("#f8fafc")); // White helmet
                    }
                    else if (y > 28)
                    {
                        // Face skin
                        Color skin = Hex("#fed7aa");
                        // Eyes
                        if (y > 40 && y < 50 && (Mathf.Abs(x - 48) < 4 || Mathf.Abs(x - 80) < 4))
                            tex.SetPixel(x, y, Hex("#1e293b"));
                        // Smile
                        else if (y > 32 && y < 38 && Mathf.Abs(x - 64) < 10)
                            tex.SetPixel(x, y, Hex("#e11d48"));
                        else
                            tex.SetPixel(x, y, skin);
                    }
                    else
                    {
                        // Blue miner shirt collar
                        tex.SetPixel(x, y, Hex("#1d4ed8"));
                    }
                }
            }
            tex.Apply();
            SaveTexture(tex, IconDir + "/Icon_Avatar_Chibi.png");
        }

        static void CreateShieldIcon()
        {
            int s = 64;
            var tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
            for (int y = 0; y < s; y++)
            {
                for (int x = 0; x < s; x++)
                {
                    float nx = (x - 32f) / 28f;
                    float ny = (y - 32f) / 28f;
                    if (Mathf.Abs(nx) <= 0.85f && ny <= 0.8f && (ny >= -0.7f + Mathf.Abs(nx) * 0.8f))
                    {
                        // White checkmark inside
                        if (x >= 24 && x <= 44 && Mathf.Abs((y - 28) - (x > 32 ? (x - 32) : -(x - 32))) < 3)
                            tex.SetPixel(x, y, Color.white);
                        else
                            tex.SetPixel(x, y, Hex("#22c55e"));
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }
            tex.Apply();
            SaveTexture(tex, IconDir + "/Icon_Shield.png");
        }

        static void CreateHeartIcon()
        {
            int s = 64;
            var tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
            for (int y = 0; y < s; y++)
            {
                for (int x = 0; x < s; x++)
                {
                    float nx = (x - 32f) / 24f;
                    float ny = (y - 34f) / 24f;
                    // Heart equation (x^2 + y^2 - 1)^3 - x^2 * y^3 <= 0
                    float eq = Mathf.Pow(nx * nx + ny * ny - 0.9f, 3) - (nx * nx) * Mathf.Pow(ny, 3);
                    if (eq <= 0)
                    {
                        // 3D gloss
                        if (ny > 0.1f && nx < -0.1f) tex.SetPixel(x, y, Hex("#fca5a5"));
                        else tex.SetPixel(x, y, Hex("#ef4444"));
                    }
                    else tex.SetPixel(x, y, Color.clear);
                }
            }
            tex.Apply();
            SaveTexture(tex, IconDir + "/Icon_Heart.png");
        }

        static void CreateGearIcon()
        {
            int s = 64;
            var tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
            Vector2 c = new Vector2(32, 32);
            for (int y = 0; y < s; y++)
            {
                for (int x = 0; x < s; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), c);
                    float angle = Mathf.Atan2(y - 32, x - 32);
                    float teeth = Mathf.Cos(angle * 6);
                    float r = 22 + teeth * 4;

                    if (d > r) tex.SetPixel(x, y, Color.clear);
                    else if (d < 8) tex.SetPixel(x, y, Color.clear); // Inner hole
                    else if (d < 11) tex.SetPixel(x, y, Hex("#94a3b8"));
                    else tex.SetPixel(x, y, Hex("#e2e8f0"));
                }
            }
            tex.Apply();
            SaveTexture(tex, IconDir + "/Icon_Gear.png");
        }

        static void CreateLightbulbIcon()
        {
            int s = 64;
            var tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
            for (int y = 0; y < s; y++)
            {
                for (int x = 0; x < s; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), new Vector2(32, 38));
                    if (d < 16)
                    {
                        tex.SetPixel(x, y, Hex("#facc15"));
                    }
                    else if (y < 26 && y > 14 && Mathf.Abs(x - 32) < 8)
                    {
                        tex.SetPixel(x, y, Hex("#94a3b8"));
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }
            tex.Apply();
            SaveTexture(tex, IconDir + "/Icon_Bulb.png");
        }

        static void CreateCheckIcon()
        {
            int s = 64;
            var tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
            Vector2 c = new Vector2(32, 32);
            for (int y = 0; y < s; y++)
            {
                for (int x = 0; x < s; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), c);
                    if (d > 28) { tex.SetPixel(x, y, Color.clear); continue; }
                    // Checkmark
                    bool isCheck = (x >= 20 && x <= 46 && Mathf.Abs((y - 28) - (x > 30 ? (x - 30) : -(x - 30))) < 4);
                    if (isCheck) tex.SetPixel(x, y, Color.white);
                    else tex.SetPixel(x, y, Hex("#22c55e"));
                }
            }
            tex.Apply();
            SaveTexture(tex, IconDir + "/Icon_Check.png");
        }

        static void CreateCrossIcon()
        {
            int s = 64;
            var tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
            Vector2 c = new Vector2(32, 32);
            for (int y = 0; y < s; y++)
            {
                for (int x = 0; x < s; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), c);
                    if (d > 28) { tex.SetPixel(x, y, Color.clear); continue; }
                    // X
                    bool isCross = (Mathf.Abs((x - 32) - (y - 32)) < 4 || Mathf.Abs((x - 32) + (y - 32)) < 4) && d < 18;
                    if (isCross) tex.SetPixel(x, y, Color.white);
                    else tex.SetPixel(x, y, Hex("#ef4444"));
                }
            }
            tex.Apply();
            SaveTexture(tex, IconDir + "/Icon_Cross.png");
        }

        static void CreateGasIcon()
        {
            int s = 64;
            var tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
            for (int y = 0; y < s; y++)
            {
                for (int x = 0; x < s; x++)
                {
                    bool w1 = Mathf.Abs(y - 44 - Mathf.Sin(x * 0.15f) * 4) < 3 && x > 12 && x < 52;
                    bool w2 = Mathf.Abs(y - 32 - Mathf.Sin(x * 0.15f + 1) * 4) < 3 && x > 8 && x < 56;
                    bool w3 = Mathf.Abs(y - 20 - Mathf.Sin(x * 0.15f + 2) * 4) < 3 && x > 14 && x < 48;
                    if (w1 || w2 || w3) tex.SetPixel(x, y, Hex("#38bdf8"));
                    else tex.SetPixel(x, y, Color.clear);
                }
            }
            tex.Apply();
            SaveTexture(tex, IconDir + "/Icon_Gas.png");
        }

        static void CreateTempIcon()
        {
            int s = 64;
            var tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
            for (int y = 0; y < s; y++)
            {
                for (int x = 0; x < s; x++)
                {
                    float dBottom = Vector2.Distance(new Vector2(x, y), new Vector2(32, 20));
                    bool stem = Mathf.Abs(x - 32) < 4 && y >= 20 && y <= 50;
                    if (dBottom < 10 || stem) tex.SetPixel(x, y, Hex("#ef4444"));
                    else if (dBottom < 13 || (Mathf.Abs(x - 32) < 7 && y >= 20 && y <= 53)) tex.SetPixel(x, y, Hex("#f8fafc"));
                    else tex.SetPixel(x, y, Color.clear);
                }
            }
            tex.Apply();
            SaveTexture(tex, IconDir + "/Icon_Temp.png");
        }

        static void CreateHumidityIcon()
        {
            int s = 64;
            var tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
            for (int y = 0; y < s; y++)
            {
                for (int x = 0; x < s; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), new Vector2(32, 26));
                    float ny = y - 26;
                    bool topCone = ny > 0 && ny < 26 && Mathf.Abs(x - 32) < (26 - ny) * 0.6f;
                    if (d < 14 || topCone) tex.SetPixel(x, y, Hex("#0ea5e9"));
                    else tex.SetPixel(x, y, Color.clear);
                }
            }
            tex.Apply();
            SaveTexture(tex, IconDir + "/Icon_Humidity.png");
        }

        static void CreateLocationIcon()
        {
            int s = 64;
            var tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
            for (int y = 0; y < s; y++)
            {
                for (int x = 0; x < s; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), new Vector2(32, 38));
                    bool pinPoint = y >= 14 && y <= 38 && Mathf.Abs(x - 32) < (y - 14) * 0.7f;
                    if ((d < 16 || pinPoint))
                    {
                        if (d < 6) tex.SetPixel(x, y, Hex("#1e293b"));
                        else tex.SetPixel(x, y, Hex("#38bdf8"));
                    }
                    else tex.SetPixel(x, y, Color.clear);
                }
            }
            tex.Apply();
            SaveTexture(tex, IconDir + "/Icon_Location.png");
        }

        static void CreateBookIcon()
        {
            int s = 64;
            var tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
            for (int y = 0; y < s; y++)
            {
                for (int x = 0; x < s; x++)
                {
                    if (y >= 18 && y <= 46 && Mathf.Abs(x - 32) > 2 && Mathf.Abs(x - 32) < 24)
                        tex.SetPixel(x, y, Hex("#cbd5e1"));
                    else if (y >= 16 && y <= 48 && (Mathf.Abs(x - 32) <= 2 || Mathf.Abs(x - 32) == 24))
                        tex.SetPixel(x, y, Hex("#3b82f6"));
                    else
                        tex.SetPixel(x, y, Color.clear);
                }
            }
            tex.Apply();
            SaveTexture(tex, IconDir + "/Icon_Book.png");
        }

        static void CreateGraphIcon()
        {
            int w = 128; int h = 64;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float wave = Mathf.Sin(x * 0.12f) * 12 + Mathf.Cos(x * 0.05f) * 6 + 28;
                    float d = Mathf.Abs(y - wave);
                    if (d < 2.5f) tex.SetPixel(x, y, Hex("#22c55e"));
                    else if (y < wave) tex.SetPixel(x, y, new Color(0.13f, 0.77f, 0.36f, 0.2f));
                    else tex.SetPixel(x, y, Color.clear);
                }
            }
            tex.Apply();
            SaveTexture(tex, IconDir + "/Icon_Graph.png");
        }

        static void CreateMongDuongLogo()
        {
            int w = 256; int h = 64;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    // Mountain peaks on the left (x: 10..60)
                    bool m1 = (y >= 16 && y <= 48 && Mathf.Abs(x - 32) < (48 - y) * 0.75f);
                    bool m2 = (y >= 16 && y <= 40 && Mathf.Abs(x - 48) < (40 - y) * 0.75f);
                    if (m1 || m2) tex.SetPixel(x, y, Hex("#3b82f6"));
                    else tex.SetPixel(x, y, Color.clear);
                }
            }
            tex.Apply();
            SaveTexture(tex, IconDir + "/Logo_MongDuong.png");
        }

        // ========================================================================= CCTV VIEWS
        static void CreateCCTVViews()
        {
            // 4 CCTV views: Cam01 (Rails), Cam02 (Vent Duct), Cam03 (Substation), Cam04 (Face)
            int w = 256; int h = 192;

            for (int c = 1; c <= 4; c++)
            {
                var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
                Vector2 center = new Vector2(w / 2f, h / 2f + 10);

                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        float d = Vector2.Distance(new Vector2(x, y), center);
                        // Vignette & tunnel arch
                        float darkness = Mathf.Clamp01(d / 120f);
                        Color cBase = Color.Lerp(Hex("#334155"), Hex("#090d16"), darkness);

                        // Tunnel arch perspective lines
                        if (y < 80)
                        {
                            // Ground / Rails
                            float rail1 = Mathf.Abs((x - 128) - (80 - y) * 1.2f);
                            float rail2 = Mathf.Abs((x - 128) + (80 - y) * 1.2f);
                            if (rail1 < 3 || rail2 < 3) cBase = Hex("#94a3b8");
                        }
                        else
                        {
                            // Ceiling arch
                            if (c == 2)
                            {
                                // Cam 2: Yellow/green flexible ventilation duct on ceiling
                                if (x > 60 && x < 90 && y > 100) cBase = Hex("#eab308");
                            }
                            else if (c == 3)
                            {
                                // Cam 3: Substation metal panel with warning hazard sign
                                if (Mathf.Abs(x - 128) < 30 && y < 110 && y > 50) cBase = Hex("#64748b");
                            }
                        }

                        // Scanline effect
                        if (y % 4 == 0) cBase *= 0.85f;

                        tex.SetPixel(x, y, cBase);
                    }
                }
                tex.Apply();
                SaveTexture(tex, CCTVDir + $"/CCTV_Cam0{c}.png");
            }
        }

        // ========================================================================= ENVIRONMENT & CHARACTER
        static void CreateDispatcherCharacter()
        {
            int w = 180; int h = 220;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    // Back/3-quarter angle of dispatcher sitting on chair
                    // Helmet (top)
                    float dHead = Vector2.Distance(new Vector2(x, y), new Vector2(90, 165));
                    if (dHead < 42 && y > 140)
                    {
                        // White helmet with light/emblem
                        tex.SetPixel(x, y, Hex("#f1f5f9"));
                        continue;
                    }

                    // Blue miner uniform / shoulders
                    if (y <= 145 && y >= 60 && Mathf.Abs(x - 90) < (150 - y) * 0.7f)
                    {
                        // Reflective stripes
                        if (y >= 100 && y <= 112) tex.SetPixel(x, y, Hex("#eab308"));
                        else tex.SetPixel(x, y, Hex("#1d4ed8"));
                        continue;
                    }

                    // Office chair back
                    if (y < 90 && y >= 20 && Mathf.Abs(x - 90) < 55)
                    {
                        tex.SetPixel(x, y, Hex("#1e293b"));
                        continue;
                    }

                    tex.SetPixel(x, y, Color.clear);
                }
            }
            tex.Apply();
            SaveTexture(tex, EnvDir + "/Char_Dispatcher.png");
        }

        static void CreateControlRoomBG()
        {
            int w = 420; int h = 860;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);

            Color wallTop = Hex("#3b82f6");
            Color wallMid = Hex("#1e3a8a");
            Color wallBot = Hex("#0f172a");

            for (int y = 0; y < h; y++)
            {
                float ty = (float)y / h;
                Color baseC = ty > 0.6f ? Color.Lerp(wallMid, wallTop, (ty - 0.6f) / 0.4f) : Color.Lerp(wallBot, wallMid, ty / 0.6f);

                for (int x = 0; x < w; x++)
                {
                    // Window showing headframe tower in the upper right
                    if (x > 310 && x < 400 && y > 680 && y < 810)
                    {
                        tex.SetPixel(x, y, Hex("#0284c7"));
                    }
                    // Desk area at the bottom
                    else if (y < 280)
                    {
                        tex.SetPixel(x, y, Hex("#1e293b"));
                    }
                    else
                    {
                        tex.SetPixel(x, y, baseC);
                    }
                }
            }
            tex.Apply();
            SaveTexture(tex, EnvDir + "/BG_ControlRoom.png");
        }

        // ========================================================================= CONFIGURE IMPORTERS
        static void ConfigureTextureImporters()
        {
            ConfigureDir(UIDir, true, 16);
            ConfigureDir(IconDir, false, 0);
            ConfigureDir(EnvDir, false, 0);
            ConfigureDir(CCTVDir, false, 0);
        }

        static void ConfigureDir(string dir, bool isNineSlice, int border)
        {
            if (!Directory.Exists(dir)) return;
            string[] files = Directory.GetFiles(dir, "*.png");
            foreach (var f in files)
            {
                string path = f.Replace("\\", "/");
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.alphaIsTransparency = true;
                    importer.mipmapEnabled = false;
                    importer.filterMode = FilterMode.Bilinear;

                    if (isNineSlice)
                    {
                        importer.spriteBorder = new Vector4(border, border, border, border);
                    }
                    EditorUtility.SetDirty(importer);
                    importer.SaveAndReimport();
                }
            }
        }
    }
}
