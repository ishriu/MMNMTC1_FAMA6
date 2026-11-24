using UnityEngine;

public static class GameAudioSettings
{
    private const string VolumePrefKey = "MasterVolume";

    public static float MasterVolume
    {
        get => PlayerPrefs.GetFloat(VolumePrefKey, 1f);
        set
        {
            float clamped = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(VolumePrefKey, clamped);
            PlayerPrefs.Save();
            AudioListener.volume = clamped;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ApplySavedVolumeOnLoad()
    {
        AudioListener.volume = MasterVolume;
    }
}
