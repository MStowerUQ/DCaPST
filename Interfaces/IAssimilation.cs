namespace DCAPST.Interfaces
{
    public enum AssimilationType { Ac1, Ac2, Aj }

    public interface IAssimilation
    {
        AssimilationType Type { get; set; }

        double CO2Rate { get; set; }
        double WaterUse { get; set; }
        double LeafTemperature { get; set; }
        double IntercellularCO2 { get; set; }
        double MesophyllCO2 { get; set; }
        double ChloroplasticCO2 { get; set; }
        double ChloroplasticO2 { get; set; }

        /// <summary>
        /// Attempt to calculate possible changes to the assimilation value under current conditions.
        /// Returns false if the updated assimilation value is not sensible, otherwise it returns true.
        /// </summary>
        void TryUpdateAssimilation(ILeafWaterInteraction Water, PhotosynthesisParams Params);
    }
}
