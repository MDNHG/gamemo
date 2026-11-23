using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MoAnToan
{
    /// <summary>Thẻ vuốt trái/phải (Đúng/Sai).</summary>
    public class SwipeCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public Action<bool> OnDecide;
        public Image Frame;
        float dx; bool done;
        RectTransform rt;
        Vector2 home;
        Color baseCol;

        void Awake() { rt = (RectTransform)transform; }

        public void OnBeginDrag(PointerEventData e) { if (!done) { home = rt.anchoredPosition; if (Frame) baseCol = Frame.color; } }

        public void OnDrag(PointerEventData e)
        {
            if (done) return;
            float k = rt.lossyScale.x > 0 ? rt.lossyScale.x : 1;
            dx += e.delta.x / k;
            rt.anchoredPosition = home + new Vector2(dx, 0);
            rt.localRotation = Quaternion.Euler(0, 0, -dx / 16f);
            if (Frame) Frame.color = dx > 40 ? UIK.Hex("#14301f") : dx < -40 ? UIK.Hex("#3a1715") : baseCol;
        }

        public void OnEndDrag(PointerEventData e)
        {
            if (done) return;
            if (Mathf.Abs(dx) > 90) Decide(dx > 0);
            else { StartCoroutine(Back()); }
        }

        IEnumerator Back()
        {
            Vector2 from = rt.anchoredPosition; float fx = dx;
            for (float t = 0; t < 1; t += Time.unscaledDeltaTime * 6)
            {
                rt.anchoredPosition = Vector2.Lerp(from, home, t);
                rt.localRotation = Quaternion.Euler(0, 0, -Mathf.Lerp(fx, 0, t) / 16f);
                yield return null;
            }
            rt.anchoredPosition = home; rt.localRotation = Quaternion.identity; dx = 0;
            if (Frame) Frame.color = baseCol;
        }

        public void Decide(bool yes)
        {
            if (done) return;
            done = true;
            StartCoroutine(Fly(yes));
        }

        IEnumerator Fly(bool yes)
        {
            Vector2 from = rt.anchoredPosition;
            Vector2 to = from + new Vector2(yes ? 700 : -700, 0);
            for (float t = 0; t < 1; t += Time.unscaledDeltaTime * 4)
            {
                rt.anchoredPosition = Vector2.Lerp(from, to, t * t);
                rt.localRotation = Quaternion.Euler(0, 0, yes ? -18 * t : 18 * t);
                yield return null;
            }
            if (OnDecide != null) OnDecide(yes);
        }
    }

    /// <summary>Ô nhận thẻ kéo thả. index = -1 là khay ban đầu.</summary>
    public class DropZone : MonoBehaviour
    {
        public int index;
        public RectTransform content;
    }

    /// <summary>Thẻ kéo thả được, chạm để chọn.</summary>
    public class DragChip : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public int item;
        public Action<DragChip> OnTap;
        public Action<DragChip, DropZone> OnDrop;
        public bool Locked;
        Transform oldParent; int oldIndex;
        CanvasGroup cg;
        RectTransform rt;

        void Awake()
        {
            rt = (RectTransform)transform;
            cg = gameObject.AddComponent<CanvasGroup>();
        }

        public void OnPointerClick(PointerEventData e) { if (!Locked && OnTap != null) OnTap(this); }

        public void OnBeginDrag(PointerEventData e)
        {
            if (Locked) return;
            oldParent = transform.parent; oldIndex = transform.GetSiblingIndex();
            Vector2 size = rt.rect.size;
            transform.SetParent(UIK.OverlayRoot, true);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            cg.blocksRaycasts = false; cg.alpha = 0.92f;
        }

        public void OnDrag(PointerEventData e)
        {
            if (Locked || oldParent == null) return;
            rt.position += (Vector3)e.delta;
        }

        public void OnEndDrag(PointerEventData e)
        {
            if (Locked || oldParent == null) return;
            cg.blocksRaycasts = true; cg.alpha = 1;
            DropZone z = null;
            var go = e.pointerCurrentRaycast.gameObject;
            if (go != null) z = go.GetComponentInParent<DropZone>();
            if (z != null && OnDrop != null) OnDrop(this, z);
            else { transform.SetParent(oldParent, false); transform.SetSiblingIndex(oldIndex); }
            oldParent = null;
        }

        public void MoveTo(RectTransform content)
        {
            transform.SetParent(content, false);
            rt.localRotation = Quaternion.identity;
        }
    }
}
