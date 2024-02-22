using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ExperimentManager : MonoBehaviour
{
    [SerializeField] private int reps = 5;
    [SerializeField] private float lo = 1f;
    [SerializeField] private float hi = 4f;
    [SerializeField] private int levels = 6;
    [SerializeField] private string outfile = "results.csv";

    [SerializeField] private Rigidbody target;
    [SerializeField] private TMP_Text message;
    [SerializeField] private Button heavy_button;
    [SerializeField] private Button light_button;
    [SerializeField] private GameObject canvas;

    private float[] masses;
    private int[] responses;
    private int experiment = 0;

    private float show_time = float.MaxValue;

    // Start is called before the first frame update
    void Start()
    {
        // allocate arrays to match fields
        masses = new float[reps * levels];
        responses = new int[reps * levels];

        float step = (hi - lo) / (levels - 1);

        // set up reps of levels
        for ( int ii = 0; ii < levels; ++ii )
        {
            float val = lo + ii * step;

            for ( int jj = 0; jj < reps; ++jj )
            {
                masses[ii * reps + jj] = val;
            }
        }

        // shuffle order for presentation
        for ( int ii = 0; ii < reps * levels; ++ii )
        {
            int rr = Random.Range(ii, reps * levels);
            (masses[rr], masses[ii]) = (masses[ii], masses[rr]);
        }

        //Debug.Log($"[{string.Join(", ", masses)}]");

        // set initial mass value
        target.mass = masses[0];

        // listen for button clicks
        light_button.onClick.AddListener(delegate {Respond(0);});
        heavy_button.onClick.AddListener(delegate {Respond(1);});

        canvas.SetActive(false);
        message.text = $"[{experiment + 1}/{reps * levels}] Does the cylinder feel light or heavy?";
        show_time = Time.time + 1.5f;
    }

    // log an experiment response
    void Respond ( int response )
    {
        responses[experiment] = response;

        Debug.Log($"response received: {response}");

        experiment += 1;
        if ( experiment >= reps * levels )
        {
            message.text = $"Experiment complete. Saved to '{outfile}'.";
            heavy_button.gameObject.SetActive(false);
            light_button.gameObject.SetActive(false);
            SaveResults();
        }
        else
        {
            target.mass = masses[experiment];
            message.text = $"[{experiment + 1}/{reps * levels}] Does the cylinder feel light or heavy?";
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
        using (StreamWriter writer = new StreamWriter(outfile))
        {
            writer.WriteLine("Trial,Mass,Response");
            for ( int ii = 0; ii < reps * levels; ++ii )
            {
                writer.WriteLine($"{ii+1},{masses[ii]},{responses[ii]}");
            }
        }
    }
}
