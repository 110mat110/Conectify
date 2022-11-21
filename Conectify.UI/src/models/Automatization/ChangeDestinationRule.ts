import { AutomatizationBaseWithTarget } from "../automatizationComponent";

export class ChangeDestinationRule extends AutomatizationBaseWithTarget {

    destinationId: string = "";

    public getParametersJSon(): string {
        return "{\"DestinationId\" : \"" + this.destinationId + "\"}";
    };

    constructor(id: string, behaviourId: string, json: string) {
        super(id, behaviourId)
        try {
            let obj = JSON.parse(json);
            if (obj.destinationactuatorId) {
                this.destinationId = obj.destinationactuatorId;
            }
        }
        catch (e) {
            console.error(e);
        }
    }
}