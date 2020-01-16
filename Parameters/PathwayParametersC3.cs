using DCAPST.Canopy;
namespace DCAPST
{
    public class PathwayParametersC3 : PathwayParameters
    {

        protected override AssimilationParameters GetAc1Params(PartialCanopy s)
        {
            var param = new AssimilationParameters()
            {
                x1 = s.VcMaxT,
                x2 = s.Kc / s.Ko,
                x3 = s.Kc,
                x4 = 0.0,
                x5 = 0.0,
                x6 = 0.0,
                x7 = 0.0,
                x8 = 0.0,
                x9 = 0.0,

                m = s.Rm,
                t = s.G_,
                sb = 0.0,
                j = 1.0,
                e = s.OxygenPartialPressure,
                R = s.RdT
            };

            return param;
        }

        protected override AssimilationParameters GetAc2Params(PartialCanopy s)
        {
            var param = new AssimilationParameters()
            {
                x1 = 0.0,
                x2 = 0.0,
                x3 = 0.0,
                x4 = 0.0,
                x5 = 0.0,
                x6 = 0.0,
                x7 = 0.0,
                x8 = 0.0,
                x9 = 0.0,

                m = 0.0,
                t = 0.0,
                sb = 0.0,
                j = 0.0,
                e = 0.0,
                R = 0.0
            };

            return param;
        }

        protected override AssimilationParameters GetAjParams(PartialCanopy s)
        {
            var param = new AssimilationParameters()
            {
                x1 = s.J / 4,
                x2 = 2 * s.G_,
                x3 = 0.0,
                x4 = 0.0,
                x5 = 0.0,
                x6 = 0.0,
                x7 = 0.0,
                x8 = 0.0,
                x9 = 0.0,

                m = s.Rm,
                t = s.G_,
                sb = 0.0,
                j = 1.0,
                e = s.OxygenPartialPressure,
                R = s.RdT
            };

            return param;
        }
    }
}
