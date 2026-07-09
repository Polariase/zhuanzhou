using UnityEngine;
using System;
using System.Collections.Generic;

public enum PaintTool { DIRECTION, SPEED, BLEND }

[RequireComponent(typeof(MeshFilter))]
public class FlowBehaviour : MonoBehaviour 
{
	public Mesh procedural;
	#if UNITY_EDITOR
	[HideInInspector] public bool IsBroken;

	[HideInInspector] public bool IsPaintEditing;
    [HideInInspector] public bool IsPreviewing = false;
	[HideInInspector] public PaintTool PaintTool = PaintTool.DIRECTION;
	[HideInInspector] public MeshCollider PaintCollider;
	[HideInInspector] public float PainterMagnitude = 1;
	[HideInInspector] public float PainterOpacity = 1;
    [HideInInspector] public AnimationCurve PainterCurve = AnimationCurve.EaseInOut(0,1,1,0);
	[HideInInspector] public float PainterSize = 1;

	[HideInInspector] public MeshFilter Filter;
	[HideInInspector] public FluidVertice[] Fluids;

	[HideInInspector] public bool PainterUsing = false;
	[HideInInspector] public Vector3 PainterDirection = Vector3.zero;
	[HideInInspector] public Vector3 PainterLast = Vector3.zero;
	[HideInInspector] public Queue<Vector3> AvgDirection = new Queue<Vector3>(); 
	#endif
}

[System.Serializable]
public struct FluidVertice
{
	public Vector3 world;
	public Vector3 fluidVector;
	public float magnitude;
	public float blending;
}