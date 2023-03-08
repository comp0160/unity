using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public struct Trial
{
    public float mass1;
    public float mass2;
    public int sound1;
    public int sound2;

    public Trial ( float m1, float m2, int s1, int s2 )
    {
        mass1 = m1;
        mass2 = m2;
        sound1 = s1;
        sound2 = s2;
    }
}

public class ExperimentManager8 : MonoBehaviour
{
    [SerializeField] private int reps = 1;
    [SerializeField] private float lo = 1f;
    [SerializeField] private float hi = 3f;
    [SerializeField] private int levels = 3;
    [SerializeField] private string outfile = "results.csv";

    [SerializeField] private GameObject red_target;
    [SerializeField] private GameObject green_target;

    [SerializeField] private TMP_Text message;
    [SerializeField] private Button red_button;
    [SerializeField] private Button green_button;
    [SerializeField] private GameObject canvas;

    [SerializeField] private AudioClip[] clips;

    private Trial[] trials;
    private int[] responses;
    private int experiment = 0;

    private float show_time = float.MaxValue;

    // Start is called before the first frame update
    void Start()
    {
        // allocate arrays to match fields
        trials = new Trial[reps * levels * levels * clips.Length * clips.Length];
        responses = new int[trials.Length];

        float step = (hi - lo) / (levels - 1);
        int ptr = 0;

        // set up reps of all combos
        for ( int lev1 = 0; lev1 < levels; ++lev1 )
        {
            float mass1 = lo + lev1 * step;

            for ( int lev2 = 0; lev2 < levels; ++lev2 )
            {
                float mass2 = lo + lev2 * step;

                for ( int sound1 = 0; sound1 < clips.Length; ++sound1 )
                {
                    for ( int sound2 = 0; sound2 < clips.Length; ++sound2 )
                    {
                        for ( int rr = 0; rr < reps; ++rr )
                        {
                            trials[ptr] = new Trial ( mass1, mass2, sound1, sound2 );
                            ++ptr;
                        }
                    }
                }
            }
        }

        // shuffle order for presentation
        for ( int ii = 0; ii < trials.Length; ++ii )
        {
            int rr = Random.Range(ii, trials.Length);
            (trials[rr], trials[ii]) = (trials[ii], trials[rr]);
        }

        Debug.Log($"[{string.Join(", ", trials)}]");

        // set initial configuration
        ConfigureTrial(trials[0]);

        // listen for button clicks
        green_button.onClick.AddListener(delegate {Respond(0);});
        red_button.onClick.AddListener(delegate {Respond(1);});

        canvas.SetActive(false);
        show_time = Time.time + 1.5f;
    }

    // log an experiment response
    void Respond ( int response )
    {
        responses[experiment] = response;

        Debug.Log($"response received: {response}");

        experiment += 1;
        if ( experiment >= trials.Length )
        {
            message.text = $"Experiment complete. Saved to '{outfile}'.";
            red_button.gameObject.SetActive(false);
            green_button.gameObject.SetActive(false);
            SaveResults();
        }
        else
        {
            ConfigureTrial(trials[experiment]);
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

    void ConfigureTrial ( Trial trial )
    {
        green_target.GetComponent<Rigidbody>().mass = trial.mass1;
        green_target.GetComponent<AudioSource>().clip = clips[trial.sound1];
        red_target.GetComponent<Rigidbody>().mass = trial.mass2;
        red_target.GetComponent<AudioSource>().clip = clips[trial.sound2];

        message.text = $"[{experiment + 1}/{trials.Length}] Which cylinder feels heavier?";
    }

    void SaveResults()
    {
        using (StreamWriter writer = new StreamWriter(outfile))
        {
            writer.WriteLine("Trial,Mass1,Mass2,Sound1,Sound2,Response");
            for ( int ii = 0; ii < trials.Length; ++ii )
            {
                writer.WriteLine($"{ii+1},{trials[ii].mass1},{trials[ii].mass2},{clips[trials[ii].sound1].name},{clips[trials[ii].sound2].name},{responses[ii]}");
            }
        }
    }
}
