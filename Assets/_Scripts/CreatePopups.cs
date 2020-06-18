using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CreatePopups : MonoBehaviour
{
    public GameObject popupPrefab;
    public float delay, fadeoutTime;
    private GameObject currPopup, inst;
    private List<GameObject> popups = new List<GameObject>();
    private static IList<string> popupMsgs = new List<string>();

    public static void SendPopup(object msg, bool dbg = true)
    {
        popupMsgs.Add(msg.ToString());

#if UNITY_EDITOR            
        if (dbg)
            Debug.Log(msg);
#endif

    }
    // Update is called once per frame
    void Update()
    {
        while (popupMsgs.Count > 0)
        {
            inst = Instantiate(popupPrefab, inst == null ? transform : currPopup.transform, false);
            inst.GetComponent<FadeInOut>().delay = delay;
            inst.GetComponent<FadeInOut>().fadeoutTime = fadeoutTime;

            inst.GetComponentInChildren<TextMeshProUGUI>().text = popupMsgs[0];

            if (inst.transform.parent != transform)
                inst.GetComponent<RectTransform>().anchorMax =
                inst.GetComponent<RectTransform>().anchorMin =
                new Vector2(.5f, 0);

            inst.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            inst.GetComponent<RectTransform>().localScale = Vector3.one;
            currPopup = inst;
            popups.Add(inst);
            popupMsgs.RemoveAt(0);
        }

        for (int index = 0; index < popups.Count; ++index)
        {
            if (popups[index] == null)
            {
                popups.RemoveAt(index--);
                continue;
            }

            if (popups[index].GetComponent<CanvasGroup>().alpha == 0)
            {
                if (popups[index].transform.childCount > 1)
                {
                    var child = popups[index].transform.GetChild(1);
                    popups[index].transform.GetChild(1).SetParent(popups[index].transform.parent, false);
                    if (child.parent == transform)
                        child.GetComponent<RectTransform>().anchorMax =
                        child.GetComponent<RectTransform>().anchorMin =
                        new Vector2(.5f, 1);

                    child.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                }
                Destroy(popups[index]);
                //popups[index] = null;
            }
            else
            {
                popups[index].GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,
                    Mathf.Lerp(0, popupPrefab.GetComponent<RectTransform>().sizeDelta.y,
                        popups[index].GetComponent<CanvasGroup>().alpha));
            }
            // popups[index].transform.GetChild(1).GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        }
    }

}