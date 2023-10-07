import { AutomatizationBaseGeneric } from "../automatizationComponent";

export class SetTimeRule extends AutomatizationBaseGeneric<SetTimeRuleBehaviour> {
}

interface SetTimeRuleBehaviour{
    TimeSet : string;
    Days: string;
    Name: string;
}