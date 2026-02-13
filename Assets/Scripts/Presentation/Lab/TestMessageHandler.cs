using UnityEngine;
using Domain;
using System.Collections.Generic;

namespace Presentation.Lab
{

    public class TestMessageHandler
    {

        private string litmusMessagePattern =
                "O composto é <b>{0}</b> e <b>{1}</b> no <b>{2}</b> e fica <b>{3}</b>";
        private string defaultMessagePattern =
                "O composto é <b>{0}</b> e <b>{1}</b> no <b>{2}</b>";

        private Dictionary<string, string> GetMixtureProperties(SolubilityOutcome outcome)
        {
            string solventName = outcome.Solvent.Name;

            string compoundState;
            switch (outcome.Compound.State)
            {
                case PhysicalState.LIQUID:
                    compoundState = "líquido";
                    break;
                case PhysicalState.SOLID:
                    compoundState = "sólido";
                    break;
                default:
                    compoundState = "desconhecido";
                    break;
            }

            string litmusText;
            switch (outcome.LitmusResult)
            {
                case LitmusResultKind.Acidic:
                    litmusText = "vermelho";
                    break;
                case LitmusResultKind.Basic:
                    litmusText = "azul";
                    break;
                default:
                    litmusText = "incolor";
                    break;
            }

            string solubilityText;
            switch (outcome.SolubilityResult)
            {
                case SolubilityResultKind.Soluble:
                    solubilityText = "solúvel";
                    break;
                case SolubilityResultKind.InsolubleFloat:
                    solubilityText = "boia";
                    break;
                case SolubilityResultKind.InsolubleSink:
                    solubilityText = "afunda";
                    break;
                default:
                    solubilityText = "desconhecido";
                    break;
            }

            return new Dictionary<string, string>()
            {
                ["solventName"] = solventName,
                ["compoundState"] = compoundState,
                ["litmusText"] = litmusText,
                ["solubilityText"] = solubilityText
            };
        }

        public string GetHistoryMessage(MixtureHistoryEntry entry)
        {
            var properties = GetMixtureProperties(entry.Outcome);

            string solventName = properties["solventName"];
            string compoundState = properties["compoundState"];
            string litmusText = properties["litmusText"];
            string solubilityText = properties["solubilityText"];

            if (solventName == "Tornassol")
                return
                    $"{entry.Order}) " +
                        string.Format(litmusMessagePattern, compoundState, solubilityText, solventName, litmusText);

            return
                $"{entry.Order}) " +
                    string.Format(defaultMessagePattern, compoundState, solubilityText, solventName);
        }

        public string GetTestMessage(MixSolutionResponse response)
        {
            var properties = GetMixtureProperties(response.Outcome);

            string solventName = properties["solventName"];
            string compoundState = properties["compoundState"];
            string litmusText = properties["litmusText"];
            string solubilityText = properties["solubilityText"];

            if (solventName == "Tornassol")
                return
                    string.Format(litmusMessagePattern, compoundState, solubilityText, solventName, litmusText);

            return
                string.Format(defaultMessagePattern, compoundState, solubilityText, solventName);
        }
    }


}