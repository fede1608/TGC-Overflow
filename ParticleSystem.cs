using System;
using System.Drawing;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace AlumnoEjemplos.overflowDT
{
	

	/// <summary>
	/// 
	/// </summary>
	public class ParticleSystem
	{
		public ParticleSystem()

		{
            
		}

		// Creo el sistema de partículas utilizando el bitmap pasado como parámetro
		public bool Load(string strBitmapFilename, int iParticleCount, Renderer rnd,float posX, float posY, float posZ)
		{
			// Asigno valores a propiedades según parámetro
			m_renderer = rnd;
            m_renderer.m_device.RenderState.AlphaBlendEnable = true;
			m_iParticleCount = iParticleCount;

			// Creo las partículas
			m_particles = new Particle[iParticleCount];
			
			m_deathParticleList = new ParticleStack(iParticleCount);
			m_aliveParticleList = new ParticleQueue(iParticleCount);

			// Creo partículas sólo para la lista de partículas muertas
			for (int i=0; i<iParticleCount; i++)
			{
				m_particles[i] = new Particle();
				m_deathParticleList.Insert(m_particles[i]);
			}

			// Cargo la textura que representará cada partícula
			m_iTexNum = m_renderer.LoadTexture(strBitmapFilename);

            m_v3Pos.X = posX;
            m_v3Pos.Y = posY;
            m_v3Pos.Z = posZ;

			if (m_iTexNum == -1)
				return false;
			else
				return true;
		}

		public bool ChangeTexture(string strBitmapFilename)
		{
			// Cargo la textura que representará cada partícula
			// (Notar que cargo otra textura sin descargar la anterior,
			//  me tomo esta licencia sólo por ser una demostración)
			m_iTexNum = m_renderer.LoadTexture(strBitmapFilename);

			if (m_iTexNum == -1)
				return false;
			else
				return true;
		}

		public void Frame(float fTimeBetweenFrames)
		{
            m_fTimeAccum += fTimeBetweenFrames;

			// ¿Debo crear una nueva partícula?
			if (m_fTimeAccum >= m_fCreationParticleFreq)
			{
				m_fTimeAccum = 0.0f;
				
				CreateParticle();
			}

			if (m_aliveParticleList == null)
				return;

			// Habilito el dibujado de point sprites
			m_renderer.m_device.RenderState.PointSpriteEnable = true;
			// Habilito la escala por partícula
			m_renderer.m_device.RenderState.PointScaleEnable = true;
			m_renderer.m_device.RenderState.PointScaleA = 0.001f;
			m_renderer.m_device.RenderState.PointScaleB = 0.001f;
			m_renderer.m_device.RenderState.PointScaleC = 0.01f;

			
			m_renderer.BindTexture(m_iTexNum);

			Particle p = m_aliveParticleList.GetFirst();
			while (p != null)
			{
				p.m_fLiveTimeLeft -= fTimeBetweenFrames;

				if (p.m_fLiveTimeLeft <= 0)
				{

					// Extraigo la partícula de la lista de partículas vivas
					m_aliveParticleList.Extract(out p);

                    // Inserto la partícula en la lista de partículas muertas
					m_deathParticleList.Insert(p);
                    
				}
				else
				{
					// Dibujo la partícula
					Draw(fTimeBetweenFrames, p);
			
				}

				p = m_aliveParticleList.GetNext();
			}
			m_renderer.BindTexture(-1);

		}

		private void Draw(float fTimeBetweenFrames, Particle p)
		{

            int val;
			float fInterpolParam;
			Vector3 vel;
			
			float fT = fTimeBetweenFrames / 50.0f;
			if (m_bUseGravity)
			{
				vel.X = p.m_v3Speed.X + m_Gravity.X * fT;
				vel.Y = p.m_v3Speed.Y + m_Gravity.Y * fT;
				vel.Z = p.m_v3Speed.Z + m_Gravity.Z * fT;
			}
			else
			{
				vel.X = p.m_v3Speed.X;
				vel.Y = p.m_v3Speed.Y;
				vel.Z = p.m_v3Speed.Z;
			}
									
			p.m_pointSprite[0].X += ((p.m_v3Speed.X + vel.X) * fT) / 2;
			p.m_pointSprite[0].Y += ((p.m_v3Speed.Y + vel.Y) * fT) / 2;
			p.m_pointSprite[0].Z += ((p.m_v3Speed.Z + vel.Z) * fT) / 2;

			p.m_v3Speed = vel;
				
			// Calculo parámetro de interpolación
			// (para hacer variar el color de la partícula a medida
			// que va llegando al final de su vida)
			fInterpolParam = 1.0f - (p.m_fLiveTimeLeft/p.m_fLiveTotalTime);
			// Utilizo el parámetro de interpolación para hallar
			// un número entre 0 y 255
			val = System.Convert.ToInt32((p.m_fLiveTimeLeft * 255.0f) / p.m_fLiveTotalTime);

			// Realizo la interpolación lineal
			int iRedComp = System.Convert.ToInt32(m_iFromColor.R + ((m_iToColor.R - m_iFromColor.R) * fInterpolParam));
			int iGreenComp = System.Convert.ToInt32(m_iFromColor.G + ((m_iToColor.G - m_iFromColor.G) * fInterpolParam));
			int iBlueComp = System.Convert.ToInt32(m_iFromColor.B + ((m_iToColor.B - m_iFromColor.B) * fInterpolParam));

			// Utilizo los componentes calculados
			p.m_pointSprite[0].Color = System.Drawing.Color.FromArgb(0xff, iRedComp, iGreenComp, iBlueComp).ToArgb();
					

			// Modulo el canal alpha de la partícula
			// para que se vaya haciendo transparente
			m_renderer.m_device.RenderState.TextureFactor = p.m_pointSprite[0].Color;
			m_renderer.m_device.TextureState[0].AlphaOperation = TextureOperation.Modulate;
			m_renderer.m_device.TextureState[0].AlphaArgument1 = TextureArgument.TextureColor;
			m_renderer.m_device.TextureState[0].AlphaArgument2 = TextureArgument.TFactor;
	
			// Dibujo efectivamente la partícula
			m_renderer.ProcsVerts(p.m_pointSprite, 1, PrimitiveType.PointList);
		}
		public void CreateParticle()
		{
			Particle p = null;

			// Tomo una partícula de la lista de partículas muertas
			if (m_deathParticleList.Extract(out p))
			{
				// Agrego dicha partícula a la lista
				// de partículas vivas
				m_aliveParticleList.Insert(p);


				// Fijo valores iniciales a la partícula
				p.m_fLiveTotalTime = m_fParticleLiveTime;
				p.m_fLiveTimeLeft = m_fParticleLiveTime;
				p.m_pointSprite[0].X = m_v3Pos.X;
				p.m_pointSprite[0].Y = m_v3Pos.Y;
				p.m_pointSprite[0].Z = m_v3Pos.Z;
				p.m_pointSprite[0].Color = System.Drawing.Color.FromArgb(0xff, 0x20, 0x25, 0x00).ToArgb();

				float fNum;
				
				// Según la dispersión asigno una velocidad inicial
				// (notar que si la disp. es 0, la partícula posee una vel = 0, 1, 0
				fNum = m_randGen.Next(m_iDispersion) / 1000.0f;
				fNum *= (fNum*1000%2 == 0 ? 1.0f : -1.0f);
				p.m_v3Speed.X = fNum * 2.0f;
				fNum = 1.0f - (2.0f * m_randGen.Next(m_iDispersion) / 1000.0f);
				p.m_v3Speed.Y = fNum * 2.0f;
				fNum = m_randGen.Next(m_iDispersion) / 1000.0f;
				fNum *= (fNum*1000%2 == 0 ? 1.0f : -1.0f);
				p.m_v3Speed.Z = fNum * 2.0f;

				// Modifico la escala de manera randómica
				fNum = m_randGen.Next(100) / 10000.0f;
				p.m_pointSprite[0].Scale = m_fScale + fNum;

			}
			else
			{
				// La lista de partículas muertas SI está vacía
				// NO creo la partícula
				// nota: Acá se podría tomar la decisión de tomar la
				// partícula viva más vieja para matarla prematuramente
			}
			
		}
		
		// Cantidad máxima de partículas
		private int m_iParticleCount;
		// Referencia al renderer
		public Renderer m_renderer = null;
		// Identificador de la textura
		private int m_iTexNum = -1;

		// Partículas
		Particle[] m_particles = null;
		ParticleStack m_deathParticleList = null;
		ParticleQueue m_aliveParticleList = null;

		// Posición del generador
		// (Esto se podría cambiar para que el generador no sea puntual)
		public Vector3 m_v3Pos;

		// Si siempre especifico el mismo valor semilla
		// la serie numérica generada será siempre la misma
		// pero en este caso particular no es importante
		System.Random m_randGen = new System.Random(0);
		float m_fTimeAccum = 0.0f;

		// Frecuencia de creación de partículas
		// (mientras mas baja mayor cantidad de parts. por unidad de tiempo)
		public float m_fCreationParticleFreq = 5f;
		
		// Color "desde" de la partícula
		public System.Drawing.Color m_iFromColor = System.Drawing.Color.FromArgb(0x00,0xff, 0xd7, 0x00);
		// Color "hasta" de la partícula
        public System.Drawing.Color m_iToColor = System.Drawing.Color.FromArgb( 0x00,0xff,0x45, 0x00 );

		// Escala por defecto de la partícula
		public float m_fScale = 0.01f;
        public Mesh m_mesh;
		// Gravedad por defecto
		public Vector3 m_Gravity = new Vector3(0.0f, +0.6f, 0f);
		// Tiempo de vida por defecto de la partícula
		public float m_fParticleLiveTime = 800f;

		// Indica si se usa o no la gravedad
		public bool m_bUseGravity = true;
		// Dispersión (ruido introducido a la velocidad vectorial
		// inicial de la partícula)
		public int m_iDispersion = 500;

	}
}
