using System;
using System.Drawing;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace AlumnoEjemplos.overflowDT
{
	public struct PositionScaleColor
	{
		public float X;
		public float Y;
		public float Z;
		public float Scale;
		public int Color;
				
		public static VertexFormats Format
		{
			get { return VertexFormats.Position | VertexFormats.Diffuse | VertexFormats.PointSize; }
		}
	};

	/// <summary>
	/// 
	/// </summary>
	public class Particle
	{
		public Particle()
		{
			m_pointSprite[0].X = 10.0f;
			m_pointSprite[0].Y = 10.0f;
			m_pointSprite[0].Z = 10.0f;
            m_pointSprite[0].Scale = 1.0f;
			m_pointSprite[0].Color = System.Drawing.Color.FromArgb(0xff, 0xff, 0xff, 0xff).ToArgb();
		}
						
		// Propiedades
		public PositionScaleColor[] m_pointSprite = new PositionScaleColor[1];
		public float m_fLiveTotalTime = 0;
		public float m_fLiveTimeLeft = 0;
		public Vector3 m_v3Speed;
	}
}
