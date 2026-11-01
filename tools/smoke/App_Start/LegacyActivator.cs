// WebActivator 1.x, the package before WebActivatorEx (Ninject.MVC3 2.x's App_Start template uses it; Subtext): its
// pre-start and post-start methods must run, as webForCore.WebActivator's do.
[assembly: WebActivator.PreApplicationStartMethod(typeof(SmokeMvc.LegacyActivator), "Start")]
[assembly: WebActivator.PostApplicationStartMethod(typeof(SmokeMvc.LegacyActivator), "Started")]

namespace SmokeMvc
{
    public static class LegacyActivator
    {
        public static bool PreStartRan;
        public static bool PostStartRan;

        public static void Start() => PreStartRan = true;

        public static void Started() => PostStartRan = true;
    }
}
