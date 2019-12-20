using DCAPST.Canopy;
namespace DCAPST
{
    public class PathwayParametersCCM : PathwayParameters
    {
        public PathwayParametersCCM()
        {
            StructuralN = 25;
            SLNRatioTop = 1.3;
            SLNAv = 1.3;
            PsiVc = 0.5;
            PsiJ = 2.3;
            PsiRd = 0;
            PsiVp = 0.9;
            F2 = 0.75;
            F1 = 0.95;
            Fcyc = 0.136;
            CiCaRatio = 0.4;
            CiCaRatioIntercept = 0.4;
            CiCaRatioSlope = 0;
            KcP25 = 1210;
            KcTEa = 64200;
            KoP25 = 292000;         
            KoTEa = 10500;
            VcTEa = 78000;
            JMaxC = 0.7667497364194;
            JTMax = 55;
            JTMin = 0.0;
            JTOpt = 37.8649150880407;
            JBeta = 1;
            VcMax_VoMaxP25 = 5.51328906454566;
            VcMax_VoMaxTEa = 21265.4029552906;
            KpP25 = 139;
            KpTEa = 36300;
            VpMaxTEa = 57043.2677590512;
            RdTEa = 46390;
            GmC = 0.445752468880047;
            GmTMax = 55;
            GmTMin = 0.0;
            GmTOpt = 42;
            GmBeta = 1;
        }

        protected override AssimilationParameters GetAc1Params(PartialCanopy s)
        {
            var param = new AssimilationParameters()
            {
                x_1 = s.VcMaxT,
                x_2 = s.Kc / s.Ko,
                x_3 = s.Kc,
                x_4 = s.VpMaxT / (s.Cm + s.Kp),
                x_5 = 0.0,
                x_6 = 0.0,
                x_7 = s.Cc * s.VcMaxT / (s.Cc + s.Kc * (1 + s.Oc / s.Ko)),
                x_8 = 1.0,
                x_9 = 1.0,

                m = s.Rm,
                t = s.G_,
                b = 0.1 / s.Constant,
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
                x_1 = s.VcMaxT,
                x_2 = s.Kc / s.Ko,
                x_3 = s.Kc,
                x_4 = 0.0,
                x_5 = s.Vpr,
                x_6 = 0.0,
                x_7 = s.Cc * s.VcMaxT / (s.Cc + s.Kc * (1 + s.Oc / s.Ko)),
                x_8 = 1.0,
                x_9 = 1.0,

                m = s.Rm,
                t = s.G_,
                b = 0.1 / s.Constant,
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
                x_1 = (1 - s.CPath.X) * s.CPath.z * s.J / 3.0,
                x_2 = 7.0 / 3.0 * s.G_,
                x_3 = 0.0,
                x_4 = 0.0,
                x_5 = s.CPath.X * s.CPath.z * s.J / s.CPath.Phi,
                x_6 = 0.0,
                x_7 = s.Cc * (1 - s.CPath.X) * s.CPath.z * s.J / (3 * s.Cc + 7 * s.G_ * s.Oc),
                x_8 = 1.0,
                x_9 = 1.0,

                m = s.Rm,
                t = s.G_,
                b = 0.1 / s.Constant,
                j = s.Gbs,
                e = s.OxygenPartialPressure,
                R = s.RdT
            };

            return param;
        }
    }
}
