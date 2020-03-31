using System;
using ModelFramework;

using DCAPST;
using DCAPST.Canopy;
using DCAPST.Environment;
using DCAPST.Interfaces;
using DCAPST.Utilities; 

public class Script 
{      
   [Link]  public Simulation MySimulation;
   [Link] Paddock MyPaddock; // Can be used to dynamically get access to simulation structure and variables
   [Input] DateTime Today;   // Equates to the value of the current simulation date - value comes from CLOCK
   [Output] public double[] dcaps = new double[5];
   
   //Additional Outputs
   [Output] public double BIOtotalDAY;
   [Output] public double BIOshootDAY;
   [Output] public double RootShoot;
   [Output] public double EcanDemand; 
   [Output] public double EcanSupply;
   [Output] public double RUE;
   [Output] public double TE; 
   [Output] public double RadIntDcaps; 
   [Output] public double BIOshootDAYPot;
   [Output] public double SoilWater;
   
   public CanopyParameters CP;
   public PathwayParameters PP;
   public DCAPSTModel DM;
   
   public double LAITrigger = 0.5;
   public double PsiFactor = 1.0; // Psi reduction factor

   // The following event handler will be called once at the beginning of the simulation
   [EventHandler] public void OnInitialised()
   {
      /* IMPORTANT - Do NOT change the order of these values */

      CP = Classic.SetUpCanopy(
         CanopyType.C4, // Canopy type
         370,     // CO2 partial pressure
         0.7,     // Empirical curvature factor
         0.047,   // Diffusivity-solubility ratio
         210000,  // O2 partial pressure
         0.78,    // PAR diffuse extinction coefficient
         0.8,     // NIR diffuse extinction coefficient
         0.036,   // PAR diffuse reflection coefficient
         0.389,   // NIR diffuse reflection coefficient
         60,      // Leaf angle
         0.15,    // PAR leaf scattering coefficient
         0.8,     // NIR leaf scattering coefficient
         0.05,    // Leaf width
         1.3,     // SLN ratio at canopy top
         14,      // Minimum structural nitrogen
         1.5,     // Wind speed
         1.5      // Wind speed profile distribution coefficient
      );

      PP = Classic.SetUpPathway(
         0,                   // Electron transport minimum temperature
         30.0,                // Electron transport optimum temperature
         45.0,                // Electron transport maximum temperature
         0.911017958600129,   // Electron transport scaling constant
         1,                   // Electron transport Beta value

         0,                   // Mesophyll conductance minimum temperature
         29.2338417788683,    // Mesophyll conductance optimum temperature
         45,                  // Mesophyll conductance maximum temperature
         0.875790608584141,   // Mesophyll conductance scaling constant
         1,                   // Mesophyll conductance Beta value

         273.422964228666,    // Kc25 - Michaelis Menten constant of Rubisco carboxylation at 25 degrees C
         93720,               // KcFactor

         165824.064155384,    // Ko25 - Michaelis Menten constant of Rubisco oxygenation at 25 degrees C
         33600,               // KoFactor

         4.59217066521612,    // VcVo25 - Rubisco carboxylation to oxygenation at 25 degrees C
         35713.19871277176,   // VcVoFactor

         0.0,   // Kp25 - Michaelis Menten constant of PEPc activity at 25 degrees C (Unused in C3)
         0.0,   // KpFactor (Unused in C3)

         65330, // VcFactor
         46390, // RdFactor
         57043.2677590512, // VpFactor

         0.0,     // PEPc regeneration (Unused in C3)
         0.15,    // Spectral correction factor
         0.1,     // Photosystem II activity fraction
         0.003,   // Bundle sheath CO2 conductance
         1.1 * PsiFactor,        // Max Rubisco activity to SLN ratio
         1.85 * PsiFactor,       // Max electron transport to SLN ratio
         0.0 * PsiFactor,        // Respiration to SLN ratio
         0.0 * PsiFactor,        // Max PEPc activity to SLN ratio
         0.005296 * PsiFactor,   // Mesophyll CO2 conductance to SLN ratio
         2,    // Extra ATP cost
         0.4,  // Mesophyll electron transport fraction
         0.7   // Intercellular CO2 to air CO2 ratio
      );

      //Set the LAI trigger
      MyPaddock.Set("laiTrigger", LAITrigger);
   }
   
   // This routine is called when the plant model wants us to do the calculation
   [EventHandler] public void Ondodcaps() 
   {
      int DOY = 0;
      double latitude = 0;
      double maxT = 0;
      double minT = 0;
      double radn = 0;
      double RootShootRatio = 0;
      double SLN = 0;
      double SWAvailable = 0;
      double lai = 0;
     
      MyPaddock.Get("DOY", out DOY);
      MyPaddock.Get("latitude", out latitude);
      MyPaddock.Get("maxT", out maxT);
      MyPaddock.Get("minT", out minT);
      MyPaddock.Get("radn", out radn);
      MyPaddock.Get("RootShootRatio", out RootShootRatio);
      MyPaddock.Get("SLN", out SLN);
      MyPaddock.Get("SWAvailable", out SWAvailable);
      MyPaddock.Get("lai", out lai);
            
      // Model the photosynthesis
      DCAPSTModel DM = Classic.SetUpModel(CP, PP, DOY, latitude, maxT, minT, radn);
      DM.DailyRun(lai, SLN, SWAvailable, RootShootRatio);
      //DM.DailyRun(0.5, 1.0, 3.5, 1.3, 100);
      
      // Outputs
      RootShoot = RootShootRatio;
      BIOshootDAY = dcaps[0] = DM.ActualBiomass;
      BIOtotalDAY = BIOshootDAY * (1 + RootShoot);
      EcanDemand = dcaps[1] = DM.WaterDemanded; 
      EcanSupply = dcaps[2] = DM.WaterSupplied;
      RadIntDcaps = dcaps[3] = DM.InterceptedRadiation;
      RUE = (RadIntDcaps == 0 ? 0 : BIOshootDAY / RadIntDcaps);
      TE = (EcanSupply == 0 ? 0 : BIOshootDAY / EcanSupply);
      BIOshootDAYPot = dcaps[4] = DM.PotentialBiomass;
      SoilWater = SWAvailable;
   }
      
   // Set its default value to garbage so that we find out quickly
   [EventHandler] public void OnPrepare()
   {
      RootShoot = 0;
      BIOshootDAY = 0;
      BIOtotalDAY = 0;
      EcanDemand = 0; 
      EcanSupply = 0;
      RadIntDcaps = 0;
      RUE = 0;
      TE = 0;
      BIOshootDAYPot = 0;
      
      for(int i = 0; i < 5; i++) { dcaps[i] = -1.0f;}
   }
}