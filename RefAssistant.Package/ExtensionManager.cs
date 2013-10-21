using System;
using Lardite.RefAssistant.VsProxy;
using Lardite.RefAssistant.VsProxy.Projects;

namespace Lardite.RefAssistant
{
    using PoolAction = System.Func<object, object>;

    internal partial class ExtensionManager
    {
        private readonly IVsProjectFacade _vsFacade;
        private readonly PoolAction[] _actionsPool = new PoolAction[5];
                
        public ExtensionManager(IVsProjectFacade vsFacade)
        {
            ThrowUtils.ArgumentNull(() => vsFacade);

            _vsFacade = vsFacade;
            InitializeActions();
        }

        public void ProcessProject(IVsProjectExtended project)
        {
            ThrowUtils.ArgumentNull(() => project);

            throw new NotImplementedException();
        }

        #region Helpers


        #endregion
    }
}
