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

public class ExperimentManager10 : MonoBehaviour
{
    [SerializeField] private int reps = 1;
    [SerializeField] private float lo = 1f;
    [SerializeField] private float hi = 3f;
    [SerializeField] private int levels = 3;
    [SerializeField] private int max_trials = 0;
    [SerializeField] private string outfile = "results.csv";

    [SerializeField] private GameObject red_target;
    [SerializeField] private GameObject blue_target;

    [SerializeField] private TMP_Text message;
    [SerializeField] private Button red_button;
    [SerializeField] private Button blue_button;
    [SerializeField] private GameObject canvas;

    [SerializeField] private AudioClip[] clips;

    private Trial[] trials;
    private int[] responses;
    private int experiment = 0;

    private float show_time = float.MaxValue;

    private Vector3 red_position;
    private Vector3 blue_position;
    private Quaternion red_rotation;
    private Quaternion blue_rotation;

    // Start is called before the first frame update
    void Start()
    {
        // stash starting positions so we can revert to them later
        red_position = red_target.transform.position;
        red_rotation = red_target.transform.rotation;
        blue_position = blue_target.transform.position;
        blue_rotation = blue_target.transform.rotation;
        
        int num_trials = reps * levels * levels * clips.Length * clips.Length;
        
        if ( max_trials > 0 && max_trials < num_trials )
        {
            num_trials = max_trials;
        }

        // allocate arrays to match fields
        trials = new Trial[reps * levels * levels * clips.Length * clips.Length];
        responses = new int[num_trials];

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

        Debug.Log($"{trials.Length} variations, using {responses.Length} of them");

        // set initial configuration
        ConfigureTrial(trials[0]);

        // listen for button clicks
        blue_button.onClick.AddListener(delegate {Respond(0);});
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
        if ( experiment >= responses.Length )
        {
            message.text = $"Experiment complete. Saved to '{outfile}'.";
            red_button.gameObject.SetActive(false);
            blue_button.gameObject.SetActive(false);
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
        blue_target.GetComponent<Rigidbody>().mass = trial.mass1;
        blue_target.GetComponent<AudioSource>().clip = clips[trial.sound1];
        red_target.GetComponent<Rigidbody>().mass = trial.mass2;
        red_target.GetComponent<AudioSource>().clip = clips[trial.sound2];

        red_target.transform.position = red_position;
        red_target.transform.rotation = red_rotation;
        blue_target.transform.position = blue_position;
        blue_target.transform.rotation = blue_rotation;

        message.text = $"[{experiment + 1}/{responses.Length}] Which cylinder feels heavier?";
    }

    void SaveResults()
    {
        using (StreamWriter writer = new StreamWriter(outfile))
        {
            writer.WriteLine("Trial,Mass1,Mass2,Sound1,Sound2,Response");
            for ( int ii = 0; ii < responses.Length; ++ii )
            {
                writer.WriteLine($"{ii+1},{trials[ii].mass1},{trials[ii].mass2},{clips[trials[ii].sound1].name},{clips[trials[ii].sound2].name},{responses[ii]}");
            }
        }
    }
}
