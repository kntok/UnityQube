using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class Grid : MonoBehaviour
{
	public int column;
	public int row;
	public int height;

	GameObject block;

	FieldManager Field;

	void Awake()
	{
		block = (GameObject)Resources.Load("Prefab/Block", typeof(GameObject));
		Field = GameObject.FindGameObjectWithTag("FieldManager").GetComponent<FieldManager>();

		column = 0;
		row = 0;
		height = 0;
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

	public void Erase()
	{
		if (heightBlocks.Count > 0)
			heightBlocks.Clear();
		
		Destroy(transform.gameObject);
	}

	#region 高さ
	
	Stack<GameObject> heightBlocks = new Stack<GameObject>();

	// 高さ設定
	public void SetHeight(int value)
	{
		height = value;
		
		for (var y = 1; y < value; y++)
		{
			var obj = Instantiate(this.block, Field.GetGridCenter2(column, y, row), Quaternion.identity);
			obj.transform.localScale = new Vector3(1f, 1f, 1f);
			obj.transform.SetParent(this.transform);

			heightBlocks.Push(obj.transform.gameObject);
		}
	}

	// 高くする
	public void AddHeight(int value)
	{
		value = value - 1;
		var obj = Instantiate(this.block, Field.GetGridCenter2(column, value, row), Quaternion.identity);
        obj.transform.localScale = new Vector3(1f, 1f, 1f);
		obj.transform.SetParent(this.transform);

		heightBlocks.Push(obj.transform.gameObject);
	}

	// 低くする
	public void CutHeight(int value = 1)
	{	
		if (heightBlocks.Count > 0)
		{
			// var block = heightBlocks.Peek();
			Destroy(heightBlocks.Pop());
			// Destroy(block);
		}
		else
			return;
	}

	#endregion
}
