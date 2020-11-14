using System.Collections.Generic;

namespace Tss.Core
{
	public class TssMappings
	{
		public class Mapping
		{
			public string Good { get; set; }
			public string NotGood { get; set; }
		}

		public Dictionary<string, Mapping> Mappings { get; set; }
		public Mapping Default { get; set; }
	}
}