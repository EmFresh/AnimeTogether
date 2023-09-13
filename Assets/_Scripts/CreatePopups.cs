using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEditor;

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
		if(dbg)
			Debug.Log(msg);
#endif
	}
	// Update is called once per frame
	void Update()
	{
		while(popupMsgs.Count > 0)
		{
			inst = Instantiate(popupPrefab, transform, false);
			inst.GetComponent<FadeInOut>().delay = delay;
			inst.GetComponent<FadeInOut>().fadeoutTime = fadeoutTime;

			inst.GetComponentInChildren<TextMeshProUGUI>().text = popupMsgs[0];

			popups.Add(inst);
			popupMsgs.RemoveAt(0);


		}

		for(int index = 0; index < popups.Count; ++index)
		{
			if(popups[index] == null)
			{
				popups.RemoveAt(index--);
				continue;
			}

			if(popups[index].GetComponent<CanvasGroup>().alpha == 0)
				Destroy(popups[index]);
			else
				popups[index].GetComponent<RectTransform>().SetSizeWithCurrentAnchors
					(RectTransform.Axis.Vertical,
					Mathf.Lerp(0, popupPrefab.GetComponent<RectTransform>().sizeDelta.y,
					popups[index].GetComponent<CanvasGroup>().alpha));

		}
	}

}