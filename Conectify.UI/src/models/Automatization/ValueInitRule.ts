import { AutomatizationBaseGeneric } from "../automatizationComponent";

export class ValueInitRule extends AutomatizationBaseGeneric<ValueInitBehaviour> {
}

interface ValueInitBehaviour{
    SourceSensorId: string;
    Name: string;
}