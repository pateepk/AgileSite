using CMS.Scheduler;

namespace CMS.DancingGoat.Samples
{
    internal class EnableDataProtectionSampleTask : ITask
    {
        public string Execute(TaskInfo task)
        {
            DancingGoatSamplesModule.RegisterSamples();

            return null;
        }
    }
}
