using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System;

public class cameraMotion : MonoBehaviour {

    public Transform Target;
    public Vector3 Offset;
    public float Radius = 300.0f;

    private GUIStyle blackStyle = new GUIStyle();
    private GUIStyle whiteStyle = new GUIStyle();

    public static Stopwatch watch = new Stopwatch();

    struct Delta
    {
        public float Value;
        //Stopwatch.tim
        TimeSpan captured;

        public Delta(float value)
        {
            Value = value;
            captured = watch.Elapsed;
        }

        public bool Expired
        {
            get
            {
                return (watch.Elapsed - captured).Milliseconds > 500;
            }
        }
    }

    Queue<Delta> deltas = new Queue<Delta>();

    int fps = 0;

    float angleX = 0.0f,
            angleY = 0.0f;
	// Use this for initialization

    TimeSpan lastFrame;// = watch.Elapsed;

	void Start () {
        watch.Start();
        lastFrame = watch.Elapsed;
        blackStyle.normal.textColor = Color.black;
        whiteStyle.normal.textColor = Color.white;
    }


//    int renderRevents = 

    void OnPostRender()
    {
        TimeSpan now = watch.Elapsed;
        TimeSpan delta = now - lastFrame;
        lastFrame = now;
        float d = (float)delta.TotalSeconds;
        //UnityEngine.Debug.Log(now+" "+delta);
        deltas.Enqueue(new Delta(d));
        while (deltas.Peek().Expired)
            deltas.Dequeue();

    }

    void OnGUI()
    {
        string text = "Unless specified otherwise:\n Arrow-Keys: rotate\n C/Space: Zoom\n Escape: Quit\n\n FPS: "+fps;
        GUI.Label(new UnityEngine.Rect(11, 11, 300, 100), text ,blackStyle);
        GUI.Label(new UnityEngine.Rect(10, 10, 300, 100), text, whiteStyle);
    }

	// Update is called once per frame
    void Update()
    {

        if (Input.GetKey("escape"))
        {
            Application.Quit();
        }


        {
            float sum = 0.0f;
            foreach (var d in deltas)
                sum += d.Value;

            if (deltas.Count > 0)
                fps = Mathf.RoundToInt(1.0f / (sum / deltas.Count));

            //fps = Time.captureFramerate;
        }

        float angleSpeed = 1.0f;
        float radialSpeed = 1.3f;
        angleX += Time.deltaTime * angleSpeed * Input.GetAxis("Horizontal");
        if (Input.GetKey(KeyCode.Mouse0))
            angleX += Time.deltaTime * angleSpeed * Input.GetAxis("Mouse X");

        angleY -= Time.deltaTime * angleSpeed * Input.GetAxis("Vertical");
        Radius *= Mathf.Pow(radialSpeed, Time.deltaTime * Input.GetAxis("Jump"));


        float cs = Mathf.Cos(angleY);
        float sn = Mathf.Sin(angleY);
        transform.position = Target.position + Offset + new Vector3(Mathf.Sin(angleX) * cs, sn, Mathf.Cos(angleX) * cs) * Radius;
        transform.LookAt(Target.position + Offset);

    }
}
