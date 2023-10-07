import { AutomatizationBaseGeneric } from "../automatizationComponent";

export class ChangeDestinationRule extends AutomatizationBaseGeneric<ChangeDestinationBehaviour> {
}

interface ChangeDestinationBehaviour{
    DestinationId : string;
    Name: string;
}