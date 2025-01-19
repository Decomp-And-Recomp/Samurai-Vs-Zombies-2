using System;
using System.Collections.Generic;
using UnityEngine;

public class MeleeWeaponTrail : MonoBehaviour
{
	[Serializable]
	public class Point
	{
		public float timeCreated;

		public Vector3 basePosition;

		public Vector3 tipPosition;
	}

	[SerializeField]
	private bool _emit = true;

	private bool _use = true;

	[SerializeField]
	private float _emitTime;

	[SerializeField]
	private Material _material;

	[SerializeField]
	private float _lifeTime = 1f;

	[SerializeField]
	private Color[] _colors;

	[SerializeField]
	private float[] _sizes;

	[SerializeField]
	private float _minVertexDistance = 0.1f;

	[SerializeField]
	private float _maxVertexDistance = 10f;

	private float _minVertexDistanceSqr;

	private float _maxVertexDistanceSqr;

	[SerializeField]
	private float _maxAngle = 3f;

	[SerializeField]
	private bool _autoDestruct;

	[SerializeField]
	private int subdivisions = 4;

	[SerializeField]
	private Transform _base;

	[SerializeField]
	private Transform _tip;

	private List<Point> _points = new List<Point>();

	private List<Point> _smoothedPoints = new List<Point>();

	private GameObject _trailObject;

	private Mesh _trailMesh;

	private Vector3 _lastPosition;

	public bool Emit
	{
		set
		{
			_emit = value;
		}
	}

	public bool Use
	{
		set
		{
			_use = value;
		}
	}

	private void Start()
	{
		if (_trailObject == null)
		{
			Init();
		}
	}

	private void Init()
	{
		if (_base == null)
		{
			UnityEngine.Object.Destroy(this);
		}
		_lastPosition = base.transform.position;
		_trailObject = new GameObject("Trail_" + base.name);
		_trailObject.transform.parent = null;
		_trailObject.transform.position = Vector3.zero;
		_trailObject.transform.rotation = Quaternion.identity;
		_trailObject.transform.localScale = Vector3.one;
		_trailObject.AddComponent(typeof(MeshFilter));
		_trailObject.AddComponent(typeof(MeshRenderer));
		_trailObject.GetComponent<Renderer>().material = _material;
		_trailMesh = new Mesh();
		_trailMesh.name = base.name + "TrailMesh";
		_trailObject.GetComponent<MeshFilter>().mesh = _trailMesh;
		_minVertexDistanceSqr = _minVertexDistance * _minVertexDistance;
		_maxVertexDistanceSqr = _maxVertexDistance * _maxVertexDistance;
	}

	private void OnEnable()
	{
		if (_trailObject == null)
		{
			Init();
		}
	}

	private void OnDisable()
	{
		UnityEngine.Object.Destroy(_trailObject);
	}

	private void Update()
	{
		if (!_use)
		{
			return;
		}
		if (_emit && _emitTime != 0f)
		{
			_emitTime -= Time.deltaTime;
			if (_emitTime == 0f)
			{
				_emitTime = -1f;
			}
			if (_emitTime < 0f)
			{
				_emit = false;
			}
		}
		if (!_emit && _points.Count == 0 && _autoDestruct)
		{
			UnityEngine.Object.Destroy(_trailObject);
			UnityEngine.Object.Destroy(base.gameObject);
		}
		if (!Camera.main)
		{
			return;
		}
		float sqrMagnitude = (_lastPosition - base.transform.position).sqrMagnitude;
		if (_emit)
		{
			if (sqrMagnitude > _minVertexDistanceSqr)
			{
				bool flag = false;
				if (_points.Count < 3)
				{
					flag = true;
				}
				else
				{
					Vector3 from = _points[_points.Count - 2].tipPosition - _points[_points.Count - 3].tipPosition;
					Vector3 to = _points[_points.Count - 1].tipPosition - _points[_points.Count - 2].tipPosition;
					if (Vector3.Angle(from, to) > _maxAngle || sqrMagnitude > _maxVertexDistanceSqr)
					{
						flag = true;
					}
				}
				if (flag)
				{
					Point point = new Point();
					point.basePosition = _base.position;
					point.tipPosition = _tip.position;
					point.timeCreated = Time.time;
					_points.Add(point);
					_lastPosition = base.transform.position;
					if (_points.Count == 1)
					{
						_smoothedPoints.Add(point);
					}
					else if (_points.Count > 1)
					{
						for (int i = 0; i < 1 + subdivisions; i++)
						{
							_smoothedPoints.Add(point);
						}
					}
					if (_points.Count >= 4)
					{
						IEnumerable<Vector3> collection = Interpolate.NewCatmullRom(new Vector3[4]
						{
							_points[_points.Count - 4].tipPosition,
							_points[_points.Count - 3].tipPosition,
							_points[_points.Count - 2].tipPosition,
							_points[_points.Count - 1].tipPosition
						}, subdivisions, false);
						IEnumerable<Vector3> collection2 = Interpolate.NewCatmullRom(new Vector3[4]
						{
							_points[_points.Count - 4].basePosition,
							_points[_points.Count - 3].basePosition,
							_points[_points.Count - 2].basePosition,
							_points[_points.Count - 1].basePosition
						}, subdivisions, false);
						List<Vector3> list = new List<Vector3>(collection);
						List<Vector3> list2 = new List<Vector3>(collection2);
						float timeCreated = _points[_points.Count - 4].timeCreated;
						float timeCreated2 = _points[_points.Count - 1].timeCreated;
						for (int j = 0; j < list.Count; j++)
						{
							int num = _smoothedPoints.Count - (list.Count - j);
							if (num > -1 && num < _smoothedPoints.Count)
							{
								Point point2 = new Point();
								point2.basePosition = list2[j];
								point2.tipPosition = list[j];
								point2.timeCreated = Mathf.Lerp(timeCreated, timeCreated2, (float)j / (float)list.Count);
								_smoothedPoints[num] = point2;
							}
						}
					}
				}
				else
				{
					_points[_points.Count - 1].basePosition = _base.position;
					_points[_points.Count - 1].tipPosition = _tip.position;
					_smoothedPoints[_smoothedPoints.Count - 1].basePosition = _base.position;
					_smoothedPoints[_smoothedPoints.Count - 1].tipPosition = _tip.position;
				}
			}
			else
			{
				if (_points.Count > 0)
				{
					_points[_points.Count - 1].basePosition = _base.position;
					_points[_points.Count - 1].tipPosition = _tip.position;
				}
				if (_smoothedPoints.Count > 0)
				{
					_smoothedPoints[_smoothedPoints.Count - 1].basePosition = _base.position;
					_smoothedPoints[_smoothedPoints.Count - 1].tipPosition = _tip.position;
				}
			}
		}
		RemoveOldPoints(_points);
		if (_points.Count == 0)
		{
			_trailMesh.Clear();
		}
		RemoveOldPoints(_smoothedPoints);
		if (_smoothedPoints.Count == 0)
		{
			_trailMesh.Clear();
		}
		List<Point> smoothedPoints = _smoothedPoints;
		if (smoothedPoints.Count <= 1)
		{
			return;
		}
		Vector3[] array = new Vector3[smoothedPoints.Count * 2];
		Vector2[] array2 = new Vector2[smoothedPoints.Count * 2];
		int[] array3 = new int[(smoothedPoints.Count - 1) * 6];
		Color[] array4 = new Color[smoothedPoints.Count * 2];
		for (int k = 0; k < smoothedPoints.Count; k++)
		{
			Point point3 = smoothedPoints[k];
			float num2 = (Time.time - point3.timeCreated) / _lifeTime;
			Color color = Color.Lerp(Color.white, Color.clear, num2);
			if (_colors != null && _colors.Length > 0)
			{
				float num3 = num2 * (float)(_colors.Length - 1);
				float num4 = Mathf.Floor(num3);
				float num5 = Mathf.Clamp(Mathf.Ceil(num3), 1f, _colors.Length - 1);
				float t = Mathf.InverseLerp(num4, num5, num3);
				if (num4 >= (float)_colors.Length)
				{
					num4 = _colors.Length - 1;
				}
				if (num4 < 0f)
				{
					num4 = 0f;
				}
				if (num5 >= (float)_colors.Length)
				{
					num5 = _colors.Length - 1;
				}
				if (num5 < 0f)
				{
					num5 = 0f;
				}
				color = Color.Lerp(_colors[(int)num4], _colors[(int)num5], t);
			}
			float num6 = 0f;
			if (_sizes != null && _sizes.Length > 0)
			{
				float num7 = num2 * (float)(_sizes.Length - 1);
				float num8 = Mathf.Floor(num7);
				float num9 = Mathf.Clamp(Mathf.Ceil(num7), 1f, _sizes.Length - 1);
				float t2 = Mathf.InverseLerp(num8, num9, num7);
				if (num8 >= (float)_sizes.Length)
				{
					num8 = _sizes.Length - 1;
				}
				if (num8 < 0f)
				{
					num8 = 0f;
				}
				if (num9 >= (float)_sizes.Length)
				{
					num9 = _sizes.Length - 1;
				}
				if (num9 < 0f)
				{
					num9 = 0f;
				}
				num6 = Mathf.Lerp(_sizes[(int)num8], _sizes[(int)num9], t2);
			}
			Vector3 vector = point3.tipPosition - point3.basePosition;
			array[k * 2] = point3.basePosition - vector * (num6 * 0.5f);
			array[k * 2 + 1] = point3.tipPosition + vector * (num6 * 0.5f);
			array4[k * 2] = (array4[k * 2 + 1] = color);
			float x = (float)k / (float)smoothedPoints.Count;
			array2[k * 2] = new Vector2(x, 0f);
			array2[k * 2 + 1] = new Vector2(x, 1f);
			if (k > 0)
			{
				array3[(k - 1) * 6] = k * 2 - 2;
				array3[(k - 1) * 6 + 1] = k * 2 - 1;
				array3[(k - 1) * 6 + 2] = k * 2;
				array3[(k - 1) * 6 + 3] = k * 2 + 1;
				array3[(k - 1) * 6 + 4] = k * 2;
				array3[(k - 1) * 6 + 5] = k * 2 - 1;
			}
		}
		_trailMesh.Clear();
		_trailMesh.vertices = array;
		_trailMesh.colors = array4;
		_trailMesh.uv = array2;
		_trailMesh.triangles = array3;
	}

	private void RemoveOldPoints(List<Point> pointList)
	{
		List<Point> list = new List<Point>();
		foreach (Point point in pointList)
		{
			if (Time.time - point.timeCreated > _lifeTime)
			{
				list.Add(point);
			}
		}
		foreach (Point item in list)
		{
			pointList.Remove(item);
		}
	}
}
