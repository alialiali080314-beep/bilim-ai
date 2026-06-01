using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [System.Serializable]
    public class Sound
    {
        public string id;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        [Range(0.8f, 1.2f)] public float pitchVariance = 1f;
        public bool loop;
        [HideInInspector] public AudioSource source;
    }

    [SerializeField] private Sound[] sounds;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        foreach (var s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.loop = s.loop;
        }
    }

    public void Play(string id)
    {
        Sound s = Find(id);
        if (s == null) { Debug.LogWarning($"Sound '{id}' not found"); return; }
        s.source.pitch = Random.Range(1f / s.pitchVariance, s.pitchVariance);
        s.source.Play();
    }

    public void Stop(string id) => Find(id)?.source.Stop();

    public void PlayAtPoint(AudioClip clip, Vector3 pos, float vol = 1f)
        => AudioSource.PlayClipAtPoint(clip, pos, vol);

    private Sound Find(string id)
    {
        foreach (var s in sounds)
            if (s.id == id) return s;
        return null;
    }
}
