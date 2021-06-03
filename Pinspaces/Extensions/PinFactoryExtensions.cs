using Microsoft.Extensions.DependencyInjection;
using Pinspaces.Controls;
using Pinspaces.Core.Controls;
using Pinspaces.Core.Interfaces;
using Pinspaces.Interfaces;
using Pinspaces.Shell.Controls;
using System;
using System.Reflection;

namespace Pinspaces.Extensions
{
    public static class PinFactoryExtensions
    {
        public static IServiceCollection AddPinControls(this IServiceCollection services)
        {
            // TODO: Load plugin assemblies and automatically register compatible types

            var pinControlTypes = new RegisteredPinControlTypes()
            {
                typeof(FileListPinPanel),
                typeof(TextBoxPinPanel)
            };
            services.AddSingleton<IRegisteredPinControlTypes>((p) => pinControlTypes);

            foreach (var pinControlType in pinControlTypes)
            {
                services.AddPinControl(pinControlType);
            }
            return services;
        }

        private static IServiceCollection AddPinControl(this IServiceCollection services, Type pinControlType)
        {
            if (!pinControlType.IsAssignableTo(typeof(IPinControl)))
            {
                throw new ArgumentException($"Incompatible Pin Control Type: {pinControlType.FullName}");
            }

            var attribute = pinControlType.GetCustomAttribute<PinTypeAttribute>();
            if (attribute == null)
            {
                throw new ArgumentException($"Pin Type Attribute not found: {pinControlType.FullName}");
            }

            services.AddTransient(pinControlType);
            services.AddTransient(attribute.PinType);

            return services;
        }
    }
}
