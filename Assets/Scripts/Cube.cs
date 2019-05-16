using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using DG.Tweening;

public class Cube : MonoBehaviour
{
	public int column;
	public int row;
	public bool isRemoved;
	public int type;
	public string typeString;
	public bool isFall;

	public float rotationPeriod = 1f; // 隣に移動するのにかかる時間
	Vector3 scale; // 直方体のサイズ

	bool isRotate = false; // Cubeが回転中かどうかを検出するフラグ
	float directionX = 0; // 回転方向フラグ
	float directionZ = 0; // 回転方向フラグ

	float startAngleRad = 0; // 回転前の重心の水平面からの角度
	Vector3 startPos; // 回転前のCubeの位置
	float rotationTime = 0; // 回転中の時間経過
	float radius = 1; // 重心の軌道半径 (とりあえず仮で1)
	Quaternion fromRotation; // 回転前のCubeのクォータニオン
	Quaternion toRotation; // 回転後のCubeのクォータニオン

	public float rotationSpeed = 10f;
	public string rollDirection = "";

	[Header("CAN FLAGS")]
	public bool canRoll;
	public bool canChain;
	public bool canErase;

	public bool isRegular = false;

	public bool animationStarted;
	public bool riseStarted;
	public bool isRising;
	public bool isSpawned;

	public bool isRolled;
	public bool isReverse;

	public bool sinkStarted;
	public bool isSinking;

	public bool isPaused;

	public bool deleteStarted;

	#region # Const

	const float RISE_SPEED = 1f;  // 昇る速さの値 2.5f
	const float SINK_SPEED = 0.8f; // 沈む速さの値
	const float FALL_SPEED = 0.7f;

	#endregion

	FieldManager Field;
	PlayerController Player;
	Animator anim;

	void Awake()
	{
		Field = GameObject.FindGameObjectWithTag("FieldManager").GetComponent<FieldManager>();
		Player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();

		anim = GetComponent<Animator>();

		scale = transform.lossyScale;

		column = 0;
		row = 0;
		type = 0;
		typeString = "";
		deleteStarted = false;

		animationStarted = false;
		riseStarted = false;

		sinkStarted = false;
		isSinking = false;

		isPaused = false;

		isRemoved = false;
		isReverse = false;
		isFall = false;
	}

	void FixedUpdate()
	{

	}

	public void Position(int x, int y)
	{
		this.column = x;
		this.row = y;

		var position = transform.position;
		position.x = x + 0.5f;
		position.z = -y - 0.5f;

		transform.position = position;
	}

	void FixPosition()
	{
		Position(this.column, this.row);
	}

	public void Place()
	{
		var position = transform.position;
		position.y = 1f;
		transform.position = position;

		isRegular = true;
		isPaused = false;
		isRemoved = false;
		isReverse = false;
		isFall = false;
	}

	public void Cast()
	{
		var position = transform.position;
		position.y = -0.5f;
		transform.position = position;

		isRegular = false;
		isPaused = false;
		isRemoved = false;
		isReverse = false;
		isFall = false;
	}

	public void Remove()
	{
		// Field.RemoveCube(transform.gameObject);
		this.isRemoved = true;
		Delete();
	}

	void Delete()
	{
		Destroy(transform.gameObject);
	}

	public int GetType()
	{
		return type;
	}

	public string GetTypeString()
	{
		return typeString;
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

	public void ActivateRise()
	{
		transform.DOMoveY(1.5f, SINK_SPEED);
		transform.DOScaleY(0.5f, SINK_SPEED).OnComplete(() => Remove());

		// StartCoroutine("RiseAnimation");
	}
	
	IEnumerator RiseAnimation()
	{
		if (isRemoved || sinkStarted)
			yield break;

		Vector3 dicePosition = transform.position;
		Vector3 startPosition = transform.position;
		Vector3 targetPosition = new Vector3(startPosition.x, 1f, startPosition.z);

		riseStarted = true;
		isRising = true;

		canRoll = false;
		canChain = false;
		canErase = false;
		
		// for (float timer = 0f; timer < RISE_SPEED; timer += Time.deltaTime)
		// {
		// 	isSinking = true;
		// 	float progress = timer / RISE_SPEED;

		// 	transform.position = Vector3.Lerp(startPosition, new Vector3(transform.position.x, 1f, transform.position.z), progress);

		// 	yield return null;
		// }

		// float state = 0;
		// float speed = 1 / RISE_SPEED;

		// while (state < 1)
		// {
		// 	state += Time.deltaTime * speed;
		// 	transform.position = Vector3.Lerp(startPosition, new Vector3(transform.position.x, 1f, transform.position.z), state);
		// }

		while (transform.position.y < 1f)
		{
			transform.position += Vector3.up * (Time.deltaTime / RISE_SPEED);
			yield return null;
		}

		dicePosition.y = 1f;
		transform.position = dicePosition;
		
		riseStarted = false;
		isRising = false;
		isRegular = true;
	}

	IEnumerator SinkAnimation()
	{
		if (isRemoved || riseStarted)
			yield break;

		Vector3 dicePosition = transform.position;
		Vector3 startPosition = transform.position;
		Vector3 targetPosition = new Vector3(startPosition.x, -0.5f, startPosition.z);

		sinkStarted = true;
		isSinking = true;

		canRoll = false;
		canChain = false;
		canErase = false;

		Vector3 startScale = transform.localScale;
		Vector3 scale = transform.localScale;
		
		
		for (float timer = 0f; timer < SINK_SPEED; timer += Time.deltaTime)
		{
			isSinking = true;
			float progress = timer / SINK_SPEED;

			transform.localScale += Vector3.Lerp(Vector3.zero, new Vector3(startScale.x, 0, startScale.z), progress);

			yield return null;
		}
		
		/*
		Vector3 scale = transform.localScale;

		while (transform.localScale.y > 0)
		{
			//transform.localScale = new Vector3(scale.x, scale.y, scale.z);
			transform.localScale -= new Vector3(scale.x, scale.y - Time.deltaTime / SINK_SPEED, scale.z);

			// transform.position += Vector3.down * (Time.fixedDeltaTime / SINK_SPEED);
			yield return null;
		}*/

		dicePosition.y = -.501f;
		transform.position = dicePosition;

		// transform.localScale = scale;
		
		sinkStarted = false;
		isSinking = false;

		Remove();
	}

	public void ActivateSink()
	{
		GetComponent<Renderer>().material.color = new Color(1f, 1f, 1f, 0.7f);
		// anim.SetBool("isSinking", true);
		
		// StartCoroutine("SinkAnimation");
		transform.DOScaleY(0f, SINK_SPEED);
		transform.DOMoveY(0.5f, SINK_SPEED).OnComplete(() => Remove());
	}

	public bool SetDirection(string direction)
	{
		int oldX = this.column;
		int oldZ = this.row;

		int newX = oldX;
		int newZ = oldZ;

		switch (direction)
		{
			case "up":
				if (row == 0)
					return false;
				newZ--;
				break;
			case "down":
				if (row == (Field.gridArray.GetLength(0) - 1))
					return false;
				newZ++;
				break;
		}
		
		this.column = newX;
		this.row = newZ;

		transform.name = $"Cube ({newX}, {newZ})";

		return true;
	}

	#region 転がるアニメーション

	// TODO:
	IEnumerator RollAnimation(string direction)
	{
		if (isRising || animationStarted)
			yield break;

		direction = direction.ToLower();
		
		if (!SetDirection(direction))
		{
			isFall = true;
		}
		// else
		// {
			isRegular = false;
			animationStarted = true;
			rollDirection = direction;
		// }

		switch (rollDirection)
		{
			case "up":
				directionZ = -1;
				break;
			case "down":
				directionZ = 1;
				break;
		}

		startPos = transform.position;												// 回転前の座標を保持
		fromRotation = transform.rotation;											// 回転前のクォータニオンを保持
		transform.Rotate(-directionZ * 90, 0, -directionX * 90, Space.World);		// 回転方向に90度回転させる
		toRotation = transform.rotation;											// 回転後のクォータニオンを保持
		transform.rotation = fromRotation;											// CubeのRotationを回転前に戻す。（transformのシャローコピーとかできないんだろうか…。）
		setRadius();																// 回転半径を計算
		rotationTime = 0;															// 回転中の経過時間を0に。

		this.isRotate = true;														// 回転中フラグをたてる。
		
		while (isRotate)
		{
			float delta = Time.fixedDeltaTime * Player.MoveSpeed * 3 / 4;
			rotationTime += Time.fixedDeltaTime * Player.MoveSpeed * 3 / 4;
			
			float ratio = Mathf.Lerp(0, 1, rotationTime / rotationPeriod);			// 回転の時間に対する今の経過時間の割合

			// if (isReverse)
			// {
			// 	rotationTime -= Time.fixedDeltaTime * Player.MoveSpeed * 5 / 4;
				
			// 	if (ratio < 0f)
			// 		ratio = 0f;
				
			// 	rotationTime = - rotationTime;
			// 	delta = - delta;
				
			// 	// Debug.Log("isReverse");
			// }
			// else
			// {
				// rotationTime += Time.fixedDeltaTime * Player.MoveSpeed * 3 / 4;

				if (ratio > 1f)
					ratio = 1f;
			// }
			
			// 移動
			float thetaRad = Mathf.Lerp(0, Mathf.PI / 2f, ratio);					// 回転角をラジアンで。
			float distanceX = -directionX * radius * (Mathf.Cos (startAngleRad) - Mathf.Cos (startAngleRad + thetaRad));		// X軸の移動距離。 -の符号はキーと移動の向きを合わせるため。
			float distanceY = radius * (Mathf.Sin(startAngleRad + thetaRad) - Mathf.Sin (startAngleRad));						// Y軸の移動距離
			float distanceZ = -directionZ * radius * (Mathf.Cos (startAngleRad) - Mathf.Cos (startAngleRad + thetaRad));		// Z軸の移動距離
			transform.position = new Vector3(startPos.x + -distanceX, startPos.y + distanceY, startPos.z + distanceZ);			// 現在の位置をセット

			// 回転
			transform.rotation = Quaternion.Lerp(fromRotation, toRotation, ratio);		// Quaternion.Lerpで現在の回転角をセット（なんて便利な関数）

			if (ratio < 1)
			{
				yield return null;
			}
			else
			{
				transform.name = $"Qube ({column}, {row})";

				directionX = 0;
				directionZ = 0;
				rotationTime = 0;

				animationStarted = false;
				rollDirection = "";

				isRegular = true;
				isReverse = false;
				isRotate = false;
				isRolled = true;

				if (isFall)
				{
					Fall();
					yield break;
				}
				
				FixPosition();

				yield break;
			}
		}
	}

	public void Roll()
	{
		StartCoroutine(RollAnimation("DOWN"));
	}

	#endregion
	
	// 落ちるアニメーション
	public void Fall()
	{
		StartCoroutine(FallAnimation());
	}

	IEnumerator FallAnimation()
	{
		if (animationStarted)
			yield break;
		else
			animationStarted = true;
		
		Vector3 dicePosition = transform.position;
		Vector3 startPosition = transform.position;
		Vector3 targetPosition = new Vector3(startPosition.x, -10f, startPosition.z);

		canRoll = false;
		canChain = false;
		canErase = false;
		
		isRegular = false;
		
		for (float timer = 0f; timer < FALL_SPEED; timer += Time.deltaTime)
		{
			isSinking = true;
			float progress = timer / FALL_SPEED;

			transform.position = Vector3.Lerp(startPosition, targetPosition, progress);

			yield return null;
		}

		// while (transform.position.y < 1f)
		// {
		// 	transform.position += Vector3.up * (Time.fixedDeltaTime / RISE_SPEED);
		// 	yield return null;
		// }

		dicePosition.y = -10f;
		transform.position = dicePosition;

		animationStarted = false;

		Remove();
	}

	// 行カウントアニメーション
	IEnumerator SlideAnimation()
	{
		if (animationStarted)
			yield break;
		else
			animationStarted = true;
		
		Vector3 dicePosition = transform.position;
		Vector3 startPosition = transform.position;
		Vector3 targetPosition = new Vector3(startPosition.x, 1f, startPosition.z);

		riseStarted = true;
		isRising = true;

		canRoll = false;
		canChain = false;
		canErase = false;
		
		for (float timer = 0f; timer < RISE_SPEED; timer += Time.deltaTime)
		{
			isSinking = true;
			float progress = timer / RISE_SPEED;

			transform.position = Vector3.Lerp(startPosition, new Vector3(transform.position.x, 1f, transform.position.z), progress);

			yield return null;
		}

		// while (transform.position.y < 1f)
		// {
		// 	transform.position += Vector3.up * (Time.fixedDeltaTime / RISE_SPEED);
		// 	yield return null;
		// }

		dicePosition.y = 1f;
		transform.position = dicePosition;
		
		riseStarted = false;
		isRising = false;
		isRegular = true;

		animationStarted = false;
	}

	void setRadius() {

		Vector3 dirVec = new Vector3(0, 0, 0);			// 移動方向ベクトル
		Vector3 nomVec = Vector3.up;					// (0,1,0)

		// 移動方向をベクトルに変換
		if (directionX != 0) {							// X方向に移動
			dirVec = Vector3.right;						// (1,0,0)
		} else if (directionZ != 0) {					// Z方向に移動
			dirVec = Vector3.forward;					// (0,0,1)
		}

		// 移動方向ベクトルとObjectの向きの内積から移動方向のradiusとstartAngleを計算
		if (Mathf.Abs (Vector3.Dot (transform.right, dirVec)) > 0.99) {						// 移動方向がobjectのx方向
			if (Mathf.Abs (Vector3.Dot (transform.up, nomVec)) > 0.99) {					// globalのy軸がobjectのy方向
				radius = Mathf.Sqrt(Mathf.Pow(scale.x/2f,2f) + Mathf.Pow(scale.y/2f,2f));	// 回転半径
				startAngleRad = Mathf.Atan2(scale.y, scale.x);								// 回転前の重心の水平面からの角度
			} else if (Mathf.Abs (Vector3.Dot (transform.forward, nomVec)) > 0.99) {		// globalのy軸がobjectのz方向
				radius = Mathf.Sqrt(Mathf.Pow(scale.x/2f,2f) + Mathf.Pow(scale.z/2f,2f));
				startAngleRad = Mathf.Atan2(scale.z, scale.x);
			}

		} else if (Mathf.Abs (Vector3.Dot (transform.up, dirVec)) > 0.99) {					// 移動方向がobjectのy方向
			if (Mathf.Abs (Vector3.Dot (transform.right, nomVec)) > 0.99) {					// globalのy軸がobjectのx方向
				radius = Mathf.Sqrt(Mathf.Pow(scale.y/2f,2f) + Mathf.Pow(scale.x/2f,2f));
				startAngleRad = Mathf.Atan2(scale.x, scale.y);
			} else if (Mathf.Abs (Vector3.Dot (transform.forward, nomVec)) > 0.99) {		// globalのy軸がobjectのz方向
				radius = Mathf.Sqrt(Mathf.Pow(scale.y/2f,2f) + Mathf.Pow(scale.z/2f,2f));
				startAngleRad = Mathf.Atan2(scale.z, scale.y);
			}
		} else if (Mathf.Abs (Vector3.Dot (transform.forward, dirVec)) > 0.99) {			// 移動方向がobjectのz方向
			if (Mathf.Abs (Vector3.Dot (transform.right, nomVec)) > 0.99) {					// globalのy軸がobjectのx方向
				radius = Mathf.Sqrt(Mathf.Pow(scale.z/2f,2f) + Mathf.Pow(scale.x/2f,2f));
				startAngleRad = Mathf.Atan2(scale.x, scale.z);
			} else if (Mathf.Abs (Vector3.Dot (transform.up, nomVec)) > 0.99) {				// globalのy軸がobjectのy方向
				radius = Mathf.Sqrt(Mathf.Pow(scale.z/2f,2f) + Mathf.Pow(scale.y/2f,2f));
				startAngleRad = Mathf.Atan2(scale.y, scale.z);
			}
		}
		//Debug.Log (radius + ", " + startAngleRad);
	}
}
