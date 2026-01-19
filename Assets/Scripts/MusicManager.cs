using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Musik Clips")]
    public AudioClip lugnMusik;
    public AudioClip stridsMusik;

    [Header("Audio Settings")]
    [Range(0f, 1f)]
    public float musikVolym = 0.7f;
    public float fadeHastighet = 1.5f;

    private AudioSource audioSource1;
    private AudioSource audioSource2;
    private AudioSource aktivKalla;
    private AudioSource inaktivKalla;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        audioSource1 = gameObject.AddComponent<AudioSource>();
        audioSource2 = gameObject.AddComponent<AudioSource>();

        audioSource1.loop = true;
        audioSource2.loop = true;
        audioSource1.volume = 0f;
        audioSource2.volume = 0f;

        aktivKalla = audioSource1;
        inaktivKalla = audioSource2;
    }

    void Start()
    {
        SpelaMusik(lugnMusik, audioSource1);
    }

    void Update()
    {
        if (aktivKalla.volume < musikVolym)
        {
            aktivKalla.volume = Mathf.MoveTowards(aktivKalla.volume, musikVolym, fadeHastighet * Time.deltaTime);
        }

        if (inaktivKalla.volume > 0f)
        {
            inaktivKalla.volume = Mathf.MoveTowards(inaktivKalla.volume, 0f, fadeHastighet * Time.deltaTime);

            if (inaktivKalla.volume <= 0.01f)
            {
                inaktivKalla.Stop();
            }
        }
    }

    public void StartaStrid()
    {
        BytMusik(stridsMusik);
    }

    public void AvslutaStrid()
    {
        BytMusik(lugnMusik);
    }

    private void BytMusik(AudioClip nyttClip)
    {
        AudioSource temp = aktivKalla;
        aktivKalla = inaktivKalla;
        inaktivKalla = temp;

        SpelaMusik(nyttClip, aktivKalla);
    }

    private void SpelaMusik(AudioClip clip, AudioSource source)
    {
        source.clip = clip;
        source.volume = 0f;
        source.Play();
    }
}