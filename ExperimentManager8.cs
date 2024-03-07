using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public struct Trial
{
    public float mass1;
    public float mass2;
    public float scale1;
    public float scale2;
    public int sound1;
    public int sound2;

    public Trial(float m1, float m2, float sz1, float sz2, int snd1, int snd2)
    {
        mass1 = m1;
        mass2 = m2;
        scale1 = sz1;
        scale2 = sz2;
        sound1 = snd1;
        sound2 = snd2;
    }

    public static string Header()
    {
        return "Mass1,Mass2,Size1,Size2,Sound1,Sound2";
    }

    public string Values()
    {
        return $"{mass1},{mass2},{scale1},{scale2},{sound1},{sound2}";
    }
}

public class ExperimentManager8 : MonoBehaviour
{
    [SerializeField] private GameObject target1;
    [SerializeField] private GameObject target2;
    [SerializeField] private TMP_Text message;
    [SerializeField] private Button left_button;
    [SerializeField] private Button right_button;
    [SerializeField] private GameObject canvas;

    [SerializeField] private AudioClip[] clips;
    [SerializeField] private float[] masses = { 0.5f, 1.0f, 1.5f };
    [SerializeField] private float[] sizes = { 0.75f, 1.0f };

    private Trial[] trials;
    private int[] responses;
    private int trial = 0;

    private string experiment;

    private float show_time = float.MaxValue;

    private Vector3 reset_position_1;
    private Quaternion reset_rotation_1;
    private Vector3 reset_scale_1;

    private Vector3 reset_position_2;
    private Quaternion reset_rotation_2;
    private Vector3 reset_scale_2;

    // Start is called before the first frame update
    void Start()
    {
        // allocate arrays to match fields
        trials = new Trial[masses.Length * masses.Length * sizes.Length * sizes.Length * clips.Length * clips.Length];
        responses = new int[trials.Length];

        // set up all factor combos -- there are potentially an awful lot of these!
        int ii = 0;

        for ( int m1 = 0; m1 < masses.Length; ++m1 )
        {
            for ( int m2 = 0; m2 < masses.Length; ++m2 )
            {
                for ( int s1 = 0; s1 < sizes.Length; ++s1 )
                {
                    for ( int s2 = 0; s2 < sizes.Length; ++s2 )
                    {
                        for ( int c1 = 0; c1 < clips.Length; ++c1 )
                        {
                            for ( int c2 = 0; c2 < clips.Length; ++c2 )
                            {
                                trials[ii] = new Trial(masses[m1], masses[m2], sizes[s1], sizes[s2], c1, c2);
                                ++ii;
                            }
                        }
                    }
                }
            }
        }

        // shuffle order for presentation
        for ( ii = 0; ii < trials.Length; ++ii )
        {
            int rr = Random.Range(ii, trials.Length);
            (trials[rr], trials[ii]) = (trials[ii], trials[rr]);
        }

        // find an unused experiment ID
        do
        {
            experiment = RandomString(4);
        }
        while ( File.Exists(Path.Combine(Application.persistentDataPath, $"{experiment}.csv")) );

        reset_position_1 = target1.transform.position;
        reset_rotation_1 = target1.transform.rotation;
        reset_scale_1 = target1.transform.localScale;

        reset_position_2 = target2.transform.position;
        reset_rotation_2 = target2.transform.rotation;
        reset_scale_2 = target2.transform.localScale;

        // set initial values
        ConfigureTrial();

        // listen for button clicks
        left_button.onClick.AddListener(delegate {Respond(0);});
        right_button.onClick.AddListener(delegate {Respond(1);});

        canvas.SetActive(false);
        show_time = Time.time + 1.5f;
    }

    string RandomString(int length)
    {
        char[] chars = new char[length];
        for ( int ii = 0 ; ii < length; ++ii )
        {
            chars[ii] = (char) ('A' + Random.Range(0, 25));
        }

        return new string(chars);
    }

    void ConfigureTrial ()
    {
        Trial t = trials[trial];

        target1.GetComponent<Rigidbody>().mass = t.mass1;
        target1.transform.localScale = new Vector3(t.scale1 * reset_scale_1.x,
                                                   t.scale1 * reset_scale_1.y,
                                                   t.scale1 * reset_scale_1.z);
        target1.GetComponent<AudioSource>().clip = clips[t.sound1];

        target2.GetComponent<Rigidbody>().mass = t.mass2;
        target2.transform.localScale = new Vector3(t.scale2 * reset_scale_2.x,
                                                   t.scale2 * reset_scale_2.y,
                                                   t.scale2 * reset_scale_2.z);
        target2.GetComponent<AudioSource>().clip = clips[t.sound2];

        message.text = $"[{experiment}: {trial + 1}/{trials.Length}] Which block feels heavier?";
    }

    // log an experiment response
    void Respond ( int response )
    {
        responses[trial] = response;

        Debug.Log($"response received: {response}");

        trial += 1;
        if ( trial >= trials.Length )
        {
            message.text = $"Experiment complete. Saved to '{experiment}.csv'.";
            left_button.gameObject.SetActive(false);
            right_button.gameObject.SetActive(false);
            SaveResults();
        }
        else
        {
            ConfigureTrial();
        }

        canvas.SetActive(false);
        show_time = Time.time + 1.5f;
    }

    void Update()
    {
        if ( Time.time >= show_time )
        {
            show_time = float.MaxValue;
            canvas.SetActive(true);
        }
    }

    void SaveResults()
    {
        string outfile = Path.Combine(Application.persistentDataPath, $"{experiment}.csv");
        using StreamWriter writer = new StreamWriter(outfile);
        writer.WriteLine($"Trial,{Trial.Header()}");
        for (int ii = 0; ii < trials.Length; ++ii)
        {
            writer.WriteLine($"{ii + 1},{trials[ii].Values()}");
        }
    }
}
