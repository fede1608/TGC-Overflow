using System;
using System.Windows.Forms;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

using System.Collections.Generic;
using System.Text;
using TgcViewer.Example;
using TgcViewer;
using System.Drawing;
using TgcViewer.Utils.Modifiers;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSkeletalAnimation;
using TgcViewer.Utils.Terrain;

namespace AlumnoEjemplos.overflowDT
{
	public enum eMatrixMode
	{
		MM_WORLD,
		MM_VIEW,
		MM_PROJECTION
	};

	/// <summary>
	/// Clase Renderer
	/// </summary>
	public class Renderer
	{	
		// ---------------------------------------------------------------
		// Propiedades
		// ---------------------------------------------------------------
		// Dispositivo Direct3D
		public Device m_device;
		// Matriz activa
		private eMatrixMode m_matrixMode;
		// Matriz
		private Matrix m_theMatrix;
		// VertexBuffer (por el momento el único)
		private VertexBuffer m_vertexBuffer = null;
		// IndexBuffer
		private IndexBuffer m_indexBuffer = null;

		// Array de texturas
		private const int m_ciMaxTex = 256;
		private int m_iTexNum = 0;
		private Texture[] m_aTexs = new Texture[m_ciMaxTex];

		// Array de materiales
		private const int MAX_MATERIAL_COUNT = 256;
		private Material[] m_aMats = new Material[MAX_MATERIAL_COUNT];

		private int m_iClearColor = System.Drawing.Color.Black.ToArgb();

		static private Renderer m_mainRenderer;

		

		// ---------------------------------------------------------------
		// Constructor
		// ---------------------------------------------------------------
		public Renderer()
		{
            m_device = GuiController.Instance.D3dDevice;
			m_mainRenderer = this;

			for (int i=0; i<m_aMats.Length; i++)
				m_aMats[i].SpecularSharpness = -1.0f;
		}

		public static Renderer GetInstance()
		{
			return Renderer.m_mainRenderer;
		}

		// Inicializa el renderizador
        public bool Init(System.Windows.Forms.Control devWin)
		{
			PresentParameters d3dPresentParams = new PresentParameters();
			d3dPresentParams.Windowed = true;					// Modo ventana
			d3dPresentParams.SwapEffect = SwapEffect.Discard;	// Se descarta el frontbuffer
			
			d3dPresentParams.EnableAutoDepthStencil = true; // Activo el ZBuffer
			d3dPresentParams.AutoDepthStencilFormat = DepthFormat.D16;

			try
			{
				// Intento crear el dispositivo con la opción CreateFlags.HardwareVertexProcessing
				m_device = new Device(0, DeviceType.Hardware, devWin, CreateFlags.HardwareVertexProcessing, d3dPresentParams);
			}
			catch (DirectXException)
			{ 
				try
				{
					// Hubo un error en la creación del dispositivo, lo intento nuevamente
					// especificando CreateFlags.SoftwareVertexProcessing
					m_device = new Device(0, DeviceType.Hardware, devWin, CreateFlags.SoftwareVertexProcessing, d3dPresentParams);
				}
				catch (DirectXException)
				{
					return false;
				}
			}
			
			m_vertexBuffer = new VertexBuffer(
				typeof(CustomVertex.PositionNormalTextured),   // Formato de vértice utilizado
				5000,										// Tamaño del VertexBuffer
				m_device,								// Device utilizado
				Usage.Dynamic,							// Usage
				CustomVertex.PositionNormalTextured.Format,	// Formato de
				Pool.Default
				);

			m_indexBuffer = new IndexBuffer(
				typeof(short),
				20000,
				m_device,
				Usage.Dynamic,
				Pool.Default
				);

			m_device.DeviceReset += new EventHandler(this.DeviceResetEventHandler);

			return SetupScene();
		}

		private void DeviceResetEventHandler(object sender, EventArgs e)
		{
			SetupScene();
		}

		// Prepara la escena para que sea dibujada correctamente
		public bool SetupScene()
		{
			// Por el momento, no utilizaremos iluminación
			m_device.RenderState.CullMode = Cull.None;
			m_device.RenderState.Lighting = true;

			m_device.RenderState.Ambient = System.Drawing.Color.FromArgb(0xff, 0xff, 0xff, 0xff);

			// Fijamos el valor de la matriz de vista por medio del método estático de Matrix, LookAtLH
			m_device.Transform.View = Matrix.LookAtLH( new Vector3(0.0f, 50.0f, -50.0f), new Vector3(0.0f, 0.0f, 0.0f), new Vector3( 0.0f, 1.0f, 0.0f ) );
			
			float fAspectRatio = m_device.Viewport.Width / (float) m_device.Viewport.Height;
			// Fijamos el valor de la matriz de proyección, en perspectiva, por medio del método estático de Matrix, PerspectiveFovLH
			m_device.Transform.Projection = Matrix.PerspectiveFovLH((float) Math.PI / 4, fAspectRatio, 1.0f, 1000.0f );

			m_device.RenderState.ZBufferEnable = false;
			m_device.RenderState.ZBufferWriteEnable = true;
			m_device.RenderState.ZBufferFunction = Compare.LessEqual;

			m_device.RenderState.AlphaBlendEnable = true;
			m_device.RenderState.SourceBlend = Blend.SourceAlpha;
			m_device.RenderState.DestinationBlend = Blend.InvSourceAlpha;

			m_device.SamplerState[0].MinFilter = TextureFilter.Linear;
			m_device.SamplerState[0].MagFilter = TextureFilter.Linear;

			m_device.RenderState.SpecularEnable = true;
			
			return true;
		}

		public void SetClearColor(int iColor)
		{
			m_iClearColor = iColor;
		}
		// Limpia el backbuffer ------------------------------------------
		public void Clear()
		{
			m_device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, m_iClearColor, 1.0f, 0);
		}
		// Invoco a un BeginScene de la clase Device  --------------------
		public void BeginScene() { m_device.BeginScene();}
		// Invoco a un EndScene de la clase Device  ----------------------
		public void EndScene() { m_device.EndScene(); }
		// Invoco a un Present -------------------------------------------
		public void Present() { m_device.Present();}
		

		// Fija la matriz activa
		public void SetMatrixMode(eMatrixMode emm) { m_matrixMode = emm; }
		// Realiza una translación sobre la matriz activa
		public void Translate(float fX, float fY, float fZ)
		{
			// Realizo una traslación sobre la matriz
			m_theMatrix = Matrix.Translation(fX, fY, fZ);

			// En función del tipo de matriz activa, transformo la
			// matriz de D3D que corresponda
			if (m_matrixMode == eMatrixMode.MM_WORLD)
				m_device.MultiplyTransform(TransformType.World, m_theMatrix);
			else if (m_matrixMode == eMatrixMode.MM_VIEW)
				m_device.MultiplyTransform(TransformType.View, m_theMatrix);
			else if (m_matrixMode == eMatrixMode.MM_PROJECTION)
				m_device.MultiplyTransform(TransformType.Projection, m_theMatrix);
		}
		// Realiza una rotación sobre el eje X sobre la matriz activa
		public void RotateX(float angle)
		{
			m_theMatrix = Matrix.RotationX(angle);

			if (m_matrixMode == eMatrixMode.MM_WORLD)
				m_device.MultiplyTransform(TransformType.World, m_theMatrix);
			else if (m_matrixMode == eMatrixMode.MM_VIEW)
				m_device.MultiplyTransform(TransformType.View, m_theMatrix);
			else if (m_matrixMode == eMatrixMode.MM_PROJECTION)
				m_device.MultiplyTransform(TransformType.Projection, m_theMatrix);
			
		}
		// Realiza una rotación sobre el eje Y sobre la matriz activa
		public void RotateY(float angle)
		{
			m_theMatrix = Matrix.RotationY(angle);

			if (m_matrixMode == eMatrixMode.MM_WORLD)
				m_device.MultiplyTransform(TransformType.World, m_theMatrix);
			else if (m_matrixMode == eMatrixMode.MM_VIEW)
				m_device.MultiplyTransform(TransformType.View, m_theMatrix);
			else if (m_matrixMode == eMatrixMode.MM_PROJECTION)
				m_device.MultiplyTransform(TransformType.Projection, m_theMatrix);
		}
		// Realiza una rotación sobre el eje Z sobre la matriz activa
		public void RotateZ(float angle)
		{
			m_theMatrix = Matrix.RotationZ(angle);

			if (m_matrixMode == eMatrixMode.MM_WORLD)
				m_device.MultiplyTransform(TransformType.World, m_theMatrix);
			else if (m_matrixMode == eMatrixMode.MM_VIEW)
				m_device.MultiplyTransform(TransformType.View, m_theMatrix);
			else if (m_matrixMode == eMatrixMode.MM_PROJECTION)
				m_device.MultiplyTransform(TransformType.Projection, m_theMatrix);
		}
		public void Scale(float fValueX, float fValueY, float fValueZ)
		{
			m_theMatrix = Matrix.Scaling(fValueX, fValueY, fValueZ);

			if (m_matrixMode == eMatrixMode.MM_WORLD)
				m_device.MultiplyTransform(TransformType.World, m_theMatrix);
			else if (m_matrixMode == eMatrixMode.MM_VIEW)
				m_device.MultiplyTransform(TransformType.View, m_theMatrix);
			else if (m_matrixMode == eMatrixMode.MM_PROJECTION)
				m_device.MultiplyTransform(TransformType.Projection, m_theMatrix);
		}

		// Realiza la transformación sobre la matriz activa
		public void Transform()
		{
			if (m_matrixMode == eMatrixMode.MM_WORLD)
				m_device.SetTransform(TransformType.World , m_theMatrix);
			else if (m_matrixMode == eMatrixMode.MM_VIEW)
				m_device.SetTransform(TransformType.View, m_theMatrix);
			else if (m_matrixMode == eMatrixMode.MM_PROJECTION)
				m_device.SetTransform(TransformType.Projection, m_theMatrix);
		}

		public void MultiplyByMatrix(ref Matrix mat)
		{
			if (m_matrixMode == eMatrixMode.MM_WORLD)
				m_device.SetTransform(TransformType.World, Matrix.Multiply(mat, m_device.Transform.World));
			else if (m_matrixMode == eMatrixMode.MM_VIEW)
				m_device.SetTransform(TransformType.View, Matrix.Multiply(mat, m_device.Transform.View));
			else if (m_matrixMode == eMatrixMode.MM_PROJECTION)
				m_device.SetTransform(TransformType.Projection, Matrix.Multiply(mat, m_device.Transform.Projection));
		}

		// Cargo la matriz identidad en la matriz activa
		public void LoadIdentity()
		{
			m_theMatrix = Matrix.Identity;

			if (m_matrixMode == eMatrixMode.MM_WORLD)
				m_device.SetTransform(TransformType.World, m_theMatrix);
			else if (m_matrixMode == eMatrixMode.MM_VIEW)
				m_device.SetTransform(TransformType.View, m_theMatrix);
			else if (m_matrixMode == eMatrixMode.MM_PROJECTION)
				m_device.SetTransform(TransformType.Projection, m_theMatrix);
		}

		// Procesa los vértices de los objetos
		public void ProcsVerts(CustomVertex.PositionColored[] verts, int iVertCount, VertexFormats fmt)
		{
			m_device.VertexFormat = fmt;
			m_device.DrawUserPrimitives(PrimitiveType.TriangleList, iVertCount / 3, verts);
		}

		public void ProcsVerts(CustomVertex.PositionColored[] verts, int iVertCount, PrimitiveType pt)
		{
			m_device.VertexFormat = CustomVertex.PositionColored.Format;
			
			if (pt == PrimitiveType.LineList)
				m_device.DrawUserPrimitives(pt, iVertCount / 2, verts);
			else if (pt == PrimitiveType.PointList)
				m_device.DrawUserPrimitives(pt, iVertCount, verts);
		}
		public void ProcsVerts(PositionScaleColor[] verts, int iVertCount, PrimitiveType pt)
		{
			m_device.VertexFormat = PositionScaleColor.Format;
			
			if (pt == PrimitiveType.LineList)
				m_device.DrawUserPrimitives(pt, iVertCount / 2, verts);
			else if (pt == PrimitiveType.PointList)
				m_device.DrawUserPrimitives(pt, iVertCount, verts);
		}

		public void ProcsVerts(CustomVertex.PositionNormalColored[] verts, int iVertCount)
		{
			m_device.VertexFormat = CustomVertex.PositionNormalColored.Format;
			m_device.DrawUserPrimitives(PrimitiveType.TriangleList, iVertCount / 3, verts);
		}

		public void ProcsVerts(CustomVertex.PositionNormalTextured[] verts, int iVertCount)
		{
			m_device.VertexFormat = CustomVertex.PositionNormalTextured.Format;
			m_device.DrawUserPrimitives(PrimitiveType.TriangleList, iVertCount / 3, verts);
		}

		public void ProcsVertsVB(CustomVertex.PositionNormalTextured[] verts, int iVertCount)
		{
			GraphicsStream stm = m_vertexBuffer.Lock(0, 0, 0);
			stm.Write(verts);
			m_vertexBuffer.Unlock();

			m_device.SetStreamSource( 0, m_vertexBuffer, 0);
			m_device.VertexFormat = CustomVertex.PositionNormalTextured.Format;
			m_device.DrawPrimitives(PrimitiveType.TriangleList, 0, iVertCount / 3);
		}

		public void ProcsIdx(CustomVertex.PositionNormalTextured[] verts, int iVertCount, short[]indices, int iIdxCount)
		{
			GraphicsStream stm = m_vertexBuffer.Lock(0, 0, 0);
			stm.Write(verts);
			m_vertexBuffer.Unlock();

			m_device.SetStreamSource( 0, m_vertexBuffer, 0);
			m_device.VertexFormat = CustomVertex.PositionNormalTextured.Format;

			m_indexBuffer.SetData(indices, 0, LockFlags.None);
			m_device.Indices = m_indexBuffer;
			m_device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, iVertCount, 0, iIdxCount / 3);
		}
		

		public void ChangeViewMat(ref Vector3 v3CamPos, ref Vector3 v3CamObj)
		{
			m_device.Transform.View = Matrix.LookAtLH( v3CamPos, v3CamObj, new Vector3(0.0f, 1.0f, 0.0f));
		}

		public void SetMaterial(ref Material mat)
		{
			m_device.Material = mat;
		}

		public int LoadTexture(string strFilename)
		{
			// Cargo la textura a partir del archivo
            m_aTexs[m_iTexNum] = TextureLoader.FromFile(m_device, strFilename);
			
			// Incremento la cantidad de texturas
			m_iTexNum++;

			return m_iTexNum - 1;
		}
		public void BindTexture(int iTexNum)
		{
			if (iTexNum != -1)
				m_device.SetTexture(0, m_aTexs[iTexNum]);
			else
				m_device.SetTexture(0, null);
		}

		public int CreateMaterial(int[] aiAmb, int[] aiDif, int[] aiSpe, float fShin)
		{
			for (int i=0; i<MAX_MATERIAL_COUNT; i++)
			{
				if (m_aMats[i].SpecularSharpness == -1.0f)
				{
					m_aMats[i].Ambient = System.Drawing.Color.FromArgb(aiAmb[3], aiAmb[2], aiAmb[1], aiAmb[0]);
					m_aMats[i].Diffuse = System.Drawing.Color.FromArgb(aiDif[3], aiDif[2], aiDif[1], aiDif[0]);
					m_aMats[i].Specular = System.Drawing.Color.FromArgb(aiSpe[3], aiSpe[2], aiSpe[1], aiSpe[0]);
					m_aMats[i].Emissive = System.Drawing.Color.FromArgb(0, 0, 0, 0);
					
					m_aMats[i].SpecularSharpness = fShin;

					return i;
				}
			}

			// No existe ningún slot de material para colocar uno nuevo
			// Se debe eliminar alguno con DeleteMaterial
			return -1;
		}
		public void DeleteMaterial(int iMat)
		{
			m_aMats[iMat].SpecularSharpness = -1.0f;
		}

		// Fija un material activo por medio de un subíndice
		public void SetMaterial(int iMat)
		{
			if (iMat != -1)
				m_device.Material = m_aMats[iMat];
		}
	}
}
