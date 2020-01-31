namespace DCAPST.Interfaces
{
    public interface IPartialCanopy
    {
        /// <summary>
        /// A collection of predefined parameters used by the canopy
        /// </summary>
        ICanopyParameters Canopy { get; }

        /// <summary>
        /// Leaf area index of this region of the canopy
        /// </summary>
        double LAI { get; set; }

        /// <summary>
        /// The energy the canopy absorbs through solar radiation
        /// </summary>
        double AbsorbedRadiation { get; set; }

        /// <summary>
        /// The number of photosynthetic active photons which reach the canopy
        /// </summary>
        double PhotonCount { get; set; }

        /// <summary>
        /// Rate of biomass conversion
        /// </summary>
        double CO2AssimilationRate { get; set; }

        /// <summary>
        /// How much water the canopy consumes
        /// </summary>
        double WaterUse { get; set; }

        /// <summary>
        /// Maximum rubisco activity at 25 degrees Celsius
        /// </summary>
        double RubiscoActivity25 { get; set; }

        /// <summary>
        /// Maximum respiration at 25 degrees Celsius
        /// </summary>
        double Rd25 { get; set; }

        /// <summary>
        /// Maximum electron transport rate at 25 degrees Celsius
        /// </summary>
        double JMax25 { get; set; }

        /// <summary>
        /// Maximum PEPc activity at  25 degrees Celsius
        /// </summary>
        double PEPcActivity25 { get; set; }

        /// <summary>
        /// Maximum mesophyll CO2 conductance at 25 degrees Celsius
        /// </summary>
        double MesophyllCO2Conductance25 { get; set; }

        /// <summary>
        /// Runs the photosynthesis calculations for the canopy
        /// </summary>
        void CalculatePhotosynthesis(ITemperature temperature, PhotosynthesisParams Params);
    }
}
