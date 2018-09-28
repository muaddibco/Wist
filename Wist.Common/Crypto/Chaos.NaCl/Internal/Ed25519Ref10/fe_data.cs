using Chaos.NaCl.Internal.Ed25519Ref10;

namespace Chaos.NaCl.Internal.Ed25519Ref10
{
    internal static partial class FieldOperations
    {
        internal static FieldElement fe_ma2 = new FieldElement( -12721188, -3529, 0, 0, 0, 0, 0, 0, 0, 0 ); /* -A^2 */
        internal static FieldElement fe_ma = new FieldElement(-486662, 0, 0, 0, 0, 0, 0, 0, 0, 0 ); /* -A */
        internal static FieldElement fe_fffb1 = new FieldElement(-31702527, -2466483, -26106795, -12203692, -12169197, -321052, 14850977, -10296299, -16929438, -407568 ); /* sqrt(-2 * A * (A + 2)) */
        internal static FieldElement fe_fffb2 = new FieldElement(8166131, -6741800, -17040804, 3154616, 21461005, 1466302, -30876704, -6368709, 10503587, -13363080 ); /* sqrt(2 * A * (A + 2)) */
        internal static FieldElement fe_fffb3 = new FieldElement(-13620103, 14639558, 4532995, 7679154, 16815101, -15883539, -22863840, -14813421, 13716513, -6477756 ); /* sqrt(-sqrt(-1) * A * (A + 2)) */
        internal static FieldElement fe_fffb4 = new FieldElement(-21786234, -12173074, 21573800, 4524538, -4645904, 16204591, 8012863, -8444712, 3212926, 6885324 ); /* sqrt(sqrt(-1) * A * (A + 2)) */
        internal static FieldElement fe_sqrtm1 = new FieldElement ( -32595792, -7943725, 9377950, 3500415, 12389472, -272473, -25146209, -2005654, 326686, 11406482 ); /* sqrt(-1) */
    }
}