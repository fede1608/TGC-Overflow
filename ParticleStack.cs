using System;

namespace AlumnoEjemplos.overflowDT
{
	/// <summary>
	/// 
	/// </summary>
	public class ParticleStack
	{
		public ParticleStack(int iMax)
		{
			m_particleList = new Particle[iMax];
			m_iLastElement = 0;
		}
		
		/// <summary>
		/// Inserta elementos en la pila
		/// </summary>
		/// <param name="p"></param>
		/// <returns></returns>
		public bool Insert(Particle p)
		{
			// �La pila est� llena?
			if (m_iLastElement == m_particleList.Length)
				return false;

			// Fijo el valor de un elemento en el �ltimo elemento
			m_particleList[m_iLastElement] = p;

			// Incremento el contador que indica el �ltimo elemento
			m_iLastElement++;

			return true;

		}

		/// <summary>
		/// Extrae elementos de la pila
		/// </summary>
		/// <param name="p"></param>
		/// <returns></returns>
		public bool Extract(out Particle p)
		{
			// �La pila est� vac�a?
			if (m_iLastElement == 0)
			{
				p = null;
				return false;
			}

			m_iLastElement--;

			p = m_particleList[m_iLastElement];

			return true;
		}

		public Particle GetFirst()
		{
			// �La pila est� vac�a?
			if (m_iLastElement == 0)
				return null;

			m_iCursor = 0;
			
			return m_particleList[m_iCursor];

		}

		public Particle GetNext()
		{
			m_iCursor++;

			// �No hay mas datos que leer?
			if (m_iCursor >= m_iLastElement)
				return null;

			return m_particleList[m_iCursor];
		}

		// Propiedades
		private Particle[] m_particleList = null;
		private int m_iLastElement;
		private int m_iCursor;
	}
}
