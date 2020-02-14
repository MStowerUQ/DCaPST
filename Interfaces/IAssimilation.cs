namespace DCAPST.Interfaces
{
    public enum AssimilationType { Ac1, Ac2, Aj }

    public interface IAssimilation
    {
        Pathway Path { get; }

        /// <summary>
        /// Attempt to calculate possible changes to the assimilation value under current conditions.
        /// Returns false if the updated assimilation value is not sensible, otherwise it returns true.
        /// </summary>
        void UpdateAssimilation(ITemperature Water, WaterParameters Params);
    }
}
