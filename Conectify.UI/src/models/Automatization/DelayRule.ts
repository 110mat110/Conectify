import { AutomatizationBaseGeneric } from "../automatizationComponent";

export class DelayRule extends AutomatizationBaseGeneric<DelayRuleBehaviour> {
}

interface DelayRuleBehaviour{
    Delay : string;
}