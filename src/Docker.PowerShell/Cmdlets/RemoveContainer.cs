using System.Management.Automation;
using Docker.DotNet;
using Docker.DotNet.Models;
using System.Threading.Tasks;
using Docker.PowerShell.Objects;

namespace Docker.PowerShell.Cmdlets
{
    [Cmdlet(VerbsCommon.Remove, "Container",
            SupportsShouldProcess = true,
            DefaultParameterSetName = CommonParameterSetNames.Default)]
    public class RemoveContainer : MultiContainerOperationCmdlet
    {
        #region Parameters

        /// <summary>
        /// Whether or not to force the removal of the container.
        /// </summary>
        [Parameter]
        public SwitchParameter Force { get; set; }

        #endregion

        #region Overrides

        protected override async Task ProcessRecordAsync()
        {
            foreach (var id in ParameterResolvers.GetContainerIds(Container, ContainerIdOrName))
            {
                try
                {
                    var container = await ContainerOperations.GetContainersByIdOrName(id, DkrClient);
                }
                catch
                {
                    // Does this module have any recommendations for catching errors ?
                }
                // Confirm the operation with the user first.
                // This is always false if the WhatIf parameter is set.
                
                if (! ShouldProcess(string.Format(CultureInfo.CurrentCulture, "{0} - {1}", container.Names, id)))
                {
                    continue;
                }

                // Make sure that the user really wants to remove the container
                if (! Force)
                {
                    string message = string.Format(CultureInfo.CurrentCulture, "Are you sure you wish to remove the container with name \"{0}\"?", container.Names);
                    if (! ShouldContinue(message, "Warning!", ref yesToAll, ref noToAll))
                    {
                        continue;
                    }
                }
                await DkrClient.Containers.RemoveContainerAsync(id,
                    new ContainerRemoveParameters() { Force = Force.ToBool() }
                    );
            }
        }

        #endregion

        #region Private Data
        private bool yesToAll, noToAll;
        #endregion Private Data
    }
}
