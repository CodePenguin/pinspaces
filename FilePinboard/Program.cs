using Autofac;
using System;
using System.Windows.Forms;

namespace FilePinboard
{
    static class Program
    {
        private static IContainer BuildContainer()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new ApplicationContainer { PinboardFormFactory = c.Resolve<Func<PinboardForm>>() });
            builder.RegisterType<ApplicationContext>();
            builder.RegisterType<PinboardForm>();
            return builder.Build();
        }

        [STAThread]
        internal static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var context = BuildContainer().Resolve<ApplicationContext>();
            Application.Run(context);
        }
    }
}
