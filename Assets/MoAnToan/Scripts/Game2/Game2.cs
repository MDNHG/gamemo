using System;
using UnityEngine;
using UnityEngine.UI;

namespace MoAnToan
{
    /// <summary>
    /// Vòng lặp gameplay Game 2: "Trực ban an toàn".
    /// Giám sát camera hầm lò, theo dõi cảm biến SCADA, tiếp nhận báo cáo và ra quyết định phê duyệt / dừng lò.
    /// Sử dụng giao diện Prefab Canvas.prefab và Game2View.
    /// </summary>
    public class Game2
    {
        int curIncidentIdx = 0;
        int hearts = 3;
        int stars = 0;
        Game2View view;

        public void Start(int incidentIdx = 0)
        {
            curIncidentIdx = Mathf.Clamp(incidentIdx, 0, Data2.Incidents.Count - 1);
            hearts = 3;

            // Xóa màn hình cũ
            var root = App.I.NewScreen();

            // Load và tạo Canvas.prefab
            var prefab = Resources.Load<GameObject>("Game2/Canvas");
#if UNITY_EDITOR
            if (prefab == null)
            {
                prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/MoAnToan/Scripts/Game2/Canvas.prefab");
            }
#endif
            if (prefab != null)
            {
                // Kiểm tra nếu đã có Game2Canvas cũ thì xóa
                var oldCanvas = GameObject.Find("Game2Canvas");
                if (oldCanvas != null) UnityEngine.Object.Destroy(oldCanvas);

                var go = UnityEngine.Object.Instantiate(prefab);
                go.name = "Game2Canvas";
                view = go.GetComponent<Game2View>();
                if (view != null)
                {
                    SetupView();
                    LoadIncident(curIncidentIdx);
                    return;
                }
            }

            // Fallback nếu không có prefab
            DispatcherGame.Open();
        }

        void SetupView()
        {
            view.SetHearts(hearts);

            if (view.btnSettings != null)
            {
                view.btnSettings.onClick.RemoveAllListeners();
                view.btnSettings.onClick.AddListener(() =>
                {
                    Sfx.Play("tap");
                    var c = GameObject.Find("Game2Canvas");
                    if (c != null) UnityEngine.Object.Destroy(c);
                    App.I.ShowMenu();
                });
            }

            if (view.btnRuleBook != null)
            {
                view.btnRuleBook.onClick.RemoveAllListeners();
                view.btnRuleBook.onClick.AddListener(() => view.ShowRuleModal());
            }

            if (view.btnAllowWork != null)
            {
                view.btnAllowWork.onClick.RemoveAllListeners();
                view.btnAllowWork.onClick.AddListener(() => OnDecision(true));
            }

            if (view.btnStopProduction != null)
            {
                view.btnStopProduction.onClick.RemoveAllListeners();
                view.btnStopProduction.onClick.AddListener(() => OnDecision(false));
            }

            if (view.cameraButtons != null)
            {
                for (int i = 0; i < view.cameraButtons.Length; i++)
                {
                    int camIdx = i + 1;
                    var btn = view.cameraButtons[i];
                    if (btn != null)
                    {
                        btn.onClick.RemoveAllListeners();
                        btn.onClick.AddListener(() => OnClickCamera(camIdx));
                    }
                }
            }
        }

        void LoadIncident(int idx)
        {
            if (idx >= Data2.Incidents.Count)
            {
                view.ShowVictoryModal(() =>
                {
                    var c = GameObject.Find("Game2Canvas");
                    if (c != null) UnityEngine.Object.Destroy(c);
                    App.I.ShowMenu();
                });
                return;
            }

            curIncidentIdx = idx;
            var inc = Data2.Incidents[curIncidentIdx];
            view.BindIncident(inc, curIncidentIdx + 1);
            view.SetHearts(hearts);
        }

        void OnDecision(bool chooseWork)
        {
            var inc = Data2.Incidents[curIncidentIdx];
            bool requireStop = inc.requireStop;
            bool isCorrect = (chooseWork && !requireStop) || (!chooseWork && requireStop);

            if (isCorrect)
            {
                stars += 25;
                view.ShowResultFeedback(true, "XỬ LÝ CHÍNH XÁC! (+25 XP)", inc.explanation, () =>
                {
                    LoadIncident(curIncidentIdx + 1);
                });
            }
            else
            {
                hearts--;
                view.SetHearts(hearts);
                string failMsg = "Hành động này tiềm ẩn nguy cơ mất an toàn nghiêm trọng trong hầm lò.\n\n<b>Gợi ý quy chuẩn:</b>\n" + inc.tip;
                view.ShowResultFeedback(false, "QUYẾT ĐỊNH CHƯA AN TOÀN! (-1 Tim)", failMsg, () =>
                {
                    if (hearts <= 0)
                    {
                        Start(0);
                    }
                });
            }
        }

        void OnClickCamera(int camIdx)
        {
            Sfx.Play("tap");
            var inc = Data2.Incidents[curIncidentIdx];
            string detail = "Bình thường";
            switch (camIdx)
            {
                case 1: detail = inc.cam01Status; break;
                case 2: detail = inc.cam02Status; break;
                case 3: detail = inc.cam03Status; break;
                case 4: detail = inc.cam04Status; break;
            }

            UIK.Modal((m, close) =>
            {
                UIK.Txt(m, $"📸 Camera 0{camIdx} — Giám sát hiện trường", 16, UIK.Ink, FontStyle.Bold, TextAnchor.MiddleCenter);
                var ic = MATArt.Img(m, MATArt.CCTV($"CCTV_Cam0{camIdx}"));
                UIK.LE(ic.gameObject, 280, 200, -1, 280, 200);
                var t = UIK.Txt(m, $"<b>Trạng thái:</b> {detail}", 13, UIK.Hex("#93c5fd"), FontStyle.Normal, TextAnchor.MiddleLeft);
                t.lineSpacing = 1.15f;
                UIK.Btn(m, "Đóng", UIK.Hex("#2563eb"), UIK.Ink, 14, () => close(), 42);
            });
        }
    }
}
