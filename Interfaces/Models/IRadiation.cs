namespace DCAPST.Interfaces
{
    public interface IRadiation
    {
        double TotalIncidentRadiation { get; }
        double Direct { get; }
        double Diffuse { get; }
        double DirectPAR { get; }
        double DiffusePAR { get; }


        void UpdateHourlyRadiation(double time);
    }
}
