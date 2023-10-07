import { AutomatizationBaseGeneric } from "../automatizationComponent";

export class SetValueRule extends AutomatizationBaseGeneric<SetValueRuleBehaviour> {
}

interface SetValueRuleBehaviour{
    StringValue : string;
    Unit: string;
    NumericValue: number;
}