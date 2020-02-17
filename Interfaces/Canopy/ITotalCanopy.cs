using DCAPST.Canopy;

namespace DCAPST.Interfaces
{
    public interface ITotalCanopy
    {
        /// <summary>
        /// A collection of predefined parameters used by the canopy
        /// </summary>
        ICanopyParameters Canopy { get; }

        /// <summary>
        /// The section of canopy currently in sunlight
        /// </summary>
        IPartialCanopy Sunlit { get; }

        /// <summary>
        /// The section of canopy currently in shade
        /// </summary>
        IPartialCanopy Shaded { get; }        

        /// <summary>
        /// Performs initial calculations for the canopy provided daily conditions 
        /// </summary>
        void InitialiseDay(double lai, double sln);

        /// <summary>
        /// Updates 
        /// </summary>
        void RecalculateRadiation(ISolarRadiation radiation);

        void CalcCanopyStructure(double sunAngleRadians);

        double GetInterceptedRadiation();

        double CalcBoundaryHeatConductance();

        double CalcSunlitBoundaryHeatConductance();
    }
}
