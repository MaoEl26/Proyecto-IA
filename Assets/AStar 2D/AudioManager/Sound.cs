using UnityEngine.Audio;
using UnityEngine;

[System.Serializable]
public class Sound {

	public string name;

	public AudioClip clip;

	public float volume = 1.0f;

	[HideInInspector]
	public AudioSource source;

}
