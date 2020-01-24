﻿using DCAPST.Interfaces;

namespace DCAPST.Canopy
{
    public class CanopyParameters : ICanopyParameters
    {
        public CanopyType Type { get; set; }

        public IPathwayParameters Pathway { get; set; }

        public double AirCO2 { get; set; }
        public double Windspeed { get; set; }
        public double LeafAngle { get; set; }
        public double DiffuseExtCoeff { get; set; }
        public double DiffuseExtCoeffNIR { get; set; }
        public double LeafScatteringCoeff { get; set; }
        public double LeafScatteringCoeffNIR { get; set; }
        public double DiffuseReflectionCoeff { get; set; }
        public double DiffuseReflectionCoeffNIR { get; set; }
        public double LeafWidth { get; set; }
        public double WindSpeedExtinction { get; set; }        
        public double ConvexityFactor { get; set; }
        public double DiffusivitySolubilityRatio { get; set; }
       
        public double StructuralN { get; set; }
        public double SLNRatioTop { get; set; }
        public double SLNAv { get; set; }
    }
}
