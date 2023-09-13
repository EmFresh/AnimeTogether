using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoiceChat : MonoBehaviour
{
	public AudioSource s;

	// Start is called before the first frame update
	void OnStart()
	{
		if(!s) return;

		s.Stop();
		var clip = Microphone.Start(Microphone.devices[0], true, 10, 44100);
		Microphone.GetPosition(Microphone.devices[0]);
		s.resource = clip;
		s.loop = true;
		while(!(Microphone.GetPosition(null) > 0)) ;
		s.Play();
	}
}
