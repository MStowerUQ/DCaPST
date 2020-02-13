using System;
using DCAPST.Interfaces;

namespace DCAPST
{
    /// <summary>
    /// Manages the parameters used to calculate assimilation
    /// </summary>
    public abstract class AssimilationParameters
    {
        protected readonly ICanopyParameters canopy;        
        protected readonly IPartialCanopy partial;
        protected readonly IAssimilation assimilation;
        protected readonly IPathwayParameters path;
        public AssimilationCalculator Calculator;

        public AssimilationParameters(IAssimilation assimilation, IPartialCanopy partial)
        {            
            this.assimilation = assimilation;
            this.partial = partial;
            canopy = partial.Canopy;
            path = canopy.Pathway;

            Current = new LeafTemperatureFunction(partial);

            MesophyllCO2 = canopy.AirCO2 * path.IntercellularToAirCO2Ratio;
            ChloroplasticCO2 = MesophyllCO2 + 20;
            ChloroplasticO2 = 210000;
        }

        public static AssimilationParameters Create(IAssimilation assimilation, IPartialCanopy partial)
        {
            if (partial.Canopy.Type == CanopyType.C3)
                return new ParametersC3(assimilation, partial);
            else if (partial.Canopy.Type == CanopyType.C4)
                return new ParametersC4(assimilation, partial);
            else
                return new ParametersCCM(assimilation, partial);
        }
        
        public double MesophyllCO2 { get; set; }
        public double ChloroplasticCO2 { get; set; }
        public double ChloroplasticO2 { get; set; }

        public LeafTemperatureFunction Current { get; } 

        public double Gbs => path.BundleSheathCO2ConductancePerLeaf * partial.LAI;
        public double Vpr => path.PEPRegenerationPerLeaf * partial.LAI;

        public void PrepareCalculator()
        {
            if (assimilation.Type == AssimilationType.Ac1) Calculator = GetAc1Calculator();
            else if (assimilation.Type == AssimilationType.Ac2) Calculator = GetAc2Calculator();
            else Calculator = GetAjCalculator();            
        }

        public virtual void UpdateMesophyllCO2(double intercellularCO2, double CO2Rate) { /*C4 & CCM overwrite this.*/ }
        public virtual void UpdateChloroplasticO2(double CO2Rate) { /*CCM overwrites this.*/ }
        public virtual void UpdateChloroplasticCO2(double CO2Rate) { /*CCM overwrites this.*/ }

        protected abstract AssimilationCalculator GetAc1Calculator();
        protected abstract AssimilationCalculator GetAc2Calculator();
        protected abstract AssimilationCalculator GetAjCalculator();
    }
}
