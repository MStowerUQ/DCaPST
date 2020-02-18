namespace DCAPST.Interfaces
{
    
    public interface IAssimilation
    {
        ILeafWaterInteraction LeafWater { get; }

        /// <summary>
        /// Attempt to calculate possible changes to the assimilation value under current conditions.
        /// Returns false if the updated assimilation value is not sensible, otherwise it returns true.
        /// </summary>
        void UpdateAssimilation(WaterParameters Params);        

        double GetCO2Rate();

        double GetWaterUse();
    }
}
