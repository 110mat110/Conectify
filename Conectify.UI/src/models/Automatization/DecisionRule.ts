import { AutomatizationBaseGeneric } from "../automatizationComponent";

export class DecisionRule extends AutomatizationBaseGeneric<DecisionRuleBehaviour> {
}

interface DecisionRuleBehaviour{
    Rule : string;
}