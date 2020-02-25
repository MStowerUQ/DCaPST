namespace DCAPST.Interfaces
{
    
    public interface IAssimilation
    {
        AssimilationFunction GetFunction(AssimilationPathway pathway, LeafTemperatureResponseModel leaf);

        void UpdateIntercellularCO2(AssimilationPathway pathway, double gt, double waterUseMolsSecond);

        void UpdatePartialPressures(AssimilationPathway pathway, LeafTemperatureResponseModel leaf, AssimilationFunction function);
    }
}
