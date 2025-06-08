// As suggested by ChatGPT.
// It's because of somewhat of a fault within Unity, supposedly by design:
// A HBox, containing a text with a content size fitter (for auto sizing), would cause padding to not register until the second rendering.

using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(LayoutElement), typeof(TMP_Text))]
public class TextWidthSizer : MonoBehaviour
{
    private LayoutElement layoutElement;
    private TMP_Text tmpText;
    private RectTransform parentRect;

    void Awake()
    {
        layoutElement = GetComponent<LayoutElement>();
        tmpText = GetComponent<TMP_Text>();
        parentRect = transform.parent as RectTransform;
    }

    void OnEnable()
    {
        // Delay layout update to next frame so TMP has time to generate mesh
        StartCoroutine(DelayedUpdate());
    }

    System.Collections.IEnumerator DelayedUpdate()
    {
        yield return null; // wait one frame
        layoutElement.preferredWidth = tmpText.preferredWidth;

        // Force layout rebuild on parent
        LayoutRebuilder.ForceRebuildLayoutImmediate(parentRect);
    }
}
