using System;

namespace AlumnoEjemplos.overflowDT
{
	/// <summary>
	/// 
	/// </summary>
	public class ParticleQueue
	{
		public ParticleQueue(int iMax)
		{
			m_particleList = new Particle[iMax];
			m_iFirstElement = m_iLastElement = 0;
		}
		
		/// <summary>
		/// Inserta elementos en la cola
		/// </summary>
		/// <param name="p"></param>
		/// <returns></returns>
		public bool Insert(Particle p)
		{
			// ¿La cola está llena?
			if (m_iLastElement == m_iFirstElement - 1 ||
				(m_iLastElement == m_particleList.Length-1 && m_iFirstElement == 0))
				return false;
	
			// Fijo el valor de un elemento en el último elemento
			m_particleList[m_iLastElement] = p;
			
			// Incremento el contador que indica el último elemento
			m_iLastElement++;

			if (m_iLastElement == m_particleList.Length)
				m_iLastElement = 0;

			return true;

		}

		/// <summary>
		/// Extrae elementos de la cola
		/// </summary>
		/// <param name="p"></param>
		/// <returns></returns>
		public bool Extract(out Particle p)
		{
			// ¿La cola está vacía?
			if (m_iFirstElement == m_iLastElement)
			{
				p = null;
				return false;
			}

			p = m_particleList[m_iFirstElement];

			// Incremento el contador que indica el primer elemento
			m_iFirstElement++;

			if (m_iFirstElement == m_particleList.Length)
				m_iFirstElement = 0;

			return true;
		}

		public Particle GetFirst()
		{
			// ¿La cola está vacía?
			if (m_iFirstElement == m_iLastElement)
				return null;

			m_iCursor = m_iFirstElement;
			
			return m_particleList[m_iCursor];

		}

		public Particle GetNext()
		{
			m_iCursor++;

			if (m_iCursor == m_particleList.Length)
				m_iCursor = 0;

			// ¿No hay mas datos que leer?
			if (m_iCursor == m_iLastElement)
				return null;

			return m_particleList[m_iCursor];
		}

		
		// Propiedades
		private Particle[] m_particleList = null;
		private int m_iFirstElement;
		private int m_iLastElement;
		private int m_iCursor;
	}
}
