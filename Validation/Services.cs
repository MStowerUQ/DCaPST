using DCAPST;
using DCAPST.Canopy;
using DCAPST.Environment;
using DCAPST.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Validation
{
    static class Services
    {
        public static IServiceProvider Register()
        {
            var collection = new ServiceCollection();
            collection.AddSingleton<IPhotosynthesisModel, PhotosynthesisModel>();
            collection.AddSingleton<ISolarGeometry, SolarGeometryModel>();
            collection.AddSingleton<ISolarRadiation, SolarRadiationModel>();
            collection.AddSingleton<ITemperature, TemperatureModel>();
            collection.AddSingleton<ICanopyParameters, CanopyParameters>();
            collection.AddSingleton<IPathwayParameters, PathwayParameters>();
            collection.AddSingleton<ITotalCanopy, TotalCanopy>();
            collection.AddSingleton<ILeafWaterInteraction, LeafWaterInteractionModel>();
            collection.AddSingleton(typeof(LeafTemperatureResponseModel));
            collection.AddSingleton(typeof(IAssimilation), sp => AssimilationFactory(sp));

            collection.AddTransient<IPartialCanopy, PartialCanopy>();

            return collection.BuildServiceProvider();
        }

        public static void Dispose(this IServiceProvider provider)
        {
            if (provider == null)
            {
                return;
            }
            if (provider is IDisposable)
            {
                ((IDisposable)provider).Dispose();
            }
        }

        private static IAssimilation AssimilationFactory(IServiceProvider sp)
        {
            var canopy = sp.GetService<ICanopyParameters>();
            var pathway = sp.GetService<IPathwayParameters>();

            switch(canopy.Type)
            {
                case (CanopyType.C3):
                    return new AssimilationC3(canopy, pathway);

                case (CanopyType.C4):
                    return new AssimilationC4(canopy, pathway);

                case (CanopyType.CCM):
                    return new AssimilationCCM(canopy, pathway);

                default:
                    throw new Exception("You have reached unreachable code. Congratulations.");
            }
        }
    }
}
