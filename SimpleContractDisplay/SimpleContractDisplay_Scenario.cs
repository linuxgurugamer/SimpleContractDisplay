using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceTuxUtility;

#if true
namespace SimpleContractDisplay
{
    [KSPScenario(ScenarioCreationOptions.AddToAllGames, new GameScenes[]{GameScenes.FLIGHT,GameScenes.SPACECENTER,GameScenes.EDITOR,GameScenes.TRACKSTATION})]
    public class SimpleContractDisplay_Scenario : ScenarioModule
    {
        public SimpleContractDisplay_Scenario()
        {
        }

        public override void OnSave(ConfigNode node)
        {
            foreach (var contract in Settings.Instance.activeContracts)
            {
                if (contract.Value.manual)
                {
                    ConfigNode configFileNode = new ConfigNode(Settings.CONTRACT_NODENAME);
                    configFileNode.AddValue("manualTitle", contract.Value.manualTitle);
                    configFileNode.AddValue("manualContract", contract.Value.manualContract);
                    node.AddNode(configFileNode);
                }
            }

        }
        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
            foreach (var n in node.GetNodes(Settings.CONTRACT_NODENAME))
            {
                string manualTitle = n.SafeLoad("manualTitle", "");
                string manualContract = n.SafeLoad("manualContract", "");
                Settings.Instance.activeContracts.Add( Guid.NewGuid(), new Contract(manualTitle, manualContract));

            }
        }


    }
}
#endif