using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Marker : MonoBehaviour
{
    public string type;
    public int column;
	public int row;
    public bool isAdvantage;
    
    public float startTime;
    public float deleteStartTime;
    public bool isPaused;

    public Material material;
    public Color materialColor;

	GameObject plane;

	FieldManager Field;

    readonly float TEXTURE_MARKER_ALPHA = 0.5f;

    #region Properties

    #endregion

	void Awake()
	{
		plane = (GameObject)Resources.Load("Prefab/Plane", typeof(GameObject));
		Field = GameObject.FindGameObjectWithTag("FieldManager").GetComponent<FieldManager>();

		column = 0;
		row = 0;
        isAdvantage = false;

        material = GetComponent<Renderer>().material;
	}

    public void Position(int x, int y)
	{
		this.column = x;
		this.row = y;

		var position = transform.position;
		position.x = x + 0.5f;
        position.y = 0.5001f;
		position.z = -y - 0.5f;

		transform.position = position;
	}

    public void SetType(string type)
    {
        if (this.type == type)
            return;

        switch (type)
        {
            case "advantage":
                materialColor = new Color(0, 1, 0, TEXTURE_MARKER_ALPHA);
                isAdvantage = true;
                break;
            case "gold":
                materialColor = new Color(255, 215, 0, TEXTURE_MARKER_ALPHA);
                isAdvantage = true;
                break;
            case "red":
                materialColor = new Color(1, 0, 0, TEXTURE_MARKER_ALPHA);
                break;
            case "normal":
                materialColor = new Color(0, 0, 1, TEXTURE_MARKER_ALPHA);
                isAdvantage = false;
                break;
        }

        this.type = type;
        material.color = materialColor;
    }

    public void Erase()
    {
        // material.color = new Color(1, 0, 0, 0.5f);
        Destroy(transform.gameObject, 1f);
    }

    public void Pause()
	{
		if(!isPaused)
		{

		}
		isPaused = false;
	}

	public void Resume(float pausedTime = 0)
	{
		if (!isPaused)
		{

		}
		isPaused = false;

		AddTime(pausedTime);
	}

	public void AddTime(float msec)
	{
		
	}
}
