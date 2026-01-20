using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Musik Clips")]
    public AudioClip lugnMusik;
    public AudioClip stridsMusik;

    [Header("Atmosfär (Natur)")]
    public AudioClip naturLjud; // Här lägger du ditt Suno-naturljud
    [Range(0f, 1f)]
    public float naturVolym = 0.5f;

    [Header("Audio Settings")]
    [Range(0f, 1f)]
    public float musikVolym = 0.7f;
    public float fadeHastighet = 1.5f;

    private AudioSource audioSource1;
    private AudioSource audioSource2;
    private AudioSource naturSource; // Ny källa för naturljud

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

        // Setup för musikkällor
        audioSource1 = gameObject.AddComponent<AudioSource>();
        audioSource2 = gameObject.AddComponent<AudioSource>();

        // Setup för naturkälla
        naturSource = gameObject.AddComponent<AudioSource>();
        naturSource.loop = true;
        naturSource.playOnAwake = false;

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

        // Starta naturljudet direkt om det finns
        if (naturLjud != null)
        {
            naturSource.clip = naturLjud;
            naturSource.volume = naturVolym;
            naturSource.Play();
        }
    }

    void Update()
    {
        // Hantera fade för musik (precis som innan)
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

        // Uppdatera naturvolym i realtid om du ändrar i inspektorn
        if (naturSource.isPlaying)
        {
            naturSource.volume = naturVolym;
        }
    }

    // --- Befintliga metoder för strid ---
    public void StartaStrid() { BytMusik(stridsMusik); }
    public void AvslutaStrid() { BytMusik(lugnMusik); }

    private void BytMusik(AudioClip nyttClip)
    {
        if (aktivKalla.clip == nyttClip) return; // Byt inte om det redan spelas

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

    // --- Ny metod om du vill byta naturljud mitt i spelet (t.ex. gå in i en grotta) ---
    public void BytNaturLjud(AudioClip nyttNatur)
    {
        naturLjud = nyttNatur;
        naturSource.clip = naturLjud;
        naturSource.Play();
    }
}