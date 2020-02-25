namespace DCAPST.Interfaces
{
    
    public interface IAssimilation
    {
        AssimilationFunction GetFunction(AssimilationPathway pathway);

        void UpdateIntercellularCO2(AssimilationPathway pathway, double gt, double waterUseMolsSecond);

        void UpdatePartialPressures(AssimilationPathway pathway, AssimilationFunction function);
    }
}
