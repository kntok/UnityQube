using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    Camera _camera = null;

    [SerializeField] Transform target1 = null, target2 = null;
	[SerializeField] Vector3 offset;

    private float screenAspect = 0; 

    FieldManager Field;
    PlayerController Player;

    [SerializeField] public GameObject targetPlayer;

    void Awake()
    {
        _camera = GetComponent<Camera>();

        screenAspect = (float)Screen.height / Screen.width;
        
        Field = GameObject.FindGameObjectWithTag("FieldManager").GetComponent<FieldManager>();
		Player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
    }

    // Start is called before the first frame update
    void Start()
    {
        // targetPlayer = Player.GetPosition();
		Vector3 offset2 = new Vector3(0, -4.5f, 4f);
        // this.offset = transform.position - targetPlayer.transform.position;
		this.offset = offset2;
    }

    void Update () 
	{
		UpdateCameraPosition();
		// UpdateOrthographicSize();
	}

	// void UpdateCameraPosition()
	// {
	// 	// 2点間の中心点からカメラの位置を更新
	// 	Vector3 center = Vector3.Lerp (target1.position, target2.position, 0.5f);
	// 	transform.position = center + Vector3.forward * -10;
	// }

    void UpdateCameraPosition()
	{
		this.transform.position = targetPlayer.transform.position - this.offset;
	}

	void UpdateOrthographicSize()
	{
		// ２点間のベクトルを取得
		Vector3 targetsVector = AbsPositionDiff (target1, target2) + (Vector3)offset;

		// アスペクト比が縦長ならyの半分、横長ならxとアスペクト比でカメラのサイズを更新
		float targetsAspect = targetsVector.y / targetsVector.x;
		float targetOrthographicSize = 0;
		if ( screenAspect < targetsAspect) {
			targetOrthographicSize = targetsVector.y * 0.5f;
		} else {
			targetOrthographicSize = targetsVector.x * (1/_camera.aspect) * 0.5f;
		}
		_camera.orthographicSize =  targetOrthographicSize;
	}

	Vector3 AbsPositionDiff(Transform target1, Transform target2)
	{
		Vector3 targetsDiff = target1.position - target2.position;
		return new Vector3(Mathf.Abs(targetsDiff.x), Mathf.Abs(targetsDiff.y));
    }
}
