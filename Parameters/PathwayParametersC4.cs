using DCAPST.Canopy;
namespace DCAPST
{
    public class PathwayParametersC4 : PathwayParameters
    {

        protected override AssimilationParameters GetAc1Params(PartialCanopy s)
        {
            var param = new AssimilationParameters()
            {
                x1 = s.VcMaxT,
                x2 = s.Kc / s.Ko,
                x3 = s.Kc,
                x4 = s.VpMaxT / (s.Cm + s.Kp),
                x5 = 0.0,
                x6 = 1.0,
                x7 = 0.0,
                x8 = 1.0,
                x9 = 1.0,

                m = s.Rm,
                t = s.G_,
                sb = 0.1 / s.CPath.Canopy.DiffusivitySolubilityRatio,
                j = s.Gbs,
                e = s.OxygenPartialPressure,
                R = s.RdT
            };

            return param;
        }

        protected override AssimilationParameters GetAc2Params(PartialCanopy s)
        {
            var param = new AssimilationParameters()
            {
                x1 = s.VcMaxT,
                x2 = s.Kc / s.Ko,
                x3 = s.Kc,
                x4 = 0.0,
                x5 = s.Vpr,
                x6 = 1.0,
                x7 = 0.0,
                x8 = 1.0,
                x9 = 1.0,

                m = s.Rm,
                t = s.G_,
                sb = 0.1 / s.CPath.Canopy.DiffusivitySolubilityRatio,
                j = s.Gbs,
                e = s.OxygenPartialPressure,
                R = s.RdT
            };

            return param;
        }

        protected override AssimilationParameters GetAjParams(PartialCanopy s)
        {
            var param = new AssimilationParameters()
            {
                x1 = (1.0 - s.CPath.X) * s.J / 3.0,
                x2 = 7.0 / 3.0 * s.G_,
                x3 = 0.0,
                x4 = 0.0,
                x5 = s.CPath.X * s.J / s.CPath.Phi,
                x6 = 1.0,
                x7 = 0.0,
                x8 = 1.0,
                x9 = 1.0,

                m = s.Rm,
                t = s.G_,
                sb = 0.1 / s.CPath.Canopy.DiffusivitySolubilityRatio,
                j = s.Gbs,
                e = s.OxygenPartialPressure,
                R = s.RdT
            };

            return param;
        }
    }
}
