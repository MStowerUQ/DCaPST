namespace DCAPST.Interfaces
{
    
    public interface IAssimilation
    {
        /// <summary>
        /// Attempt to calculate possible changes to the assimilation value under current conditions.
        /// Returns false if the updated assimilation value is not sensible, otherwise it returns true.
        /// </summary>
        void UpdateAssimilation(ITemperature Water, WaterParameters Params);

        double GetCO2Rate();

        double GetWaterUse();
    }
}
