namespace DCAPST.Interfaces
{
    public enum AssimilationType { Ac1, Ac2, Aj }

    public interface IAssimilation
    {
        AssimilationType Type { get; set; }

        Pathway Path { get; }

        /// <summary>
        /// Attempt to calculate possible changes to the assimilation value under current conditions.
        /// Returns false if the updated assimilation value is not sensible, otherwise it returns true.
        /// </summary>
        void UpdateAssimilation(ILeafWaterInteraction Water, WaterParameters Params);
    }
}
