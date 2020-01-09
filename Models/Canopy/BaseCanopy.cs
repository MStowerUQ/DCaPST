namespace DCAPST.Canopy
{
    public abstract class BaseCanopy
    {
        public AbsorbedRadiation Rad;
        public AbsorbedRadiation PAR;
        public AbsorbedRadiation NIR;

        // [Description("Total Leaf Area Index of the plant")]
        // [Units("m2 / m2")]
        // [Symbol("L")]
        // [Subscript("c")]
        public double LAI { get; set; } = 5;

        // [Description("Vcmax for the sunlit and shade leaf fractions  @ 25°")]
        // [Units("μmol/m2/s")]
        // [Symbol("V")]
        // [Subscript("c_max")]
        public double VcMax25 { get; set; }

        // [Description("Rd for the sunlit and shade leaf fractions @ 25°")]
        // [Units("μmol/m2/s1")]
        // [Symbol("Rd")]
        // [Subscript("25")]
        public double Rd25 { get; set; }

        // [Description("Jmax for the sunlit and shade leaf fractions @ 25°")]
        // [Units("μmol/m2/s")]
        // [Symbol("J")]
        // [Subscript("max_25")]
        public double JMax25 { get; set; }
        
        // [Description("Maximum rate of P activity-limited carboxylation for the canopy @ 25°")]
        // [Units("μmol/m2/s")]
        // [Symbol("V")]
        // [Subscript("p_max_25")]
        public double VpMax25 { get; set; }

        // [Description("")]
        // [Units("μmol/m2/s")]
        // [Symbol("G")]
        // [Subscript("m_25")]
        public double Gm25 { get; set; }
    }
}
