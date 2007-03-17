using System;
using System.Collections.Generic;
using System.Text;

namespace KeePassLib
{
	public sealed class SecureString
	{
		private string m_secString = string.Empty;

		public int Length
		{
			get { return m_secString.Length; }
		}

		public void AppendChar(char ch)
		{
			m_secString += ch;
		}

		public string ReadAsString()
		{
			return m_secString;
		}

		public void Clear()
		{
			m_secString = string.Empty;
		}
	}
}
