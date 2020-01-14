using DCAPST.Canopy;
namespace DCAPST
{
    public class PathwayParametersC3 : PathwayParameters
    {
        public PathwayParametersC3()
        {
            StructuralN = 25;
            SLNRatioTop = 1.32;
            SLNAv = 1.45;

            //Params based on C3
            CiCaRatio = 0.7;
            CiCaRatioIntercept = 0.70;
            CiCaRatioSlope = 0;

            Fcyc = 0;

            PsiRd = 0;
            PsiVc = 1.1;
            PsiJ = 1.85;
            PsiVp = 1;
            PsiGm = 0.00529573590096286;

            X = 0.4;

            
            F2 = 0.75;
            F1 = 0.95;

            // Kc µbar	Curvilinear Temperature Model 
            KcP25 = 273.422964228666;
            KcTEa = 93720;

            // Ko µbar	Curvilinear Temperature Model 
            KoP25 = 165824.064155384;
            KoTEa = 33600;

            VcTEa = 65330;

            //Jmax µmol/m2/s*	Curvilinear Temperature Model 
            JMaxC = 0.911017958600129;
            JTMax = 45;
            JTMin = 0.0;
            JTOpt = 30;
            JBeta = 1;

            VcMax_VoMaxP25 = 4.59217066521612;
            VcMax_VoMaxTEa = 35713.1987127717;
   
            RdTEa = 46390;

            //gm(Arabidopsis, Bernacchi 2002)    µmol/m2/s/bar	
            //GmP25 = 0.55;
            GmC = 0.875790608584141;
            GmTMax = 45;
            GmTMin = 0.0;
            GmTOpt = 29.2338417788683;
            GmBeta = 1;
        }

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
