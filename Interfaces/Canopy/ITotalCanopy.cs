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
        /// Models the radiation which is absorbed by the canopy during photosynthesis
        /// </summary>
        CanopyRadiation Absorbed { get; }

        double LeafAngle { get; set; }
        double LeafWidth { get; set; }
        double LeafNTopCanopy { get; set; }

        double WindSpeed { get; set; }
        double WindSpeedExtinction { get; set; }

        double NAllocationCoeff { get; set; }

        double PropnInterceptedRadns { get; set; }

        /// <summary>
        /// Total leaf area index of the plant
        /// </summary>
        double LAI { get; set; }

        int Layers { get; }

        /// <summary>
        /// Performs initial calculations for the canopy provided daily conditions 
        /// </summary>
        void InitialiseDay(double lai, double sln);

        void PerformTimeAdjustment(ISolarRadiation radiation);

        void CalcCanopyStructure(double sunAngleRadians);

        double CalcBoundaryHeatConductance();

        double CalcSunlitBoundaryHeatConductance();
    }
}
