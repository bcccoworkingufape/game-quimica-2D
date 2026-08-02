using UnityEngine;
using Domain;
using System.Collections.Generic;

namespace Presentation.Lab
{
    public class TestMessageHandler
    {
        private string defaultMessagePattern =
            "O composto é <b>{0}</b>{1} {2}";
        private string litmusMessagePattern =
            "O composto é <b>{0}</b> {1}";

        private Dictionary<string, string> MapOutcome(SolubilityOutcome outcome)
        {
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

            string solubilityText;
            switch (outcome.SolubilityResult)
            {
                case SolubilityResultKind.Soluble:
                    solubilityText = " e <b>solúvel</b> em";
                    break;
                case SolubilityResultKind.InsolubleFloat:
                    solubilityText = ", <b>insolúvel</b> e <b>menos denso</b> do que";
                    break;
                case SolubilityResultKind.InsolubleSink:
                    solubilityText = ", <b>insolúvel</b> e <b>mais denso</b> do que";
                    break;
                default:
                    solubilityText = "desconhecido";
                    break;
            }

            string solventName = outcome.Solvent.Name;
            switch (outcome.Solvent.ChemicalClass)
            {
                case "solvent":
                    solventName = "<b>" + solventName + "</b>";
                    break;
                case "solution":
                    solventName = "solução de <b>" + solventName + "</b>";
                    break;
                default:
                    solventName = "<b>" + solventName + "</b>";
                    break;
            }

            string litmusText;
            switch (outcome.LitmusResult)
            {
                case LitmusResultKind.Acidic:
                    litmusText = "e altera a cor do tornassol para <b>vermelho</b>";
                    break;
                case LitmusResultKind.Basic:
                    litmusText = "e altera a cor do tornassol para <b>azul</b>";
                    break;
                default:
                    litmusText = "e <b>não altera a cor</b> do tornassol";
                    break;
            }

            return new Dictionary<string, string>()
            {
                ["solventName"] = solventName,
                ["compoundState"] = compoundState,
                ["litmusText"] = litmusText,
                ["solubilityText"] = solubilityText,
            };
        }

        private string BuildMessage(SolubilityOutcome outcome, string orderPrefix = "")
        {
            var properties = MapOutcome(outcome);

            bool isLitmus = outcome.Solvent.Name == "Tornassol";
            string compoundState = properties["compoundState"];
            string litmusText = properties["litmusText"];
            string solubilityText = properties["solubilityText"];
            string solventName = properties["solventName"];

            string body;
            if (isLitmus)
                body = string.Format(litmusMessagePattern, compoundState, litmusText);
            else
                body = string.Format(defaultMessagePattern, compoundState, solubilityText, solventName);

            return orderPrefix + body;
        }

        public string GetHistoryMessage(MixtureHistoryEntry entry)
        {
            return BuildMessage(entry.Outcome, $"{entry.Order}) ");
        }

        public string GetTestMessage(MixSolutionResponse response)
        {
            return BuildMessage(response.Outcome);
        }
    }
}
