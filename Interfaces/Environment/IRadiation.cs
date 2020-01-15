namespace DCAPST.Interfaces
{
    public interface IRadiation
    {
        double TotalIncidentRadiation { get; }
        double DirectRadiation { get; }
        double DiffuseRadiation { get; }
        double DirectRadiationPAR { get; }
        double DiffuseRadiationPAR { get; }


        void UpdateHourlyRadiation(double time);
    }
}
