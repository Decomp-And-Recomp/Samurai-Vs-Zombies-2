using System.Runtime.InteropServices;

namespace Glu.Diagnostics.Internal
{
	public static class NativeAPI
	{
		private const string __importName = "GluDiagnostics";

		[DllImport("GluDiagnostics")]
		public static extern void Glu_Diagnostics_Abort();

		[DllImport("GluDiagnostics")]
		public static extern void Glu_Console_WriteLine(string line);
	}
}
